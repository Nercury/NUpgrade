using System;
using System.Collections.Generic;
using System.Linq;

namespace NUpgrade
{
    public class NUpgrader<VersionT, UpgradeScopeT> where VersionT : IComparable<VersionT>
    {
        /// <summary>
        /// Place to store all upgrade paths. In first dictionary - all "from" values, in second disctionary - all "to" values and their methods
        /// </summary>
        protected 
        Dictionary<VersionT, 
            Dictionary<VersionT, 
                Action<UpgradeScopeT>
            >
        > paths = new Dictionary<VersionT, Dictionary<VersionT, Action<UpgradeScopeT>>>();

        /// <summary>
        /// Add upgrade method to upgrader
        /// </summary>
        /// <param name="from">From which version this method can upgrade</param>
        /// <param name="to">To which version this method can upgrade</param>
        /// <param name="upgradeAction">Action to run when upgrading</param>
        /// <returns></returns>
        public NUpgrader<VersionT, UpgradeScopeT> Add(VersionT from, VersionT to, Action<UpgradeScopeT> upgradeAction)
        {
            Dictionary<VersionT, Action<UpgradeScopeT>> fromMap;
            if (!paths.TryGetValue(from, out fromMap))
            {
                fromMap = new Dictionary<VersionT, Action<UpgradeScopeT>>();
                paths[from] = fromMap;
            }
            fromMap[to] = upgradeAction;

            return this;
        }

        /// <summary>
        /// List of delegates which are lsitening to messages
        /// </summary>
        protected List<Action<UpgradeMessage>> messageListeners = new List<Action<UpgradeMessage>>();

        /// <summary>
        /// Add message listener
        /// </summary>
        /// <param name="messageListener"></param>
        /// <returns></returns>
        public virtual NUpgrader<VersionT, UpgradeScopeT> Listen(Action<UpgradeMessage> messageListener)
        {
            messageListeners.Add(messageListener);

            return this;
        }

        /// <summary>
        /// Post message to message listeners
        /// </summary>
        /// <param name="message"></param>
        public virtual void PostMessage(UpgradeMessage message)
        {
            messageListeners.ForEach(a => { a(message); });
        }

        /// <summary>
        /// Calculate upgrade steps needed to upgrade over specified versions
        /// </summary>
        /// <param name="fromVersion">From which version to upgrade</param>
        /// <param name="toVersion">Target version</param>
        /// <returns>List of upgrade steps, containing information about versions and method to execute</returns>
        public virtual IEnumerable<UpgradeStep<VersionT, UpgradeScopeT>> FindUpgradeSteps(VersionT fromVersion, VersionT toVersion)
        {
            // upgrade step contains method info (version - from, to), and method to use to launch this step
            var methods = new LinkedList<UpgradeStep<VersionT, UpgradeScopeT>>();
            // first find "from" version
            VersionT currentVersion = fromVersion;
            while (currentVersion.CompareTo(toVersion) < 0)
            {
                // variants available from this version
                Dictionary<VersionT, Action<UpgradeScopeT>> variants;
                if (paths.TryGetValue(currentVersion, out variants))
                {
                    if (variants.ContainsKey(toVersion)) // we can upgrade directly
                    {
                        methods.AddLast(new UpgradeStep<VersionT, UpgradeScopeT>(
                            new UpgradePathMethodInfo<VersionT, UpgradeScopeT>(this, currentVersion, toVersion),
                            variants[toVersion]));
                        break;
                    }
                    else
                    {
                        // take latest possible upgrade from list
                        VersionT lastVersion = default(VersionT);
                        bool versionFound = false;
                        Action<UpgradeScopeT> lastDelegate = null;

                        bool first = true;
                        foreach (var kv in variants)
                        {
                            if (kv.Key.CompareTo(toVersion) > 0)
                                break;

                            versionFound = true;
                            lastVersion = kv.Key;
                            lastDelegate = kv.Value;

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
                                methods.AddLast(new UpgradeStep<VersionT, UpgradeScopeT>(
                                    new UpgradePathMethodInfo<VersionT, UpgradeScopeT>(this, currentVersion, toVersion),
                                    lastDelegate));
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
            return methods;
        }

        /// <summary>
        /// Execute upgrade steps with default step loading method
        /// </summary>
        /// <param name="upgradeSteps">Upgrade steps to execute</param>
        /// <returns>True is executed successfully, false if not.</returns>
        public virtual bool Execute(IEnumerable<UpgradeStep<VersionT, UpgradeScopeT>> upgradeSteps)
        {
            return Execute(upgradeSteps, step =>
                {
                    try
                    {
                        step.MethodDelegate(default(UpgradeScopeT));
                        PostMessage(new UpgradeMessage("Success!", UpgradeMessageType.Info));
                        return true;
                    }
                    catch (Exception e)
                    {
                        PostMessage(new UpgradeMessage("Error when upgrading from version " + step.MethodInfo.From + " to " + step.MethodInfo.To + ": " + e.ToString(), UpgradeMessageType.Error));
                        return false;
                    }
                });
        }

        /// <summary>
        /// Execute upgrade steps with custom step loading method
        /// </summary>
        /// <param name="upgradeSteps">Upgrade steps to execute</param>
        /// <param name="stepLoadFunction">Function which starts upgrade delegate with specified UpgradeScopeT type.</param>
        /// <returns>True is executed successfully, false if not.</returns>
        public virtual bool Execute(IEnumerable<UpgradeStep<VersionT, UpgradeScopeT>> upgradeSteps, Func<UpgradeStep<VersionT, UpgradeScopeT>, bool> stepLoadFunction)
        {
            if (upgradeSteps == null)
                return false;

            var steps = upgradeSteps.ToArray();

            if (steps.Length > 0)
            {
                // print upgrade path message

                string upgradePathStr = "";
                foreach (var step in steps)
                {
                    if (upgradePathStr == "")
                    {
                        upgradePathStr = step.MethodInfo.From + " -> " + step.MethodInfo.To;
                    }
                    else
                    {
                        upgradePathStr += " -> " + step.MethodInfo.To;
                    }
                }

                PostMessage(new UpgradeMessage("Will use this upgrade path: " + upgradePathStr, UpgradeMessageType.Info));

                for (var i = 0; i < steps.Length; i++)
                {
                    var step = steps[i];

                    bool success = stepLoadFunction(step);

                    if (!success)
                        return false;
                }
            }

            return true;
        }
    }
}
