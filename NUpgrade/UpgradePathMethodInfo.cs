using System;
using System.Collections.Generic;
using System.Text;

namespace NUpgrade
{
    public class UpgradePathMethodInfo<VersionT>
    {
        public UpgradePathMethodInfo(Action<IUpgradeScope<VersionT>> upgradeDelegate, VersionT versionFrom, VersionT versionTo)
        {
            this.upgradeDelegate = upgradeDelegate;
            this.From = versionFrom;
            this.To = versionTo;
        }

        public IUpgradeScope<VersionT> Scope { get; set; }
        public void Run()
        {
            upgradeDelegate(Scope);
        }
        private Action<IUpgradeScope<VersionT>> upgradeDelegate;
        public VersionT From { get; private set; }
        public VersionT To { get; private set; }
    }
}
