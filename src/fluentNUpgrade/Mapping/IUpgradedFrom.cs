using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade.Mapping
{
    public interface IUpgradedFrom<T>
    {
        void InitUpgradeMap(UpgradeMap upgrade);
    }
}
