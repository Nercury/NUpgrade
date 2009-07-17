using System;
using System.Collections.Generic;
using System.Text;

namespace NUpgrade
{
    public class VersionScope<VersionT>
    {
        public VersionScope(VersionT fromVersion, VersionT toVersion)
        {
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
        }

        public VersionT FromVersion { get; private set; }
        public VersionT ToVersion { get; private set; }

        public override string ToString()
        {
            return new StringBuilder("[from ")
                .Append(FromVersion)
                .Append(" to ")
                .Append(ToVersion)
                .Append("]")
                .ToString();
        }
    }
}
