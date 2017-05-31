using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;




public class Corps
{
    PopUnit origin;
    int size;
    Procent moral = new Procent(0f);
    PrimitiveStorageSet consumption = new PrimitiveStorageSet();
    internal void initialize(PopUnit origin, int size)
    {
        this.origin = origin;
        this.size = size;
        this.moral.set(0f);
        consumption.setZero();
    }
    public Corps(PopUnit origin, int size)
    {
        initialize(origin, size);
    }
    internal void deleteData()
    {
        size = 0;
        origin = null;
        moral.set(0);
        consumption.setZero();
        //here - delete all links on that object        
    }
    internal void demobilizeFrom(Army army)
    {
        army.remove(this);
        origin.demobilize();
        Pool.ReleaseObject(this);
    }
    public void consume(Country owner)
    {
        var needs = getRealNeeds(owner);
        //float allNeedsAmount = needs.sum();
        //if (allNeedsAmount == 0f)
        //{
        //    consumption.set(1f);
        //}
        //else
        {
            float shortage = 0f;
            foreach (var stor in needs)
            {
                if (owner.storageSet.has(stor))
                {
                    owner.storageSet.subtract(stor);
                    consumption.add(stor);
                }
                else
                    shortage += stor.get();
            }
            //if (shortage == 0f)
            //    consumption.set(1f);
            //else
            //    consumption.set((allNeedsAmount - shortage) / allNeedsAmount);
        }
        //float moralChange = consumption.get() - moral.get();
        float moralChange = getConsumptionProcent(Product.Food, owner).get() - moral.get();
        moralChange = Mathf.Clamp(moralChange, Options.MaxMoralChangePerTic * -1f, Options.MaxMoralChangePerTic);
        if (moral.get() + moralChange < 0)
            moral.set(0f);
        else
            moral.add(moralChange);
    }
    public PrimitiveStorageSet getConsumption()
    {
        return consumption;        
    }
    internal Procent getConsumptionProcent(Product prod, Country country)
    {
        return Procent.makeProcent(consumption.getStorage(prod), getRealNeeds(country).getStorage(prod));
    }
    internal Value getConsumption(Product prod)
    {
        return consumption.getStorage(prod);
    }
    public PrimitiveStorageSet getRealNeeds(Country country)
    {
        Value multiplier = new Value(this.getSize() / 1000f);

        List<Storage> result = new List<Storage>();
        foreach (Storage next in origin.type.getMilitaryNeedsPer1000())
            if (next.getProduct().isInvented(country))
            {
                Storage nStor = new Storage(next.getProduct(), next.get());
                nStor.multiple(multiplier);
                result.Add(nStor);
            }
        //result.Sort(delegate (Storage x, Storage y)
        //{
        //    float sumX = x.get() * Game.market.findPrice(x.getProduct()).get();
        //    float sumY = y.get() * Game.market.findPrice(y.getProduct()).get();
        //    return sumX.CompareTo(sumY);
        //});
        return new PrimitiveStorageSet(result);
    }
    public Procent getMoral()
    {
        return moral;
    }
    //private float getStrenght()
    //{
    //    return getType().getStrenght(); // bonus
    //}
    internal float getStrenght(Army army)
    {
        return getSize() * origin.type.getStrenght() * army.getStrenghtModifier();
    }
    public PopType getType()
    {
        return origin.type;
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
        int sum = size - loss;
        if (sum > 0)
        {
            size = sum;
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
        moral.addPoportionally(getSize(), another.getSize(), Procent.ZeroProcent);
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
    public static class Pool
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

        public static void ReleaseObject(Corps po)
        {
            po.deleteData();
            lock (_available)
            {
                _available.Add(po);
                _inUse.Remove(po);
            }
        }
        //public static IEnumerable<Corps> existing()      
        //{
        //    foreach (Corps f in _inUse)
        //        yield return f;                      
        //}
    }

