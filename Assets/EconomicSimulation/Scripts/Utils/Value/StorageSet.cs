using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.EconomicSimulation;
using Nashet.Utils;
using System.Text;
using System.Linq;

namespace Nashet.ValueSpace
{
    public class StorageSet : ICopyable<StorageSet>
    {
        //private static Storage tStorage;
        //private List<Storage> container = new List<Storage>();
        private readonly Dictionary<Product, Storage> collection = new Dictionary<Product, Storage>();

        public StorageSet()
        { }
        protected StorageSet(StorageSet another)
        {
            foreach (var item in another)
            {
                collection.Add(item.getProduct(), item.Copy());
            }
        }
        public StorageSet(List<Storage> list)
        {
            for (int i = 0; i < list.Count; i++)
                collection.Add(list[i].getProduct(), list[i].Copy());
        }


        /// <summary>
        /// If duplicated than overwrites. Doesn't take abstract products
        /// </summary>    
        public void Set(Storage what)
        {
            Storage res;
            if (collection.TryGetValue(what.getProduct(), out res))
                res.set(what);
            else
                collection.Add(what.getProduct(), what);
            //Storage find = this.hasStorage(setValue.getProduct());
            //if (find == null)
            //    container.Add(new Storage(setValue));
            //else
            //    find.set(setValue);
        }
        /// <summary>
        /// If duplicated than overwrites. Doesn't take abstract products
        /// </summary>    
        //public void set(Product product, Value value)
        //{
        //    Storage find = hasStorage(product);
        //    if (find == null)
        //        container.Add(new Storage(product, value));
        //    else
        //        find.set(value);
        //}
        /// <summary>
        /// If duplicated than adds. Doesn't take abstract products
        /// </summary>
        internal void Add(Storage what)
        {
            Storage find;
            if (collection.TryGetValue(what.getProduct(), out find))
                find.add(what);
            else
                collection.Add(what.getProduct(), new Storage(what));
            //Storage find = hasStorage(what.getProduct());
            //if (find == null)
            //    container.Add(new Storage(what));
            //else
            //    find.add(what);
        }
        /// <summary>
        /// If duplicated than adds. Doesn't take abstract products
        /// </summary>
        internal void Add(StorageSet what)
        {
            foreach (Storage item in what)
                this.Add(item);
        }
        /// <summary>
        /// If duplicated than adds. Doesn't take abstract products
        /// </summary>
        internal void Add(List<Storage> need)
        {
            foreach (Storage n in need)
                this.Add(n);
        }
        public IEnumerator<Storage> GetEnumerator()
        {
            foreach (var item in collection)
            {
                yield return item.Value;
            }
        }
        //public List<Storage> getContainer()
        //{
        //    return collection;
        //}

