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
    public interface IEscapeTarget
    {
        bool HasJobsFor(PopType popType, Province province);
    }
    public interface IInvestable
    {
        Procent GetMargin();
        Value GetInvestmentCost();
        /// <summary>
        /// Nit Only for Aristocrats: allows type sampling
        /// </summary>        
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