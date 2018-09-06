using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public interface ISeller
    {
        /// <summary>
        /// Part of sale operation
        /// </summary>        
        void SendToMarket(Storage what);


        IEnumerable<Market> AllTradeMarkets();

        IEnumerable<KeyValuePair<Market, Storage>> AllSellDeals();

        Storage HowMuchSentToMarket(Market market, Product product);
    }
}