        /// <summary>
        /// Do checks outside
        /// </summary>   
        public bool send(Producer whom, Storage what)
        {
            Storage storage = getBiggestStorage(what.getProduct());
            if (storage.isZero())
                return false;
            else
                return storage.send(whom.storage, what);
        }
        /// <summary>
        /// Do checks outside
        /// </summary>   
        public bool Send(StorageSet whom, StorageSet what)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Do checks outside
        /// </summary>   
        //public bool send(StorageSet whom, List<Storage> what)
        //{
        //    bool res = true;
        //    foreach (var item in what)
        //    {
        //        whom.Add(item);
        //        this.Subtract(item);
        //    }
        //    return res;
        //}
        internal void sendAll(StorageSet toWhom)
        {
            toWhom.Add(this);
            this.setZero();
        }
        /// <summary>
        /// Do checks outside
        /// </summary>   
        public bool send(Producer whom, List<Storage> what)
        {
            bool result = true;
            foreach (var item in what)
            {
                if (!send(whom, item))
                    result = false;
            }
            return result;
        }
        public bool has(Storage what)
        {
            Storage foundStorage = getBiggestStorage(what.getProduct());
            return (foundStorage.isBiggerOrEqual(what)) ? true : false;
        }
        /// <summary>Returns False when some check not presented in here</summary>    
        internal bool has(StorageSet check)
        {
            foreach (Storage stor in check)
                if (!has(stor))
                    return false;
            return true;
        }
        /// <summary>Returns False if any item from list are not available</summary>    
        internal bool has(List<Storage> list)
        {
            foreach (Storage stor in list)
                if (!has(stor))
                    return false;
            return true;
        }
        /// <summary>Returns null if any item from list are not available, otherwise returns what real have (converted to un-abstract products)</summary>    
        internal List<Storage> hasAllOfConvertToBiggest(List<Storage> list)
        {
            var res = new List<Storage>();
            foreach (var what in list)
            {
                //Storage foundStorage = getBiggestStorage(what.getProduct());
                var foundStorage = convertToBiggestExistingStorage(what);
                if (foundStorage.isNotZero())
                    res.Add(foundStorage);
                else
                    return null;
            }
            return res;
        }
        internal bool hasMoreThan(Storage item, Value limit)
        {
            Storage disiredAmount = new Storage(item.getProduct(), item.get() + limit.get());
            return has(disiredAmount);
        }
        /// <summary>Returns  null if container hasn't storage for that product
        /// Alternative is .getStorage()</summary>    
        //protected Storage GetStorageNullable(Product product)
        //{
        //    Storage found;
        //    if (collection.TryGetValue(product, out found))
        //        return found;
        //    else
        //        return null;            
        //    //foreach (Storage stor in collection)
        //    //    if (stor.isExactlySameProduct(product))
        //    //        return stor;
        //    //return null;
        //}
        //internal Procent HowMuchHaveOf(PrimitiveStorageSet need)
        //{
        //    PrimitiveStorageSet shortage = this.subtractOuside(need);
        //    return new Procent(shortage, need);
        //}

        /// <summary>Returns NULL if search is failed</summary>
        //internal Storage findStorage(Product product)
        //{
        //    foreach (Storage stor in container)
        //        if (stor.isSameProductType(product))
        //            return stor;
        //    return null;
        //}
        /// <summary>Returns NULL if search is failed</summary>
        //internal Storage findStorage(Storage storageToFind)
        //{
        //    foreach (Storage stor in container)
        //        if (stor.isSameProduct(storageToFind))
        //            return stor;
        //    return null;
        //}
        /// <summary>Returns NULL if search is failed</summary>
        //internal Storage findSubstitute(Storage need)
        //{
        //    foreach (Storage storage in container)
        //        if (storage.hasSubstitute(need))
        //            return storage;
        //    return null;
        //}
        /// <summary>Returns NULL if search is failed</summary>
        //internal Storage findSubstitute(Product need)
        //{
        //    foreach (Storage storage in container)
        //        if (storage.isSubstituteProduct(need))
        //            return storage;
        //    return null;
        //}
        /// <summary>Returns NULL if search is failed</summary>
        //internal Storage findExistingSubstitute(Storage need)
        //{
        //    foreach (Storage storage in container)
        //        if (storage.hasSubstitute(need))
        //            return storage;
        //    return null;
        //} 

        /// <summary>Return first found storage of what products  [or it's first substitute] Doesn't cares about substitutes
        /// Returns NEW empty storage if search is failed</summary>
        internal Storage GetFirstSubstituteStorage(Product what)
        {
            Storage res;
            if (collection.TryGetValue(what, out res))
                return res;
            else
                return new Storage(what, 0f);

            //if (what.isAbstract())
            //{
            //    foreach (var Product in what.getSubstitutes())
            //    {
            //        Storage found;
            //        if (collection.TryGetValue(Product, out found))
            //            return found;
            //    }
            //    //foreach (var item in collection)
            //    //    if (item.Key.isSubstituteFor(what))
            //    //        return item.Value;
            //    return new Storage(what, 0f);
            //}
            //else
            //{
            //    Storage res;
            //    if (collection.TryGetValue(what, out res))
            //        return res;
            //    else
            //        return new Storage(what, 0f);
            //}
        }

        public StorageSet Copy()
        {
            return new StorageSet(this);
        }

