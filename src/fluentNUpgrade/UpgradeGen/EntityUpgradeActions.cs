using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade.UpgradeGen
{
    public class EntityUpgradeActions
    {
        public EntityUpgradeActions AddColumn<ColumnT>(string name)
        {
            return this;
        }
    }
}
