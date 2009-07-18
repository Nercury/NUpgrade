
namespace NUpgrade
{
    /// <summary>
    /// Suggested interface to use for upgrade scope
    /// </summary>
    public interface IUpgradeScope
    {
        void PostMessage(UpgradeMessage message);
    }
}