        /// <summary>Gets storage if there is enough product of that type. 
        /// Returns NEW empty storage if search is failed
        /// Read only!!</summary>    
        internal Storage GetExistingStorage(Storage what)
        {
            Storage found;
            if (collection.TryGetValue(what.getProduct(), out found))
                if (found.has(what))
                    return new Storage(found);
            //foreach (var storage in collection)
            //    if (storage.Value.has(what))
            //        return storage.Value;
            //if not found
            return new Storage(what.getProduct(), 0f);
        }

        /// <summary>Gets biggest storage of that product type. Returns NEW empty storage if search is failed</summary>    
        internal Storage getBiggestStorage(Product what)
        {
            return getStorage(what, CollectionExtensions.MaxBy, x => x.get());
        }
        /// <summary>Gets cheapest storage of that product type. Returns NEW empty storage if search is failed</summary>    
        internal Storage getCheapestStorage(Product what)
        {
            return getStorage(what, CollectionExtensions.MinBy, x => Game.market.getPrice(x.getProduct()).get());
        }
        /// <summary> Finds substitute for abstract need and returns new storage with product converted to non-abstract product
        /// Returns copy of need if need was not abstract (make check)
        /// If didn't find substitute returns copy of empty storage of need product</summary>  

        internal Storage convertToBiggestStorage(Storage need)
        {
            return new Storage(getBiggestStorage(need.getProduct()).getProduct(), need);
        }
        /// <summary> Finds substitute for abstract need and returns new storage with product converted to non-abstract product
        /// Returns copy of need if need was not abstract (make check)
        /// If didn't find substitute OR there is no enough product returns copy of empty storage of need product</summary>  

        internal Storage convertToBiggestExistingStorage(Storage need)
        {
            var substitute = getBiggestStorage(need.getProduct());
            if (substitute.isBiggerOrEqual(need))
                return new Storage(substitute.getProduct(), need);
            else
                return new Storage(substitute.getProduct(), 0f);
        }
        /// <summary>
        /// Returns NULL if failed
        /// </summary>    
        public Storage convertToCheapestExistingSubstitute(Storage abstractProduct)
        {
            // assuming substitutes are sorted in cheap-expensive order
            foreach (var substitute in abstractProduct.getProduct().getSubstitutes())
                if (substitute.isTradable())
                {
                    Storage newStor = new Storage(substitute, abstractProduct);
                    // check for availability
                    if (Game.market.sentToMarket.has(newStor))
                        return newStor;
                    //return this.sentToMarket.getExistingStorage(item);
                }
            return null;
        }
        /// <summary>
        /// Returns NULL if failed
        /// </summary> 
        public Storage convertToCheapestStorageProduct(Storage abstractProduct)
        {
            // assuming substitutes are sorted in cheap-expensive order
            foreach (var item in abstractProduct.getProduct().getSubstitutes())
                if (item.isTradable())
                {
                    return new Storage(item, abstractProduct);
                }
            return null;
        }
        /// <summary> Assuming product is abstract product
        /// Returns total sum of all substitute products</summary>       
        public Storage getTotal(Product product)
        {
            Value res = new Value(0f);
            foreach (var item in this)
                if (item.getProduct().isSubstituteFor(product))
                {
                    res.Add(item);
                }
            return new Storage(product, res);
        }

        /// <summary>Universal search for storages</summary>       
        private Storage getStorage(Product what,
           Func<IEnumerable<Storage>, Func<Storage, float>, Storage> selectorMethod,
           Func<Storage, float> selector)
        {
            if (what.isAbstract())
            {
                List<Storage> res = new List<Storage>();
                foreach (var storage in collection)
                    if (storage.Value.isSameProductType(what))// && !storage.isAbstractProduct())
                        res.Add(storage.Value);
                var found = selectorMethod(res, selector);
                if (found == null)
                    return new Storage(what, 0f);
                return found;
            }
            else
            {
                foreach (var storage in collection)
                    if (storage.Value.isExactlySameProduct(what))
                        return storage.Value;
                return new Storage(what, 0f);
            }
        }

