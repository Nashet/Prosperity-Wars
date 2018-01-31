using Nashet.ValueSpace;
using UnityEngine;


namespace Nashet.EconomicSimulation
{
    interface IHasStatistics
    {
        void setStatisticToZero();
    }
    public interface IHasGetCountry
    {
        Country getCountry();
    }
    public interface IEscapeTarget
    {
        bool HasJobsFor(PopType popType, Province province);
    }
    public interface IInvestable
    {
        Procent getMargin();
        Value getInvestmentCost();
        /// <summary>
        /// Only for Aristocrats: allows type sampling
        /// </summary>        
        bool canProduce(Product product);
    }
    public interface ISortableName
    {
        float GetNameWeight();
    }
    
}