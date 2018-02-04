using Nashet.ValueSpace;
using UnityEngine;


namespace Nashet.EconomicSimulation
{
    interface IHasStatistics
    {
        void SetStatisticToZero();
    }
    public interface IHasGetCountry
    {
        Country GetCountry();
    }
    public interface IHasGetProvince
    {
        Province GetProvince();
    }
    public interface IEscapeTarget
    {
        bool HasJobsFor(PopType popType, Province province);
    }
    public interface IInvestable : IHasGetCountry, IHasGetProvince
    {
        /// <summary>
        /// Includes tax (1 country only), salary and modifiers. Doesn't include risks. New value
        /// </summary>
        Procent GetMargin();
        Value GetInvestmentCost();        
        bool CanProduce(Product product);

    }
    public interface ISortableName
    {
        float GetNameWeight();
    }
    public interface IDescribable
    {
        string GetDescription();
    }
}