using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public interface IProvinceHolder
    {
        IEnumerable<Province> AllProvinces{ get; }
    }
}