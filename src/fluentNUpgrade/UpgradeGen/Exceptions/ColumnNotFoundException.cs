using System;
using FluentNUpgrade.Exceptions;

namespace FluentNUpgrade.UpgradeGen.Exceptions
{
    public class ColumnNotFoundException : NUpgradeException
    {
        public ColumnNotFoundException() : base() { }
        public ColumnNotFoundException(string message) : base(message) { }
        public ColumnNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
