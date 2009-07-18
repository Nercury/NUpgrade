using System;
using FluentNUpgrade.Exceptions;

namespace FluentNUpgrade.UpgradeGen.Exceptions
{
    public class ColumnAlreadyAddedException : NUpgradeException
    {
        public ColumnAlreadyAddedException() : base() { }
        public ColumnAlreadyAddedException(string message) : base(message) { }
        public ColumnAlreadyAddedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
