using System;
using NUpgrade;
using NHibernate;

namespace FluentNUpgrade
{
    /// <summary>
    /// Scope object passed to all upgrade methods defined in upgrader.
    /// 
    /// Here we define things we want to be accesible in any upgrade method.
    /// </summary>
    /// <typeparam name="VersionT">Type used for version tracking</typeparam>
    public class NUpgradeScope<VersionT> : IUpgradeScope where VersionT : IComparable<VersionT>
    {
        public ISession Session { get; protected set; }
        public ITransaction Transaction { get; protected set; }
        protected BasicUpgrader<VersionT, NUpgradeScope<VersionT>> upgrader;

        public NUpgradeScope(BasicUpgrader<VersionT, NUpgradeScope<VersionT>> upgrader, 
            ISession session, ITransaction transaction)
        {
            this.Session = session;
            this.Transaction = transaction;
            this.upgrader = upgrader;
        }

        #region IUpgradeScope Members

        public virtual void PostMessage(UpgradeMessage message)
        {
            upgrader.PostMessage(message);
        }

        #endregion
    }
}
