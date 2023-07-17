using System.Collections.Generic;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;

namespace Nashet.Utils
{
	public class PricePool
    {
        private Dictionary<Product, DataStorageProduct> pool = new Dictionary<Product, DataStorageProduct>();
        public static readonly int lenght = 40; // !! duplicate of DataStorage!!

        public PricePool()
        {
            foreach (var product in Product.AllNonAbstract())
                if (product != Product.Gold)
                    for (int i = 0; i < lenght; i++)
                        addData(product, new Value(0f));
        }

        public void addData(Product product, Value indata)
        {
            DataStorageProduct cell;
            if (!pool.TryGetValue(product, out cell))
            {
                cell = new DataStorageProduct(product);
                pool.Add(product, cell);
            }
            cell.addData(indata);
        }

        //public System.Collections.IEnumerator GetEnumerator()
        //{
        //    for (int i = 0; i < pool.Count; i++)
        //    {
        //        yield return pool.GetEnumerator();
        //    }
        //}
        public DataStorageProduct getPool(Product product)
        {
            //return pool[pro];
            DataStorageProduct result;
            if (pool.TryGetValue(product, out result)) // Returns true.
            {
                return result;
            }
            else
                return null;
        }
    }
}