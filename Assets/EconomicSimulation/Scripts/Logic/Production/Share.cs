using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents record of ownership right
    /// </summary>
    public class Share
    {
        private readonly Money howMuchOwns;//default value
        private readonly Money howMuchWantsToSell = new Money(0m);

        public Share(MoneyView initialSumm)
        {
            howMuchOwns = initialSumm.Copy();
        }

        public void Increase(MoneyView sum)
        {
            howMuchOwns.Add(sum);
        }

        public void Decrease(MoneyView sum)
        {
            howMuchOwns.Subtract(sum);
        }

        public void CancelBuyOrder(MoneyView sum)
        {
            howMuchWantsToSell.Subtract(sum, false);
        }

        /// <summary>
        /// Only for read! Returns copy
        /// </summary>
        public MoneyView GetShare()
        {
            return howMuchOwns;
        }

        /// <summary>
        /// Only for read! Returns copy
        /// </summary>
        public MoneyView GetShareForSale()
        {
            return howMuchWantsToSell;
        }

        public void SetToSell(MoneyView sum)
        {
            if (howMuchOwns.Get() - howMuchWantsToSell.Get() - sum.Get() < 0m)
                howMuchWantsToSell.Set(howMuchOwns);
            else
                howMuchWantsToSell.Add(sum);
        }

        public void ReduceSale(MoneyView sum)
        {
            howMuchWantsToSell.Subtract(sum, false);
            if (howMuchWantsToSell.isBiggerThan(howMuchOwns))
                howMuchWantsToSell.Set(howMuchOwns);
        }

        public override string ToString()
        {
            return howMuchOwns.ToString();
        }
    }
}