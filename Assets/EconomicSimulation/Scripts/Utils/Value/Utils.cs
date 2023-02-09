
using System.Collections.Generic;
using System.Linq;
using Nashet.EconomicSimulation;

namespace Nashet.ValueSpace
{
    static class IEnumerableStorageExtensions
    {
        public static Storage GetFirstSubstituteStorage(this IEnumerable<Storage> numerable, Product what)
        {
            var found = numerable.FirstOrDefault(x => x.Product == what);
            if (found == null)
                return new Storage(what, 0f);
            else
                return found;
        }
    }
}
