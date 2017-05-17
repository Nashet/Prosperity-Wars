using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DesignPattern.Objectpool;




namespace DesignPattern.Objectpool
{
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

}
public class Corps
{
    PopUnit origin;
    int size;
    Procent moral = new Procent(0f);
    Procent consumption = new Procent(0f);
    internal void initialize(PopUnit origin, int size)
    {
        this.origin = origin;
        this.size = size;
        this.moral.set(0f);
        consumption.set(0f);
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
        //Procent consumption = getConsumption(owner);
        var needs = getRealNeeds();
        float allNeedsAmount = needs.sum();
        if (allNeedsAmount == 0f)
        {
            consumption.set(1f);
        }
        else
        {
            float shortage = 0f;
            foreach (var item in needs)
            {
                if (owner.storageSet.has(item))
                    owner.storageSet.subtract(item);
                else
                    shortage += item.get();
            }
            if (shortage == 0f)
                consumption.set(1f);
            else
                consumption.set(shortage / allNeedsAmount);
        }
        float moralChange = consumption.get() - moral.get();
        moralChange = Mathf.Clamp(moralChange, Options.MaxMoralChangePerTic * -1f, Options.MaxMoralChangePerTic);
        if (moral.get() + moralChange < 0)
            moral.set(0f);
        else
            moral.add(moralChange);
    }
    public Procent getConsumption(Country owner)
    {
        return consumption;
        //return owner.storageSet.HowMuchHaveOf(getRealNeeds());
    }
    public PrimitiveStorageSet getRealNeeds()
    {
        Value multiplier = new Value(this.getSize() / 1000f);

        List<Storage> result = new List<Storage>();
        foreach (Storage next in origin.type.getMilitaryNeedsPer1000())
            if (next.getProduct().isInventedByAnyOne())
            {
                Storage nStor = new Storage(next.getProduct(), next.get());
                nStor.multipleInside(multiplier);
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

    internal float getStrenght()
    {
        return getSize() * origin.type.getStrenght();
    }

    internal void TakeLoss(int loss)
    {
        int sum = size - loss;
        if (sum > 0)
            size = sum;
        else
            size = 0;
        origin.takeLoss(loss);
    }

    internal PopUnit getPopUnit()
    {
        return origin;
    }

    internal void add(int v)
    {
        size += v;
    }

}
