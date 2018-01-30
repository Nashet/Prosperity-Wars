using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    //todo inherit from consumer?
    public class Corps // Consumer
    {
        PopUnit origin;
        int size;
        readonly Procent morale = new Procent(0f);
        readonly StorageSet consumption = new StorageSet();
        internal void initialize(PopUnit origin, int size)
        {
            this.origin = origin;
            this.size = size;
            this.morale.set(0f);
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
        internal void reMobilize(Staff staff)
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
        internal void deleteData()
        {
            size = 0;
            origin = null;
            morale.set(0);
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
                morale.set(0f);
            else
                morale.add(moraleChange);
            if (this.origin.popType == PopType.Soldiers && morale.isBiggerThan(origin.loyalty))
                morale.set(origin.loyalty);

            if (morale.isBiggerThan(Procent.HundredProcent))
                morale.set(1f);
            //if (getPopUnit().loyalty.isSmallerThan(Options.PopMinLoyaltyToMobilizeForGovernment))
            //    getCountry().demobilize(x => x.getPopUnit() == this);
        }
        public StorageSet getConsumption()
        {
            return consumption;
        }
        internal Procent getConsumptionProcent(Product product, Country country)
        {
            // getBiggestStorage here are duplicated in this.consume() (convertToBiggestStorageProduct())
            return Procent.makeProcent(consumption.getBiggestStorage(product), getRealNeeds(country, product), false);
        }
        internal Value getConsumption(Product prod)
        {
            return consumption.GetFirstSubstituteStorage(prod);
        }
        public List<Storage> getRealNeeds(Country country)
        {
            Value multiplier = new Value(this.getSize() / 1000f);

            List<Storage> result = origin.popType.getMilitaryNeedsPer1000Men(country);
            foreach (Storage next in result)
                next.multiply(multiplier);
            return result;
        }
        public Storage getRealNeeds(Country country, Product product)
        {
            if (product.isInventedBy(country))
            {
                Storage found = origin.popType.getMilitaryNeedsPer1000Men(country).GetFirstSubstituteStorage(product);
                if (found.isZero())
                    return found;
                else
                    return new Storage(product, found.multiplyOutside(this.getSize() / 1000f));
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
        //    return getType().getStrenght(); // bonus
        //}
        internal float getStrenght(Army army, float armyStrenghtModifier)
        {
            return getSize() * origin.popType.getStrenght() * armyStrenghtModifier;
        }
        public PopType getType()
        {
            return origin.popType;
        }
        public int getSize()
        {
            return size;
        }
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(getSize()).Append(" ").Append(origin.ToString());
            return sb.ToString();
        }

        internal int TakeLoss(int loss)
        {
            int alive = size - loss;
            if (alive > 0)
            {
                size = alive;
                origin.takeLoss(loss);
                return loss;
            }
            else
            {
                int wasSize = size;
                origin.takeLoss(size);
                size = 0;
                return wasSize;
            }
        }

        internal PopUnit getPopUnit()
        {
            return origin;
        }

        internal void add(Corps another)
        {
            size += another.getSize();
            morale.addPoportionally(getSize(), another.getSize(), Procent.ZeroProcent);
        }

        internal void setStatisticToZero()
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
