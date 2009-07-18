using System;
using FluentNUpgrade.Exceptions;

namespace FluentNUpgrade.UpgradeGen.Exceptions
{
    public class ColumnAlreadyRemovedException : NUpgradeException
    {
        public ColumnAlreadyRemovedException() : base() { }
        public ColumnAlreadyRemovedException(string message) : base(message) { }
        public ColumnAlreadyRemovedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
