
namespace NUpgrade
{
    public class UpgradeStep
    {
        public int From { get; private set; }
        public int To { get; private set; }

        public UpgradeStep(int from, int to)
        {
            this.From = from;
            this.To = to;
        }
    }
}
