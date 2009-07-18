using System;
using System.Collections.Generic;
using NUpgrade;
using NHibernate;

namespace FluentNUpgrade
{
    /// <summary>
    /// Upgrader for FluentNHibernate
    /// </summary>
    /// <typeparam name="VersionT">Type used for version tracking</typeparam>
    public class FluentNUpgrader<VersionT> : 
        BasicUpgrader<VersionT, NUpgradeScope<VersionT>> where VersionT : IComparable<VersionT>
    {
        public ISessionFactory SessionFactory { get; private set; }

        public FluentNUpgrader(ISessionFactory sessionFactory)
        {
            this.SessionFactory = sessionFactory;
        }

        public override bool Execute(IEnumerable<UpgradeStep<VersionT, NUpgradeScope<VersionT>>> upgradeSteps)
        {
            return Execute(upgradeSteps, step =>
            {
                using (var session = SessionFactory.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        try
                        {
                            PostMessage(new UpgradeMessage("Upgrading from " + step.MethodInfo.From + " to " + step.MethodInfo.To + "...", UpgradeMessageType.Info));

                            step.MethodDelegate(new NUpgradeScope<VersionT>(this, session, transaction));

                            transaction.Commit();

                            PostMessage(new UpgradeMessage("Success!", UpgradeMessageType.Info));
                            return true;
                        }
                        catch (Exception e)
                        {
                            PostMessage(new UpgradeMessage("Error when upgrading from version " + step.MethodInfo.From + " to " + step.MethodInfo.To + ": " + e.ToString(), UpgradeMessageType.Error));
                            transaction.Rollback();
                            PostMessage(new UpgradeMessage("ALL ACTIONS ROLLED BACK.", UpgradeMessageType.Error));
                            return false;
                        }
                    }
                }
            });
        }
    }
}
