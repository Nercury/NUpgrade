using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNUpgrade.UpgradeGen.Exceptions;

namespace FluentNUpgrade.UpgradeGen
{
    public class EntityUpgradeActions
    {
        private Dictionary<string, ColumnUpgradeActions> addedColumns = new Dictionary<string, ColumnUpgradeActions>();
        private Dictionary<string, bool> removedColumns = new Dictionary<string, bool>();

        private ColumnUpgradeActions RegisterColumnAdd(string name)
        {
            ColumnUpgradeActions actions;
            if (addedColumns.TryGetValue(name, out actions))
            {
                throw new ColumnAlreadyAddedException("Column with name [" + name + "] already added to entity.");
            }
            else
            {
                removedColumns.Remove(name);

                actions = new ColumnUpgradeActions();
                addedColumns[name] = actions;
            }
            return actions;
        }

        private void RegisterColumnRemove(string name)
        {
            if (removedColumns.ContainsKey(name))
            {
                throw new ColumnAlreadyRemovedException("Column with name [" + name + "] already removed from entity.");
            }
            else
            {
                addedColumns.Remove(name);

                removedColumns[name] = true; // boolean value does not mean anything here
            }
        }

        public EntityUpgradeActions AddColumn<ColumnT>(string name)
        {
            RegisterColumnAdd(name);
            return this;
        }

        public EntityUpgradeActions AddColumn<ColumnT>(string name, Action<ColumnUpgradeActions> columnSetup)
        {
            columnSetup(RegisterColumnAdd(name));
            return this;
        }

        public EntityUpgradeActions RemoveColumn<ColumnT>(string name)
        {
            RegisterColumnRemove(name);
            return this;
        }

        public EntityUpgradeActions ModifyColumn<ColumnT>(string name, Action<ColumnUpgradeActions> columnSetup)
        {
            ColumnUpgradeActions actions;
            if (!addedColumns.TryGetValue(name, out actions))
            {
                throw new ColumnNotFoundException("Column with name [" + name + "] was not found in entity.");
            }
            else
            {
                columnSetup(actions);
            }
            return this;
        }
    }
}
