using System.Collections.Generic;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;

namespace Nashet.Utils
{
    /// <summary>!! Broken. Assuming product is abstract product</summary>
    public static class ListStorageExtensions
    {
        public static Storage getStorageIncludingSubstitutes(this List<Storage> source, Product product)
        {
            var res = new Value(0f);
            foreach (var substitute in product.getSubstitutes())
                if (substitute.isTradable())
                {
                    // how find food & grain? broken
                    //var find = source.Find(x => x.is);
                    //if (find != null)
                    //var find = source.getStorage(substitute);
                    //if (find.isNotZero())
                    //  res.add(find);
                }
            return new Storage(product, res);
        }

        public static Storage getStorage(this List<Storage> list, Product product)
        {
            foreach (Storage stor in list)
                if (stor.isExactlySameProduct(product))
                    return stor;
            return new Storage(product, 0f);
        }

        public static Storage GetFirstSubstituteStorage(this List<Storage> list, Product product)
        {
            if (product.isAbstract())
            {
                foreach (Storage stor in list)
                    if (stor.isSameProductType(product))
                        return stor;
            }
            else
                return list.Find(x => x.Product == product);
            return new Storage(product, 0f);
        }

        public static List<Storage> Multiply(this List<Storage> list, Value value)
        {
            foreach (var item in list)
            {
                item.Multiply(value);
            }
            return list;
        }
        public static List<Storage> Multiply(this List<Storage> list, float value)
        {
            foreach (var item in list)
            {
                item.Multiply(value);
            }
            return list;
        }

        public static Value Sum(this IEnumerable<Storage> list)
        {
            Value sum = new Value(0f);
            if (list == null)
                return sum;
            foreach (var item in list)
            {
                sum.Add(item);
            }
            return sum;
        }

        /// <summary>
        /// Does dip copy
        /// </summary>

        public static List<Storage> Copy(this List<Storage> list)
        {
            var res = new List<Storage>();
            foreach (var item in list)
                res.Add(item.Copy());
            return res;
        }
    }
}