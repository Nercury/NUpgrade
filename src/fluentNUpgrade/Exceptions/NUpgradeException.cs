using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNUpgrade.Exceptions
{
    public class NUpgradeException : Exception
    {
        public NUpgradeException() : base("NUpgrade exception.") { }
        public NUpgradeException(string message) : base("NUpgrade exception: " + message) { }
        public NUpgradeException(string message, Exception innerException) : base("NUpgrade exception: " + message, innerException) { }
    }
}
