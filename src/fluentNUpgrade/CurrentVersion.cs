using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CurrentVersionAttribute : Attribute
    {
    }
}
