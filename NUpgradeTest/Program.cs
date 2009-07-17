using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using NUpgrade;

namespace NUpgradeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var upgrader = new NUpgrader<int, BasicUpgradeScope<int>>()
                .Add(1, 2, scope =>
                {
                    throw new Exception("aaaaaa");
                    scope.PostMessage(new UpgradeMessage(scope.VersionScope.ToString(), UpgradeMessageType.Info));
                })
                .Add(1, 3, scope =>
                {
                    scope.PostMessage(new UpgradeMessage(scope.VersionScope.ToString(), UpgradeMessageType.Info));
                })
                .Add(2, 3, scope =>
                {
                    scope.PostMessage(new UpgradeMessage(scope.VersionScope.ToString(), UpgradeMessageType.Info));
                })
                .Listen(msg =>
                {
                    Console.WriteLine(msg.ToString());
                });

            var methods = upgrader.FindUpgradePath(1, 3);
            upgrader.Execute(methods, m =>
                {
                    try
                    {
                        m.Run();
                        return true;
                    }
                    catch (Exception e)
                    {
                        m.Scope.PostMessage(
                            new UpgradeMessage("Upgrade from version " + m.From + 
                                " to " + m.To + " failed with exception: " + e.ToString(), UpgradeMessageType.Error));
                        return false;
                    }
                });

            Console.ReadKey();
        }
    }
}
