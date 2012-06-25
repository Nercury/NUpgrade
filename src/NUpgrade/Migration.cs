using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NUpgrade
{
    /// <summary>
    /// Resolves database upgrade paths and runs upgrade.
    /// </summary>
    public class Migration
    {
        /// <summary>
        /// Currently required database version.
        /// </summary>
        private int requiredDbVersion;

        /// <summary>
        /// A interface for inspecting and modifying schema
        /// </summary>
        private IDbInformationDriver schema;

        /// <summary>
        /// Collection of all upgrade paths.
        /// </summary>
        Dictionary<int,
            Dictionary<int,
                Action<IDbInformationDriver>
            >
        > paths = new Dictionary<int, Dictionary<int, Action<IDbInformationDriver>>>();

        public Migration(IDbInformationDriver schema, int requiredDbVersion)
        {
            this.requiredDbVersion = requiredDbVersion;
            this.schema = schema;
        }

        /// <summary>
        /// Check if current database is up to date
        /// </summary>
        public bool IsUpToDate
        {
            get
            {
                // if versions are equal, everything is up to date!
                return this.CurrentDbVersion == this.RequiredDbVersion;
            }
        }

        /// <summary>
        /// Get current db version
        /// </summary>
        public int CurrentDbVersion
        {
            get
            {
                return schema.Version;
            }
        }

        /// <summary>
        /// Get required db version
        /// </summary>
        public int RequiredDbVersion
        {
            get
            {
                return this.requiredDbVersion;
            }
        }

        /// <summary>
        /// Add upgrade method to upgrader
        /// </summary>
        /// <param name="from">From which version this method can upgrade</param>
        /// <param name="to">To which version this method can upgrade</param>
        /// <param name="upgradeAction">Action to run when upgrading</param>
        /// <returns></returns>
        public Migration Add(int from, int to, Action<IDbInformationDriver> upgradeAction)
        {
            Dictionary<int, Action<IDbInformationDriver>> fromMap;
            if (!paths.TryGetValue(from, out fromMap))
            {
                fromMap = new Dictionary<int, Action<IDbInformationDriver>>();
                paths[from] = fromMap;
            }
            fromMap[to] = upgradeAction;
            return this;
        }

        public IEnumerable<UpgradeStep> FindUpgradeSteps()
        {
            return this.FindUpgradeSteps(this.CurrentDbVersion, this.RequiredDbVersion);
        }

        /// <summary>
        /// Calculate upgrade steps needed to upgrade over specified versions
        /// </summary>
        /// <param name="fromVersion">From which version to upgrade</param>
        /// <param name="toVersion">Target version</param>
        /// <returns>List of upgrade steps, containing information about versions to migrate</returns>
        public IEnumerable<UpgradeStep> FindUpgradeSteps(int fromVersion, int toVersion)
        {
            // upgrade step contains method info (version - from, to), and method to use to launch this step
            var methods = new LinkedList<UpgradeStep>();
            // first find "from" version
            int currentVersion = fromVersion;
            while (currentVersion.CompareTo(toVersion) < 0)
            {
                // variants available from this version
                Dictionary<int, Action<IDbInformationDriver>> variants;
                if (paths.TryGetValue(currentVersion, out variants))
                {
                    if (variants.ContainsKey(toVersion)) // we can upgrade directly
                    {
                        methods.AddLast(new UpgradeStep(currentVersion, toVersion));
                        break;
                    }
                    else
                    {
                        // take latest possible upgrade from list
                        int lastVersion = 0;
                        bool versionFound = false;

                        bool first = true;
                        foreach (var kv in variants)
                        {
                            if (kv.Key.CompareTo(toVersion) > 0)
                                break;

                            versionFound = true;
                            lastVersion = kv.Key;

                            if (first)
                                first = false;
                        }

                        if (first) // value was not assigned
                        {
                            return null;
                        }
                        else
                        {
                            if (versionFound)
                            {
                                methods.AddLast(new UpgradeStep(currentVersion, lastVersion));
                                currentVersion = lastVersion;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                else
                {
                    return null;
                }
            }

            if (methods.Count == 0)
            {
                return null;
            }

            return methods;
        }

        /// <summary>
        /// Attempts to find a single upgrade action which migrates to specified version
        /// </summary>
        /// <param name="from">Version to migrate from</param>
        /// <param name="to">Version to migrate to</param>
        /// <returns>An action to run</returns>
        public Action<IDbInformationDriver> FindUpgradeAction(int from, int to)
        {
            Dictionary<int, Action<IDbInformationDriver>> fromMap;
            if (paths.TryGetValue(from, out fromMap))
            {
                Action<IDbInformationDriver> result;
                if (fromMap.TryGetValue(to, out result))
                {
                    return result;
                }
                throw new MigrationException("Unable to find migration step from version " + from + " to version " + to + ".");
            }
            throw new MigrationException("Unable to find migration step from version " + from + ".");
        }

        public bool Execute(IEnumerable<UpgradeStep> steps)
        {
            var hasAny = false;

            // print upgrade path message
            string upgradePathStr = "";
            foreach (var step in steps)
            {
                hasAny = true;

                if (upgradePathStr == "")
                {
                    upgradePathStr = step.From + " -> " + step.To;
                }
                else
                {
                    upgradePathStr += " -> " + step.To;
                }
            }

            if (hasAny)
            {
                Debug.WriteLine("DB MIGRATION: Will use this upgrade path: " + upgradePathStr);

                foreach (var step in steps)
                {
                    // execute each migration step in transaction
                    // in case exception occurs, always roll back transaction to leave user data consistent

                    Debug.WriteLine("DB MIGRATION: Migrating from " + step.From + " to " + step.To);

                    this.schema.BeginTransaction();
                    try
                    {
                        var action = this.FindUpgradeAction(step.From, step.To);
                        action(this.schema); // run action

                        this.schema.CommitTransaction();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("DB MIGRATION ERROR: " + e.ToString());
                        Debug.WriteLine(e.StackTrace.ToString());
                        this.schema.RollbackTransaction();
                        throw e;
                    }

                    // update current version
                    schema.Version = step.To;
                }

                Debug.WriteLine("DB MIGRATION: Done.");
            }

            return true;
        }
    }
}
