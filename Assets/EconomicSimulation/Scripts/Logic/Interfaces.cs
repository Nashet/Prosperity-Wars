using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public interface IHasCountry
    {
        Country Country { get; }
    }

    public interface IHasGetProvince
    {
        Province Province { get; }
    }

    public interface IWayOfLifeChange
    {
        //bool HasJobsFor(PopType popType, Province province);
        ReadOnlyValue getLifeQuality(PopUnit pop);

        //string getWayOfLifeString(PopUnit pop);
    }

    public interface IInvestable : IHasCountry, IHasGetProvince
    {
        /// <summary>
        /// Includes tax (1 country only), salary and modifiers. Doesn't include risks. New value
        /// </summary>
        Procent GetMargin();

        MoneyView GetInvestmentCost();

        bool CanProduce(Product product);
    }
}