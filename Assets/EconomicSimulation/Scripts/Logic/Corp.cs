using System.Collections.Generic;
using System.Text;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    //todo inherit from consumer?
    public class Corps // Consumer
    {
        private PopUnit origin;
        private int size;
        private readonly Procent morale = new Procent(0f);
        private readonly StorageSet consumption = new StorageSet();

        public void initialize(PopUnit origin, int size)
        {
            this.origin = origin;
            this.size = size;
            morale.Set(0f);
            consumption.setZero();
        }

        public Corps(PopUnit origin, int size)//:base(null,null)
        {
            initialize(origin, size);
        }

        public static Corps mobilize(Staff staff, PopUnit origin)
        {
            int howMuch = origin.mobilize(staff);
            if (howMuch > 0)
                return CorpsPool.GetObject(origin, howMuch);
            else
                return null;
        }

        public void reMobilize(Staff staff)
        {
            //int howMuchCanMobilize = getPopUnit().howMuchCanMobilize(staff, null);
            //int change = howMuchCanMobilize - getPopUnit().getMobilized();

            getPopUnit().demobilize();
            getPopUnit().mobilize(staff);
            //if ()
        }

        //public Corps(Corps corps):this(corps.getPopUnit(), corps.getSize())
        //{
        //}
        public void deleteData()
        {
            size = 0;
            origin = null;
            morale.Set(0);
            consumption.setZero();
            //here - delete all links on that object
        }

        public void consume(Country owner)
        {
            var needs = getRealNeeds(owner);

            float shortage = 0f;
            Storage realConsumption = Storage.EmptyProduct;
            foreach (var need in needs)
            {
                if (owner.countryStorageSet.has(need))
                {
                    if (need.isAbstractProduct())
                        // convertToBiggestStorageProduct here are duplicated in this.getConsumptionProcent() (getBiggestStorage())
                        realConsumption = owner.countryStorageSet.convertToBiggestStorage(need);
                    else
                        realConsumption = need;
                    if (realConsumption.isNotZero())
                    {
                        owner.consumeFromCountryStorage(realConsumption, owner);
                        //owner.countryStorageSet.subtract(realConsumption);
                        consumption.Add(realConsumption);
                    }
                }
                else
                {
                    shortage += need.get();
                }
            }

            float moraleChange = getConsumptionProcent(Product.Food, owner).get() - morale.get();
            moraleChange = Mathf.Clamp(moraleChange, Options.ArmyMaxMoralChangePerTic * -1f, Options.ArmyMaxMoralChangePerTic);
            if (morale.get() + moraleChange < 0)
                morale.Set(0f);
            else
                morale.Add(moraleChange);
            if (origin.Type == PopType.Soldiers && morale.isBiggerThan(origin.loyalty))
                morale.Set(origin.loyalty);

            if (morale.isBiggerThan(Procent.HundredProcent))
                morale.Set(1f);
            //if (getPopUnit().loyalty.isSmallerThan(Options.PopMinLoyaltyToMobilizeForGovernment))
            //    Country.demobilize(x => x.getPopUnit() == this);
        }

        public StorageSet getConsumption()
        {
            return consumption;
        }

        public Procent getConsumptionProcent(Product product, Country country)
        {
            // getBiggestStorage here are duplicated in this.consume() (convertToBiggestStorageProduct())
            return new Procent(consumption.getBiggestStorage(product), getRealNeeds(country, product), false);
        }

        public Value getConsumption(Product prod)
        {
            return consumption.GetFirstSubstituteStorage(prod);
        }

        public List<Storage> getRealNeeds(Country country)
        {
            Value multiplier = new Value(getSize() / 1000f);

            List<Storage> result = origin.Type.getMilitaryNeedsPer1000Men(country);
            foreach (Storage next in result)
                next.Multiply(multiplier);
            return result;
        }

        /// <summary>
        /// New value
        /// </summary>
        public Storage getRealNeeds(Country country, Product product)
        {
            if (country.Science.IsInvented(product))
            {
                Storage found = origin.Type.getMilitaryNeedsPer1000Men(country).GetFirstSubstituteStorage(product).Copy();
                if (found.isZero())
                    return found;
                else
                    return found.Multiply(getSize() / 1000f);
            }
            else
                return new Storage(product);
        }

        public Procent getMorale()
        {
            return morale;
        }

        //private float getStrenght()
        //{
        //    return Type.getStrenght(); // bonus
        //}
        public float getStrenght(Army army, float armyStrenghtModifier)
        {
            return getSize() * origin.Type.getStrenght() * armyStrenghtModifier;
        }

        public PopType Type { get { return origin.Type; } }

        //public PopType Type
        //{
        //    return origin.Type;
        //}
        public int getSize()
        {
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(getSize()).Append(" ").Append(origin);
            return sb.ToString();
        }

        public int TakeLoss(int loss, IWayOfLifeChange reason)
        {
            int alive = size - loss;
            if (alive > 0)
            {
                size = alive;
                origin.takeLoss(loss, reason);
                return loss;
            }
            else
            {
                int wasSize = size;
                origin.takeLoss(size, reason);
                size = 0;
                return wasSize;
            }
        }

        public PopUnit getPopUnit()
        {
            return origin;
        }

        public void add(Corps another)
        {
            size += another.getSize();
            morale.AddPoportionally(getSize(), another.getSize(), Procent.ZeroProcent);
        }

        public void setStatisticToZero()
        {
            consumption.setZero();
        }
    }

    // The PooledObject class is the type that is expensive or slow to instantiate,
    // or that has limited availability, so is to be held in the object pool.

    // The Pool class is the most important class in the object pool design pattern. It controls access to the
    // pooled objects, maintaining a list of available objects and a collection of objects that have already been
    // requested from the pool and are still in use. The pool also ensures that objects that have been released
    // are returned to a suitable state, ready for the next time they are requested.
    public static class CorpsPool
    {
        private static List<Corps> _available = new List<Corps>();
        private static List<Corps> _inUse = new List<Corps>();

        public static Corps GetObject(PopUnit origin, int size)
        {
            lock (_available)
            {
                if (_available.Count == 0)
                {
                    Corps po = new Corps(origin, size);
                    _inUse.Add(po);
                    return po;
                }
                else
                {
                    Corps po = _available[0];
                    po.initialize(origin, size);
                    _inUse.Add(po);
                    _available.RemoveAt(0);
                    return po;
                }
            }
        }

        public static void ReleaseObject(Corps corps)
        {
            corps.getPopUnit().demobilize();
            corps.deleteData();
            lock (_available)
            {
                _available.Add(corps);
                _inUse.Remove(corps);
            }
        }

        //public static IEnumerable<Corps> existing()
        //{
        //    foreach (Corps f in _inUse)
        //        yield return f;
        //}
    }
}