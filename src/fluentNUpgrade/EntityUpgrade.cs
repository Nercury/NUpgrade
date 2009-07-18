using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade
{
    public class EntityUpgrade
    {
        public EntityUpgrade Add<FromT, ToT>()
        {
            return this;
        }

        public EntityUpgrade OutputScripts()
        {
            return this;
        }

        public void Execute()
        {

        }
    }
}
