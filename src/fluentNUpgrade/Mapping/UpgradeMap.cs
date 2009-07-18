using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade.Mapping
{
    public class UpgradeMap
    {
        public UpgradeMap TableRename(string fromName, string toName)
        {
            return this;
        }

        public UpgradeMap Delete()
        {
            return this;
        }

        public UpgradeMap DeleteColumn(string columnName)
        {
            return this;
        }

        public UpgradeMap AddColumn(string columnName, Type columnType)
        {
            return this;
        }
    }
}
