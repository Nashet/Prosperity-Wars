using UnityEngine;

using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents record of ownership right
    /// </summary>
    public class Share
    {
        private readonly Value howMuchOwns;//default value
        private readonly Value howMuchWantsToSell = new Value(0f);
        public Share(Value initialSumm)
        {
            howMuchOwns = initialSumm.Copy();
        }
        public void Increase(Value sum)
        {
            howMuchOwns.Add(sum);
        }
        public void Decrease(Value sum)
        {
            howMuchOwns.Subtract(sum);
        }
        internal void CancelBuyOrder(Value sum)
        {
            howMuchWantsToSell.Subtract(sum, false);
        }
        /// <summary>
        /// Only for read! Returns copy
        /// </summary>        
        public Value GetShare()
        {
            return howMuchOwns.Copy();
        }
        /// <summary>
        /// Only for read! Returns copy
        /// </summary>        
        public Value GetShareForSale()
        {
            return howMuchWantsToSell.Copy();
        }
        public void SetToSell(Value sum)
        {
            if (howMuchOwns.get() - howMuchWantsToSell.get() - sum.get() < 0f)
                howMuchWantsToSell.Set(howMuchOwns);
            else
                howMuchWantsToSell.Add(sum);            
        }
        public void ReduceSale(Value sum)
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