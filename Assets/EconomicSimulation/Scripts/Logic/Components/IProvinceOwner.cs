using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents ability to own provinces 
    /// </summary>
    public interface IProvinceOwner
    {
        IEnumerable<Province> AllProvinces{ get; }
    }
}