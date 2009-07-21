using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade.UpgradeGen
{
    public class ColumnUpgradeActions
    {
        public Type ColumnType { get; private set; }
        public string Name { get; private set; }

        public ColumnUpgradeActions(EntityUpgradeActions parentEntity)
        {
            this.ColumnType = null; // unknown
        }

        public ColumnUpgradeActions(Type columnType)
        {
            this.ColumnType = columnType;
        }

        public ColumnUpgradeActions AddIndex()
        {
            throw new NotImplementedException();
        }

        public ColumnUpgradeActions RemoveIndex()
        {
            throw new NotImplementedException();
        }

        public ColumnUpgradeActions AddIndex(string p)
        {
            throw new NotImplementedException();
        }

        public ColumnUpgradeActions RemoveIndex(string p)
        {
            throw new NotImplementedException();
        }
    }
}
