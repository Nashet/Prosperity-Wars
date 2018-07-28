using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public interface IHasCountry
    {
        Country Country { get; }
    }

    public interface IHasProvince
    {
        Province Province { get; }
    }

    public interface IWayOfLifeChange
    {
        //bool HasJobsFor(PopType popType, Province province);
        ReadOnlyValue getLifeQuality(PopUnit pop);

        //string getWayOfLifeString(PopUnit pop);
    }

    public interface IInvestable : IHasCountry, IHasProvince
    {
        /// <summary>
        /// Includes tax (1 country only), salary and modifiers. Doesn't include risks. New value
        /// </summary>
        Procent GetMargin();

        MoneyView GetInvestmentCost(Market market);

        bool CanProduce(Product product);
    }
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