using System;
using System.Collections.Generic;
using System.Text;

namespace NUpgrade
{
    public interface IUpgradeScope<VersionT>
    {
        IUpgradeScope<VersionT> Listen(Action<UpgradeMessage> messageListener);
        void PostMessage(UpgradeMessage message);
        void PushVersionScope(VersionScope<VersionT> versionScope);
        VersionScope<VersionT> PopVersionScope();
        VersionScope<VersionT> VersionScope { get; }
    }
}
