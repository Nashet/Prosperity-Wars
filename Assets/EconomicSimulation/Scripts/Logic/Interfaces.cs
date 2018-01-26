using Nashet.ValueSpace;
using UnityEngine;


namespace Nashet.EconomicSimulation
{
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
        Value getCost();
        bool canProduce(Product product);
        Procent GetWorkForceFulFilling();
    }

}