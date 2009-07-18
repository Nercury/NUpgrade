using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using FluentNUpgrade;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using NUpgrade;

namespace NUpgradeTest
{
    public class Employee
    {
        public virtual int Id { get; private set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
    }

    [SkipMapping]
    public class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            WithTable("blink_employee");
            Id(x => x.Id);
            Map(x => x.FirstName);
            Map(x => x.LastName);
        }
    }

    public class Employee_2
    {
        public virtual int Id { get; private set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
    }

    
    public class EmployeeMap_2 : ClassMap<Employee_2>
    {
        public EmployeeMap_2()
        {
            WithTable("blink_employee_2");
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
                    scope.PostMessage(new UpgradeMessage("Doing something C...", UpgradeMessageType.Info));
                })
                .Add(4, 5, scope =>
                {
                    var cmd = new NpgsqlCommand(@"create table blink_employee2 (
  Id  serial,
   FirstName varchar(255),
   LastName varchar(255),
   primary key (Id)
)", (NpgsqlConnection)scope.Session.Connection);
                    scope.Transaction.Enlist(cmd);
                    cmd.ExecuteNonQuery();

                    throw new Exception("Some error.");

                    scope.PostMessage(new UpgradeMessage("Doing something B...", UpgradeMessageType.Info));
                })
                .Add(1, 4, scope =>
                {
                    scope.PostMessage(new UpgradeMessage("Doing something A...", UpgradeMessageType.Info));
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
