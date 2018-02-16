using UnityEngine;
using Nashet.Utils;
using Nashet.EconomicSimulation;
using System.Collections.Generic;

namespace Nashet.ValueSpace
{
    /// <summary>
    /// Allows to keep info about how much product was taken from StorageSet
    /// !!! if someone would change returning object (Storage) then country takenAway logic would be broken!!
    /// </summary>
    public class CountryStorageSet : StorageSet, IHasStatistics
    {
        /// <summary>
        /// Counts how much products was taken from country storage 
        /// for consumption or some spending. Shouldn't include sells
        /// Used to determinate how much to buy deficit or sell extra products
        /// </summary>
        public readonly StorageSet used = new StorageSet();

        //internal Value getConsumption(Product whom)
        //{
        //    foreach (Storage stor in takenAwayLastTurn)
        //        if (stor.getProduct() == whom)
        //            return stor;
        //    return new Value(0f);
        //}
        public void SetStatisticToZero()
        {
            used.setZero();
            //collection.PerformAction(x => !x.Value.getProduct().IsStorable, x => x.Value.SetZero());
            Storage record;            
            if (collection.TryGetValue(Product.Education, out record))
                record.SetZero();
        }

        /// / next - inherited


        public void set(Storage inn)
        {
            base.Set(inn);
            throw new DontUseThatMethod();
        }
        ///// <summary>
        ///// If duplicated than adds
        ///// </summary>
        //internal void add(Storage need)
        //{
        //    base.add(need);
        //    consumedLastTurn.add(need)
        //}

        ///// <summary>
        ///// If duplicated than adds
        ///// </summary>
        //internal void add(PrimitiveStorageSet need)
        //{ }

        /// <summary>
        /// Do checks outside
        /// Supports takenAway
        /// </summary>   
        public bool send(Producer whom, Storage what)
        {
            if (base.send(whom, what))
            {
                used.Add(what);
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Do checks outside Check for taken away
        /// </summary>   
        //public bool send(Producer whom, List<Storage> what)
        //{
        //    bool result = true;
        //    foreach (var item in what)
        //    {
        //        if (!send(whom, item))
        //            result = false;
        //    }
        //    return result;           
        //}
        /// <summary>
        /// Do checks outside
        /// Supports takenAway
        /// Don't need it for now
        /// </summary>   
        //public bool send(StorageSet whom, StorageSet what)
        //{
        //    return send(whom, what.getContainer());
        //}
        /// <summary>
        /// Do checks outside
        /// Supports takenAway
        /// /// Don't need it for now
        /// </summary>   
        //public bool send(StorageSet whom, List<Storage> what)
        //{
        //    used.Add(what);
        //    return base.send(whom, what);
        //}
        /// <summary>
        /// 
        /// /// Supports takenAway
        /// </summary>

        override public bool Subtract(Storage stor, bool showMessageAboutNegativeValue = true)
        {
            if (base.Subtract(stor, showMessageAboutNegativeValue))
            {
                used.Add(stor);
                return true;
            }
            else
                return false;
        }
        //internal void subtract(List<Storage> set, bool showMessageAboutNegativeValue = true)
        //{
        //    foreach (Storage stor in set)
        //    {
        //        subtract(stor, showMessageAboutNegativeValue);
        //    }
        //}
        public bool subtractNoStatistic(Storage stor, bool showMessageAboutNegativeValue = true)
        {
            return base.Subtract(stor, showMessageAboutNegativeValue);
        }
        /// <summary>
        /// //todo !!! if someone would change returning object then country consumption logic would be broken!!
        /// </summary>    
        //internal Value getStorage(Product whom)
        //{
        //    return base.getStorage(whom);
        //}

        internal void SetZero()
        {
            base.setZero();
            throw new DontUseThatMethod();
        }
        //internal PrimitiveStorageSet Divide(float v)
        //{
        //    PrimitiveStorageSet result = new PrimitiveStorageSet();
        //    foreach (Storage stor in container)
        //        result.Set(new Storage(stor.getProduct(), stor.get() / v));
        //    return result;
        //}



        //internal Storage subtractOutside(Storage stor)
        //{
        //    Storage find = this.findStorage(stor.getProduct());
        //    if (find == null)
        //        return new Storage(stor);
        //    else
        //        return new Storage(stor.getProduct(), find.subtractOutside(stor).get());
        //}
        //internal void subtract(StorageSet set, bool showMessageAboutNegativeValue = true)
        //{
        //    base.subtract(set, showMessageAboutNegativeValue);
        //    throw new DontUseThatMethod();
        //}

        // removed form ancestor
        //internal void copyDataFrom(StorageSet consumed)
        //{
        //    base.copyDataFrom(consumed);
        //    throw new DontUseThatMethod();
        //}
        internal void sendAll(StorageSet toWhom)
        {
            used.Add(this);
            base.sendAll(toWhom);
        }


    }

}