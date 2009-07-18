using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using FluentNUpgrade;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using NUpgrade;
using FluentNUpgrade.Mapping;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions;
using FluentNHibernate.AutoMap;

namespace NUpgradeTest
{
    [CurrentVersion]
    public class Employee : IUpgradedFrom<Employee_0>
    {
        public virtual int Id { get; private set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }

        #region IUpgradedFrom<Employee_0> Members

        public virtual void InitUpgradeMap(UpgradeMap upgrade)
        {
            upgrade
                .AddColumn("MiddleName", typeof(string));
        }

        #endregion
    }

    public class Employee_0
    {
        public virtual int Id { get; private set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var upgrader = new FluentNUpgrader<int>(CreateSessionFactory())
                .Add(5, 6, scope =>
                {
                    scope.PostMessage(new UpgradeMessage("Doing nothing B...", UpgradeMessageType.Info));
                })
                .Add(4, 5, scope =>
                {
                    scope.PostMessage(new UpgradeMessage("Doing nothing A...", UpgradeMessageType.Info));
                })
                .Add(1, 4, scope =>
                {
                    scope.PostMessage(new UpgradeMessage("Startting Emplyee upgrade...", UpgradeMessageType.Info));

                    new EntityUpgrade()
                        .Add<Employee_0, Employee>()
                        .OutputScripts()
                        .Execute();

                    scope.PostMessage(new UpgradeMessage("Upgrade successfull.", UpgradeMessageType.Info));
                })
                .Listen(msg =>
                {
                    Console.WriteLine(msg.ToString());
                });

            upgrader.Execute(upgrader.FindUpgradeSteps(1, 6));

            Console.ReadKey();
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(
                    PostgreSQLConfiguration.PostgreSQL82
                        .ConnectionString(c => c
                            .FromConnectionStringWithKey("BlinkConnectionString"))
                )
                .Mappings(m =>
                {
                    m.AutoMappings.Add(() =>
                        {
                            var model = AutoPersistenceModel  
                                .MapEntitiesFromAssemblyOf<Employee>()
                                .Where(type =>
                                    {
                                        return type.GetCustomAttributes(typeof(CurrentVersionAttribute), false).Length > 0;
                                    });

                            return model;
                        });
                })
                .ExposeConfiguration(config =>
                {
                    new SchemaExport(config).Create(true, true);
                })
                .BuildSessionFactory();
        }
    }
}
