using UnityEngine;
using UnityEditor;
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
            howMuchOwns = new Value(initialSumm);
        }
        public void Increase(Value sum)
        {
            howMuchOwns.add(sum);
        }
        public void Decrease(Value sum)
        {
            howMuchOwns.subtract(sum);
        }
        internal void CancelBuyOrder(Value sum)
        {
            howMuchWantsToSell.subtract(sum, false);
        }
        /// <summary>
        /// Only for read!
        /// </summary>        
        public Value GetShare()
        {
            return new Value(howMuchOwns);
        }
        /// <summary>
        /// Only for read!
        /// </summary>        
        public Value GetShareForSale()
        {
            return new Value(howMuchWantsToSell);
        }
        public void SetToSell(Value sum)
        {
            if (howMuchOwns.get() - howMuchWantsToSell.get() - sum.get() < 0f)
                howMuchWantsToSell.set(howMuchOwns);
            else
                howMuchWantsToSell.add(sum);            
        }
        public void ReduceSale(Value sum)
        {
            howMuchWantsToSell.subtract(sum, false);
            if (howMuchWantsToSell.isBiggerThan(howMuchOwns))
                howMuchWantsToSell.set(howMuchOwns);
        }
        public override string ToString()
        {
            return howMuchOwns.ToString();
        }
    }


}