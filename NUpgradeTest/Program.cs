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
            var upgrader = new NUpgrader<int, object>()
                .Add(1, 2, scope =>
                {
                    throw new Exception("aaaaaa");
                    //scope.PostMessage(new UpgradeMessage(scope.VersionScope.ToString(), UpgradeMessageType.Info));
                })
                .Add(1, 3, scope =>
                {
                    //scope.PostMessage(new UpgradeMessage(scope.VersionScope.ToString(), UpgradeMessageType.Info));
                })
                .Add(2, 3, scope =>
                {
                    //scope.PostMessage(new UpgradeMessage(scope.VersionScope.ToString(), UpgradeMessageType.Info));
                })
                .Listen(msg =>
                {
                    Console.WriteLine(msg.ToString());
                });

            var methods = upgrader.FindUpgradeSteps(1, 3);
            upgrader.Execute(methods);

            Console.ReadKey();
        }
    }
}
