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

namespace NUpgradeTest
{
    public class Employee
    {
        public virtual int Id { get; private set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
    }

    public class EmployeeMap : ClassMap<Employee>, IUpgradedFrom<EmployeeMap_0>
    {
        public EmployeeMap()
        {
            WithTable("blink_employee_1");
            Id(x => x.Id);
            Map(x => x.FirstName);
            Map(x => x.MiddleName);
            Map(x => x.LastName);
        }

        #region IUpgradedFrom<EmployeeMap_0> Members

        public void InitUpgradeMap(UpgradeMap upgrade)
        {
            upgrade
                .AddColumn("MiddleName", typeof(string))
                .TableRename("blink_employee_0", "blink_employee_1");
        }

        #endregion
    }

    public class Employee_0
    {
        public virtual int Id { get; private set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }

    [SkipMapping]
    public class EmployeeMap_0 : ClassMap<Employee_0>
    {
        public EmployeeMap_0()
        {
            WithTable("blink_employee_0");
            Id(x => x.Id);
            Map(x => x.FirstName);
            Map(x => x.LastName);
        }
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
                        .Add<EmployeeMap_0, EmployeeMap>()
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
                    m.FluentMappings.AddFromAssemblyOf<Program>();
                })
                .ExposeConfiguration(config =>
                {
                    new SchemaExport(config).Create(true, true);
                })
                .BuildSessionFactory();
        }
    }
}
