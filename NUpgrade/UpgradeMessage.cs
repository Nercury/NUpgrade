using System.Text;

namespace NUpgrade
{
    /// <summary>
    /// Upgrade message container class
    /// </summary>
    public class UpgradeMessage
    {
        /// <summary>
        /// Message contents
        /// </summary>
        public string Message;
        /// <summary>
        /// Message type
        /// </summary>
        public UpgradeMessageType Type;

        /// <summary>
        /// Create new upgrade message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public UpgradeMessage(string message, UpgradeMessageType type)
        {
            this.Message = message;
            this.Type = type;
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append(Type == UpgradeMessageType.Error ? "[error]" : "[info]")
                .Append(" ")
                .Append(Message)
                .ToString();
        }
    }
}
