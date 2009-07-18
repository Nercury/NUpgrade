using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNUpgrade.UpgradeGen;
using NHibernate.Dialect;

namespace FluentNUpgrade
{
    public class SchemaUpgrade
    {
        public Dialect Dialect { get; private set; }

        public SchemaUpgrade(Dialect dialect)
        {
            this.Dialect = dialect;
        }

        private readonly Dictionary<string, EntityUpgradeActions> entities = new Dictionary<string, EntityUpgradeActions>();

        public SchemaUpgrade ForEntity(string name, Action<EntityUpgradeActions> entitySetup)
        {
            EntityUpgradeActions actions;
            if (!entities.TryGetValue(name, out actions))
            {
                actions = new EntityUpgradeActions(name, Dialect);
                entities[name] = actions;
            }
            entitySetup(actions);

            return this;
        }

        public SchemaUpgrade OutputScripts()
        {
            foreach (var item in entities)
            {
                foreach (var script in item.Value.GenerateScripts())
                {
                    Console.WriteLine(script);
                }
            }
            return this;
        }

        public void Execute()
        {

        }
    }
}
