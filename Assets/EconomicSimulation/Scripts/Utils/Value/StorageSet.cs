using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.EconomicSimulation;
using Nashet.Utils;
using UnityEngine;
using CollectionExtensions = Nashet.Utils.CollectionExtensions;

namespace Nashet.ValueSpace
{
    public class StorageSet : ICopyable<StorageSet>, IStorageSet
    {
        //private static Storage tStorage;
        //private List<Storage> container = new List<Storage>();
        protected readonly Dictionary<Product, Storage> collection = new Dictionary<Product, Storage>();

        public StorageSet()
        { }

        protected StorageSet(StorageSet another)
        {
            foreach (var item in another)
            {
                collection.Add(item.Product, item.Copy());
            }
        }

        public StorageSet(List<Storage> list)
        {
            for (int i = 0; i < list.Count; i++)
                collection.Add(list[i].Product, list[i].Copy());
        }

        /// <summary>
        /// If duplicated than overwrites. Doesn't take abstract products
        /// </summary>
        public void Set(Storage what)
        {
            Storage res;
            if (collection.TryGetValue(what.Product, out res))
                res.set(what);
            else
                collection.Add(what.Product, what);
            //Storage find = this.hasStorage(setValue.Product);
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
        public void Add(Storage what)
        {
            Storage find;
            if (collection.TryGetValue(what.Product, out find))
                find.add(what);
            else
                collection.Add(what.Product, new Storage(what));
            //Storage find = hasStorage(what.Product);
            //if (find == null)
            //    container.Add(new Storage(what));
            //else
            //    find.add(what);
        }

        /// <summary>
        /// If duplicated than adds. Doesn't take abstract products
        /// </summary>
        public void Add(StorageSet what)
        {
            foreach (Storage item in what)
                Add(item);
        }

        /// <summary>
        /// If duplicated than adds. Doesn't take abstract products
        /// </summary>
        public void Add(List<Storage> need)
        {
            foreach (Storage n in need)
                Add(n);
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
            Storage storage = GetStorage(what.Product);
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
        public void sendAll(StorageSet toWhom)
        {
            toWhom.Add(this);
            setZero();
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
            if (collection.TryGetValue(what.Product, out var res))
                return res.isBiggerOrEqual(what);
            else
                return false;
        }

        /// <summary>Returns False when some check not presented in here</summary>
        public bool has(StorageSet check)
        {
            foreach (Storage stor in check)
                if (!has(stor))
                    return false;
            return true;
        }

        /// <summary>Returns False if any item from list are not available</summary>
        public bool has(List<Storage> list)
        {
            foreach (Storage stor in list)
                if (!has(stor))
                    return false;
            return true;
        }

        ///// <summary>Returns null if any item from list are not available, otherwise returns what real have (converted to un-abstract products)</summary>
        //public List<Storage> hasAllOfConvertToBiggest(List<Storage> list)
        //{
        //    var res = new List<Storage>();
        //    foreach (var what in list)
        //    {
        //        //Storage foundStorage = getBiggestStorage(what.Product);
        //        var foundStorage = convertToBiggestExistingStorage(what);
        //        if (foundStorage.isNotZero())
        //            res.Add(foundStorage);
        //        else
        //            return null;
        //    }
        //    return res;
        //}

        public bool hasMoreThan(Storage item, Value limit)
        {
            Storage disiredAmount = new Storage(item.Product, item.get() + limit.get());
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
        //public Procent HowMuchHaveOf(PrimitiveStorageSet need)
        //{
        //    PrimitiveStorageSet shortage = this.subtractOuside(need);
        //    return new Procent(shortage, need);
        //}

        /// <summary>Returns NULL if search is failed</summary>
        //public Storage findStorage(Product product)
        //{
        //    foreach (Storage stor in container)
        //        if (stor.isSameProductType(product))
        //            return stor;
        //    return null;
        //}
        /// <summary>Returns NULL if search is failed</summary>
        //public Storage findStorage(Storage storageToFind)
        //{
        //    foreach (Storage stor in container)
        //        if (stor.isSameProduct(storageToFind))
        //            return stor;
        //    return null;
        //}      

        /// <summary>Return first found storage of what products  [or it's first substitute] Doesn't cares about substitutes
        /// Returns NEW empty storage if search is failed</summary>
        public Storage GetStorage(Product what)
        {
            Storage res;
            if (collection.TryGetValue(what, out res))
                return res;
            else
                return new Storage(what, 0f);
        }

        public StorageSet Copy()
        {
            return new StorageSet(this);
        }

        ///// <summary>Gets storage if there is enough product of that type.
        ///// Returns NEW empty storage if search is failed
        ///// Read only!!</summary>
        //public Storage GetExistingStorage(Storage what)
        //{
        //    Storage found;
        //    if (collection.TryGetValue(what.Product, out found))
        //        if (found.has(what))
        //            return new Storage(found);
        //    //foreach (var storage in collection)
        //    //    if (storage.Value.has(what))
        //    //        return storage.Value;
        //    //if not found
        //    return new Storage(what.Product, 0f);
        //}

        ///// <summary>Gets biggest storage of that product type. Returns NEW empty storage if search is failed</summary>
        //public Storage getBiggestStorage(Product what)??
        //{
        //    return getStorage(what, CollectionExtensions.MaxBy, x => x.get());
        //}

        ///// <summary>Gets cheapest storage of that product type. Returns NEW empty storage if search is failed</summary>
        //public Storage getCheapestStorage(Product what, Market market) ??
        //{
        //    return getStorage(what, CollectionExtensions.MinBy, x => (float)market.getCost(x.Product).Get());
        //}

        ///// <summary> Finds substitute for abstract need and returns new storage with product converted to non-abstract product
        ///// Returns copy of need if need was not abstract (make check)
        ///// If didn't find substitute returns copy of empty storage of need product</summary>

        //public Storage convertToBiggestStorage(Storage need)??
        //{
        //    return new Storage(getBiggestStorage(need.Product).Product, need);
        //}

        ///// <summary> Finds substitute for abstract need and returns new storage with product converted to non-abstract product
        ///// Returns copy of need if need was not abstract (make check)
        ///// If didn't find substitute OR there is no enough product returns copy of empty storage of need product</summary>

        //public Storage convertToBiggestExistingStorage(Storage need)??
        //{
        //    var substitute = getBiggestStorage(need.Product);
        //    if (substitute.isBiggerOrEqual(need))
        //        return new Storage(substitute.Product, need);
        //    else
        //        return new Storage(substitute.Product, 0f);
        //}        

        /// <summary>Universal search for storages</summary>
        private Storage getStorage(Product what,
           Func<IEnumerable<Storage>, Func<Storage, float>, Storage> selectorMethod,
           Func<Storage, float> selector)
        {            
                foreach (var storage in collection)
                    if (storage.Value.isExactlySameProduct(what))
                        return storage.Value;
                return new Storage(what, 0f);
        }

        public List<Storage> ToList()
        {
            var res = new List<Storage>();
            foreach (var item in collection)
                res.Add(item.Value.Copy());
            return res;
        }

        public override string ToString()
        {
            return GetString(", ");
        }

        /// <summary>
        /// Provides access to dictionary.GetString()
        /// </summary>
        public string GetString(String lineBreaker)
        {
            return collection.ToString(lineBreaker);
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
        public void setZero()
        {
            foreach (Storage st in this)
                st.Set(0f);
        }

        public int Count()
        {
            return collection.Count(x => x.Value.isNotZero());
        }

        public StorageSet Multiply(Value value)
        {
            foreach (var stor in collection)
                stor.Value.Multiply(value);
            return this;
        }

        public StorageSet Divide(Value divider)
        {
            foreach (var stor in collection)
                stor.Value.Divide(divider);
            return this;
        }

        //public bool subtract(Storage stor)
        //{
        //    Storage find = this.findStorage(stor.Product);
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
        public virtual bool Subtract(Storage storage, bool showMessageAboutNegativeValue = true)
        {
            Storage found;
            if (collection.TryGetValue(storage.Product, out found))
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

                //    Storage found = GetStorageNullable(storage.Product);
                ////Storage found = getBiggestStorage(storage.Product);
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
                Subtract(stor, showMessageAboutNegativeValue);
        }

        /// <summary>
        /// Does not take  abstract products
        /// </summary>
        public void subtract(List<Storage> set, bool showMessageAboutNegativeValue = true)
        {
            foreach (Storage stor in set)
                Subtract(stor, showMessageAboutNegativeValue);
        }

        //public void copyDataFrom(StorageSet consumed)
        //{
        //    foreach (Storage stor in consumed)
        //        this.Set(stor);
        //}

        public Value GetTotalQuantity()
        {
            var result = new Value(0f);
            foreach (var item in collection)
                result.Add(item.Value);
            return result;
        }
     
        //public PrimitiveStorageSet Copy()
        //{
        //    oldList.ForEach((item) =>
        //    {
        //        newList.Add(new YourType(item));
        //    });
        //}
    }
}