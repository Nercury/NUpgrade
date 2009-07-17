using System;

namespace NUpgrade
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="UpgradeScopeT">Type of custom user object</typeparam>
    /// <typeparam name="VersionT">Type used for version tracking</typeparam>
    public class UpgradePathMethodInfo<VersionT, UpgradeScopeT> where VersionT : IComparable<VersionT>
    {
        public VersionT From { get; private set; }
        public VersionT To { get; private set; }

        private NUpgrader<VersionT, UpgradeScopeT> upgrader;

        public UpgradePathMethodInfo(NUpgrader<VersionT, UpgradeScopeT> upgrader, VersionT versionFrom, VersionT versionTo)
        {
            this.upgrader = upgrader;
            this.From = versionFrom;
            this.To = versionTo;
        }
    }
}
