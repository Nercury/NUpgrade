using System;
using System.Collections.Generic;
using System.Text;

namespace NUpgrade
{
    public class NUpgrader<VersionT, UpgradeScopeT> where VersionT : IComparable<VersionT>
    {
        public NUpgrader()
        {
        }

        private Dictionary<VersionT, Dictionary<VersionT, Action<UpgradePathMethodInfo<VersionT>>>> paths = new Dictionary<VersionT, Dictionary<VersionT, Action<UpgradePathMethodInfo<VersionT>>>>();

        public NUpgrader<VersionT, UpgradeScopeT> Add(VersionT from, VersionT to, Action<UpgradePathMethodInfo<VersionT>> upgradeAction)
        {
            Dictionary<VersionT, Action<UpgradePathMethodInfo<VersionT>>> fromMap;
            if (!paths.TryGetValue(from, out fromMap))
            {
                fromMap = new Dictionary<VersionT, Action<UpgradePathMethodInfo<VersionT>>>();
                paths[from] = fromMap;
            }
            fromMap[to] = upgradeAction;

            return this;
        }

        private List<Action<UpgradeMessage>> messageListeners = new List<Action<UpgradeMessage>>();

        public NUpgrader<VersionT, UpgradeScopeT> Listen(Action<UpgradeMessage> messageListener)
        {
            messageListeners.Add(messageListener);

            return this;
        }

        public IEnumerable<UpgradePathMethodInfo<VersionT>> FindUpgradePath(VersionT fromVersion, VersionT toVersion)
        {
            // delegate, version from, version to
            var methods = new LinkedList<UpgradePathMethodInfo<VersionT>>();
            // visu pirma randam kelia kaip atnaujinti
            VersionT currentVersion = fromVersion;
            while (currentVersion.CompareTo(toVersion) < 0)
            {
                if (paths.ContainsKey(currentVersion))
                {
                    var variants = paths[currentVersion];
                    if (variants.ContainsKey(toVersion)) // we can upgrade directly
                    {
                        methods.AddLast(new UpgradePathMethodInfo<VersionT>(variants[toVersion], currentVersion, toVersion));
                        break;
                    }
                    else
                    {
                        // take latest possible upgrade from list
                        VersionT lastVersion = default(VersionT);
                        bool versionFound = false;
                        Action<IUpgradeScope<VersionT>> lastDelegate = null;

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
                                methods.AddLast(new UpgradePathMethodInfo<VersionT>(lastDelegate, currentVersion, lastVersion));
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

        public bool Execute(IEnumerable<UpgradePathMethodInfo<VersionT>> upgradeMethods, Func<UpgradePathMethodInfo<VersionT>, bool> upgradeLoadFunction)
        {
            if (upgradeMethods == null)
                return false;

            var methods = upgradeMethods.ToArray();

            if (methods.Length > 0)
            {
                string upgradePathStr = "";
                foreach (var method in methods)
                {
                    if (upgradePathStr == "")
                    {
                        upgradePathStr = method.From + " -> " + method.To;
                    }
                    else
                    {
                        upgradePathStr += " -> " + method.To;
                    }
                }

                upgradeScope.PostMessage(new UpgradeMessage("Will use this upgrade path: " + upgradePathStr, UpgradeMessageType.Info));

                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    method.Scope = upgradeScope;

                    upgradeScope.PushVersionScope(new VersionScope<VersionT>(method.From, method.To));

                    bool success = upgradeLoadFunction(method);

                    upgradeScope.PopVersionScope();

                    if (!success)
                        return false;
                }
            }

            return true;
        }
    }
}