        internal List<Storage> ToList()
        {
            var res = new List<Storage>();
            foreach (var item in collection)
                res.Add(item.Value.Copy());
            return res;
        }

        override public string ToString()
        {
            return GetString(", ");
        }
        /// <summary>
        /// Provides access to dictionary.GetString()
        /// </summary>        
        public string GetString(String lineBreaker)
        {
            return collection.GetString(lineBreaker);
        }

        //public string GetString(String lineBreaker)
        //{
        //    if (collection.Count > 0)
        //    {
        //        var sb = new StringBuilder();
        //        bool isFirstRow = true;
        //        foreach (var item in collection)
        //            if (item.Value.isNotZero())
        //            {
        //                if (!isFirstRow)
        //                    sb.Append(lineBreaker);
        //                isFirstRow = false;
        //                sb.Append(item.Key).Append(" ").Append(item.Value.get());
        //            }
        //        return sb.ToString();
        //    }
        //    else
        //        return "none";
        //}
        internal void setZero()
        {
            foreach (Storage st in this)
                st.set(0f);
        }

        internal int Count()
        {
            return collection.Count(x=>x.Value.isNotZero());
        }

        internal StorageSet Multiply(Value value)
        {
            foreach (var stor in collection)
                stor.Value.multiply(value);
            return this;
        }
        internal StorageSet Divide(Value divider)
        {
            foreach (var stor in collection)
                stor.Value.divide(divider);
            return this;
        }

        //internal bool subtract(Storage stor)
        //{
        //    Storage find = this.findStorage(stor.getProduct());
        //    if (find == null)
        //        return false;//container.Add(value);
        //    else
        //    {
        //        if (find.has(stor))
        //            return find.subtract(stor);
        //        else
        //        {
        //            Debug.Log("Someone tried to subtract from Pr.Storage more than it has");
        //            find.setZero();
        //            return false;
        //        }
        //    }
        //}
        /// <summary>
        /// Does not take abstract products
        /// </summary>    
        virtual public bool Subtract(Storage storage, bool showMessageAboutNegativeValue = true)
        {
            Storage found;
            if (collection.TryGetValue(storage.getProduct(), out found))
            {
                var res = found.has(storage);                
                found.subtract(storage, showMessageAboutNegativeValue);
                return res;
            }
            else
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("This StorageSet don't have - " + storage + " " + storage);
                return false;//container.Add(value); }

                //    Storage found = GetStorageNullable(storage.getProduct());
                ////Storage found = getBiggestStorage(storage.getProduct());
                //if (found == null)
                //{
                //    if (showMessageAboutNegativeValue)
                //        Debug.Log("Someone tried to subtract from StorageSet more than it has - " + storage);
                //    return false;//container.Add(value);
                //}
                //else
                //    return found.subtract(storage, showMessageAboutNegativeValue);
            }
        }
        /// <summary>
        /// Does not take  abstract products
        /// </summary>
        public void subtract(StorageSet set, bool showMessageAboutNegativeValue = true)
        {
            foreach (Storage stor in set)
                this.Subtract(stor, showMessageAboutNegativeValue);
        }
        /// <summary>
        /// Does not take  abstract products
        /// </summary>
        internal void subtract(List<Storage> set, bool showMessageAboutNegativeValue = true)
        {
            foreach (Storage stor in set)
                this.Subtract(stor, showMessageAboutNegativeValue);
        }

        //internal void copyDataFrom(StorageSet consumed)
        //{
        //    foreach (Storage stor in consumed)                
        //        this.Set(stor);
        //}


        internal Value GetTotalQuantity()
        {
            var result = new Value(0f);
            foreach (var item in collection)
                result.Add(item.Value);
            return result;
        }


        //internal bool hasSubstitute(Storage need)
        //{
        //    foreach (var item in container)
        //    {
        //        if (item.hasSubstitute(need))
        //            return true;
        //    }
        //    return false;
        //}



        //internal PrimitiveStorageSet Copy()
        //{
        //    oldList.ForEach((item) =>
        //    {
        //        newList.Add(new YourType(item));
        //    });
        //}
    }
}