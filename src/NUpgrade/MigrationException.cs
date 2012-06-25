using System;
using System.Collections.Generic;
using System.Text;

namespace NUpgrade
{
    public class MigrationException : Exception
    {
        public MigrationException(string message) : base(message)
        {
        }
    }
}
