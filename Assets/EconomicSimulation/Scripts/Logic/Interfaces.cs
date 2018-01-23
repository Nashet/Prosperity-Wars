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


}