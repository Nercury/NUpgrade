using System;

namespace NUpgrade
{
    public class UpgradeStep<VersionT, UpgradeScopeT> where VersionT : IComparable<VersionT>
    {
        public UpgradeStep(UpgradePathMethodInfo<VersionT, UpgradeScopeT> methodInfo, Action<UpgradeScopeT> methodDelegate)
        {
            this.MethodInfo = methodInfo;
            this.MethodDelegate = methodDelegate;
        }

        public UpgradePathMethodInfo<VersionT, UpgradeScopeT> MethodInfo { get; private set; }
        public Action<UpgradeScopeT> MethodDelegate { get; private set; }
    }
}
