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
    public class MyPooledObject
    {
        DateTime _createdAt = DateTime.Now;

        public DateTime CreatedAt
        {
            get { return _createdAt; }
        }

        public string TempData { get; set; }
    }

    // The Pool class is the most important class in the object pool design pattern. It controls access to the
    // pooled objects, maintaining a list of available objects and a collection of objects that have already been
    // requested from the pool and are still in use. The pool also ensures that objects that have been released
    // are returned to a suitable state, ready for the next time they are requested. 
    public static class Pool
    {
        private static List<MyPooledObject> _available = new List<MyPooledObject>();
        private static List<MyPooledObject> _inUse = new List<MyPooledObject>();

        public static MyPooledObject GetObject()
        {
            lock (_available)
            {
                if (_available.Count != 0)
                {
                    MyPooledObject po = _available[0];
                    _inUse.Add(po);
                    _available.RemoveAt(0);
                    return po;
                }
                else
                {
                    MyPooledObject po = new MyPooledObject();
                    _inUse.Add(po);
                    return po;
                }
            }
        }

        public static void ReleaseObject(MyPooledObject po)
        {
            CleanUp(po);

            lock (_available)
            {
                _available.Add(po);
                _inUse.Remove(po);
            }
        }

        private static void CleanUp(MyPooledObject po)
        {
            po.TempData = null;
        }
    }

}
public class Corps
{
    PopUnit origin;
    int size;
    static List<Corps> allCorps = new List<Corps>();
    //Pool.;


    public Corps(PopUnit origin, int size)
    {
        this.origin = origin;
        this.size = size;
        allCorps.Add(this);
    }
    public PopType getType()
    { return origin.type; }
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
        origin.kill(loss);

    }

    internal PopUnit getPopUnit()
    {
        return origin;
    }

    internal void add(int v)
    {
        size += v;
    }

    internal void demobilize()
    {
        size = 0;
        origin = null;
        dfdf

            // need to destroy this object
    }
}
