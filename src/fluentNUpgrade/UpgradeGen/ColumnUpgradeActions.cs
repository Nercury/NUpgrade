using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade.UpgradeGen
{
    public class ColumnUpgradeActions
    {
        public Type ColumnType { get; private set; }

        public ColumnUpgradeActions()
        {
            this.ColumnType = null; // unknown
        }

        public ColumnUpgradeActions(Type columnType)
        {
            this.ColumnType = columnType;
        }
    }
}
