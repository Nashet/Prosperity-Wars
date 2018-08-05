using Nashet.EconomicSimulation;
using Nashet.Utils;

namespace Nashet.ValueSpace
{
    /// <summary>
    /// Allows to keep info about how much product was taken from StorageSet
    /// !!! if someone would change returning object (Storage) then country takenAway logic would be broken!!
    /// </summary>
    public class CountryStorageSet : StorageSet, IStatisticable
    {
        /// <summary>
        /// Counts how much products was taken from country storage
        /// for consumption or some spending. Shouldn't include sells
        /// Used to determinate how much to buy deficit or sell extra products
        /// </summary>
        public readonly StorageSet used = new StorageSet();

        //public Value getConsumption(Product whom)
        //{
        //    foreach (Storage stor in takenAwayLastTurn)
        //        if (stor.Product == whom)
        //            return stor;
        //    return new Value(0f);
        //}
        public void SetStatisticToZero()
        {
            used.setZero();
            //collection.PerformAction(x => !x.Value.Product.IsStorable, x => x.Value.SetZero());
            Storage record;
            if (collection.TryGetValue(Product.Education, out record))
                record.SetZero();
        }

        /// / next - inherited

        public void set(Storage inn)
        {
            Set(inn);
            throw new DontUseThatMethod();
        }

        ///// <summary>
        ///// If duplicated than adds
        ///// </summary>
        //public void add(Storage need)
        //{
        //    base.add(need);
        //    consumedLastTurn.add(need)
        //}

        ///// <summary>
        ///// If duplicated than adds
        ///// </summary>
        //public void add(PrimitiveStorageSet need)
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

        public override bool Subtract(Storage stor, bool showMessageAboutNegativeValue = true)
        {
            if (base.Subtract(stor, showMessageAboutNegativeValue))
            {
                used.Add(stor);
                return true;
            }
            else
                return false;
        }

        //public void subtract(List<Storage> set, bool showMessageAboutNegativeValue = true)
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
        //public Value getStorage(Product whom)
        //{
        //    return base.getStorage(whom);
        //}

        public void SetZero()
        {
            setZero();
            throw new DontUseThatMethod();
        }

        //public PrimitiveStorageSet Divide(float v)
        //{
        //    PrimitiveStorageSet result = new PrimitiveStorageSet();
        //    foreach (Storage stor in container)
        //        result.Set(new Storage(stor.Product, stor.get() / v));
        //    return result;
        //}

        //public Storage subtractOutside(Storage stor)
        //{
        //    Storage find = this.findStorage(stor.Product);
        //    if (find == null)
        //        return new Storage(stor);
        //    else
        //        return new Storage(stor.Product, find.subtractOutside(stor).get());
        //}
        //public void subtract(StorageSet set, bool showMessageAboutNegativeValue = true)
        //{
        //    base.subtract(set, showMessageAboutNegativeValue);
        //    throw new DontUseThatMethod();
        //}

        // removed form ancestor
        //public void copyDataFrom(StorageSet consumed)
        //{
        //    base.copyDataFrom(consumed);
        //    throw new DontUseThatMethod();
        //}
        public void sendAll(StorageSet toWhom)
        {
            used.Add(this);
            base.sendAll(toWhom);
        }
    }
}