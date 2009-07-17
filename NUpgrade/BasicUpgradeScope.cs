using System;
using System.Collections.Generic;
using System.Text;

namespace NUpgrade
{
    public class BasicUpgradeScope<VersionT> : IUpgradeScope<VersionT>
    {
        private Stack<VersionScope<VersionT>> versionStack = new Stack<VersionScope<VersionT>>();
        private List<Action<UpgradeMessage>> messageListeners = new List<Action<UpgradeMessage>>();

        public IUpgradeScope<VersionT> Listen(Action<UpgradeMessage> messageListener)
        {
            messageListeners.Add(messageListener);

            return this;
        }

        public void PostMessage(UpgradeMessage message)
        {
            messageListeners.ForEach(a => { a(message); });
        }

        public void PushVersionScope(VersionScope<VersionT> versionScope)
        {
            versionStack.Push(versionScope);
        }

        public VersionScope<VersionT> PopVersionScope()
        {
            return versionStack.Pop();
        }

        public VersionScope<VersionT> VersionScope
        {
            get { return this.versionStack.Peek(); }
        }
    }
}
