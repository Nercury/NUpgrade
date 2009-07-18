using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNUpgrade.UpgradeGen;

namespace FluentNUpgrade
{
    public class SchemaUpgrade
    {
        private readonly Dictionary<string, EntityUpgradeActions> entities = new Dictionary<string, EntityUpgradeActions>();

        public SchemaUpgrade ForEntity(string name, Action<EntityUpgradeActions> entitySetup)
        {
            EntityUpgradeActions actions;
            if (!entities.TryGetValue(name, out actions))
            {
                actions = new EntityUpgradeActions();
                entities[name] = actions;
            }
            entitySetup(actions);

            return this;
        }

        public SchemaUpgrade OutputScripts()
        {
            return this;
        }

        public void Execute()
        {

        }
    }
}
