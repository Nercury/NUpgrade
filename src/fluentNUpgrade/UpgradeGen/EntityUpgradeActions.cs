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

        public EntityUpgradeActions AddColumn<ColumnT>(string name)
        {
            ColumnUpgradeActions actions;
            if (addedColumns.TryGetValue(name, out actions))
            {
                throw new ColumnAlreadyAddedException("Column with name [" + name + "] already added to entity.");
            }
            else
            {
                if (removedColumns.ContainsKey(name))
                    removedColumns.Remove(name);

                actions = new ColumnUpgradeActions();
                addedColumns[name] = actions;
            }
            return this;
        }

        public EntityUpgradeActions AddColumn<ColumnT>(string name, Action<ColumnUpgradeActions> columnSetup)
        {
            return this;
        }

        public EntityUpgradeActions RemoveColumn<ColumnT>(string name)
        {
            return this;
        }

        public EntityUpgradeActions ModifyColumn<ColumnT>(string name, Action<ColumnUpgradeActions> columnSetup)
        {
            return this;
        }
    }
}
