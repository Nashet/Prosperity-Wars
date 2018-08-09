using Nashet.EconomicSimulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nashet.ValueSpace
{
    interface IStorageSet
    {
        Storage GetFirstSubstituteStorage(Product what);
    }
}
