using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public interface IInvestable : IHasCountry, IHasProvince
    {
        /// <summary>
        /// Includes tax (1 country only), salary and modifiers. Doesn't include risks. New value
        /// </summary>
        Procent GetMargin();

        MoneyView GetInvestmentCost(Market market);

        bool CanProduce(Product product);
    }
}