using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <summary> Assuming product is abstract product
        /// Returns total sum of all substitute products</summary>
        public static Storage getTotal(this IEnumerable<Storage> numerable, Product product)
        {
            Value res = new Value(0f);
            foreach (var item in numerable)
                if (item.Product.isSubstituteFor(product))
                {
                    res.Add(item);
                }
            return new Storage(product, res);
        }

    }
}
