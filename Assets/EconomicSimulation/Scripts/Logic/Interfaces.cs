using Nashet.ValueSpace;
using UnityEngine;


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
    public interface IEscapeTarget
    {
        bool HasJobsFor(PopType popType, Province province);
    }
    public interface IInvestable : IHasCountry, IHasGetProvince
    {
        /// <summary>
        /// Includes tax (1 country only), salary and modifiers. Doesn't include risks. New value
        /// </summary>
        Procent GetMargin();
        Value GetInvestmentCost();        
        bool CanProduce(Product product);

    }
    
    
}