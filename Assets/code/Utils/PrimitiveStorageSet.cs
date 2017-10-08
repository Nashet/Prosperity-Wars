using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveStorageSet
{
    //private static Storage tStorage;
    private List<Storage> container = new List<Storage>();
    public PrimitiveStorageSet()
    {
        container = new List<Storage>();
    }
    public PrimitiveStorageSet(List<Storage> incontainer)
    {
        container = incontainer;
    }
    public PrimitiveStorageSet getCopy()
    {
        PrimitiveStorageSet res = new PrimitiveStorageSet();
        foreach (Storage stor in this)
            res.container.Add(new Storage(stor.getProduct(), stor.get()));
        return res;
    }
    public void sort(Comparison<Storage> comparison)
    {
        container.Sort(comparison);
    }
    /// <summary>
    /// If duplicated than overwrites
    /// </summary>    
    public void set(Storage setValue)
    {
        Storage find = this.findStorage(setValue.getProduct());
        if (find == null)
            container.Add(new Storage(setValue));
        else
            find.set(setValue);
    }
    /// <summary>
    /// If duplicated than overwrites
    /// </summary>    
    public void set(Product product, Value value)
    {
        Storage find = this.findStorage(product);
        if (find == null)
            container.Add(new Storage(product, value));
        else
            find.set(value);
    }
    /// <summary>
    /// If duplicated than adds
    /// </summary>
    internal void add(Storage need)
    {
        Storage find = this.findStorage(need.getProduct());
        if (find == null)
            container.Add(new Storage(need));
        else
            find.add(need);
    }
    /// <summary>
    /// If duplicated than adds
    /// </summary>
    internal void add(PrimitiveStorageSet need)
    {
        foreach (Storage n in need)
            this.add(n);
    }
    /// <summary>
    /// If duplicated than adds
    /// </summary>
    internal void add(List<Storage> need)
    {
        foreach (Storage n in need)
            this.add(n);
    }
    public IEnumerator<Storage> GetEnumerator()
    {
        for (int i = 0; i < container.Count; i++)
        {
            yield return container[i];
        }
    }
    public List<Storage> getContainer()
    {
        return container;
    }

    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, Storage what)
    {
        Storage storage = findStorage(what.getProduct());
        if (storage == null)
            return false;
        else
            return storage.send(whom.storage, what);
    }
    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, PrimitiveStorageSet what)
    {
        bool res = true;
        foreach (var item in what)
        {
            if (!send(whom, item))//!has(item) || 
                res = false;
        }
        return res;
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
        Storage foundStorage = findStorage(what.getProduct());
        if (foundStorage == null)
            return false;                                              
        else
            return (foundStorage.isBiggerOrEqual(what)) ? true : false;

    }

    /// <summary>Returns False when some check not presented in here</summary>    
    internal bool has(PrimitiveStorageSet check)
    {
        foreach (Storage stor in check)
            if (!has(stor))
                return false;
        return true;
    }
    /// <summary>Returns False if any item from are not available in that storage</summary>    
    internal bool has(List<Storage> list)
    {
        foreach (Storage stor in list)
            if (!has(stor))
                return false;
        return true;
    }
    internal Procent HowMuchHaveOf(PrimitiveStorageSet need)
    {
        PrimitiveStorageSet shortage = this.subtractOuside(need);
        return Procent.makeProcent(shortage, need);
    }
    
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

    /// <summary>Returns NEW empty storage if search is failed</summary>
    internal Storage getStorage(Product what)
    {
        foreach (Storage storage in container)
            if (storage.isSameProductType(what))
                return storage;
        //if not found
        return new Storage(what, 0f);
    }
    /// <summary>Gets storage if there is enough product of that type. Returns NEW empty storage if search is failed</summary>    
    internal Storage getExistingStorage(Storage what)
    {
        foreach (Storage storage in container)
            if (storage.has(what))
                return storage;
        //if not found
        return new Storage(what.getProduct(), 0f);
    }
    /// <summary>Gets storage if there is enough product of that type. Returns NEW empty storage if search is failed</summary>    
    internal Storage getBiggestStorage(Product what)
    {
        List<Storage> res = new List<Storage>();
        foreach (Storage storage in container)
            if (storage.isSameProductType(what))
                res.Add(storage);
        var found = res.MaxBy(x => x.get());
        if (found == null)
            return new Storage(what, 0f);
        return found;
    }
    override public string ToString()
    {
        return container.getString(", ");
    }
    internal void setZero()
    {
        foreach (Storage st in this)
            st.set(0f);
    }

    internal int Count()
    {
        return container.Count;
    }
    /// <summary>
    /// returns new copy
    /// </summary>    
    internal PrimitiveStorageSet Divide(float v)
    {
        PrimitiveStorageSet result = new PrimitiveStorageSet();
        foreach (Storage stor in container)
            result.set(new Storage(stor.getProduct(), stor.get() / v));
        return result;
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
    internal bool subtract(Storage stor, bool showMessageAboutNegativeValue = true)
    {
        Storage find = this.findStorage(stor.getProduct());
        if (find == null)
            return false;//container.Add(value);
        else
            return find.subtract(stor, showMessageAboutNegativeValue);
    }
    internal Storage subtractOutside(Storage stor)
    {
        Storage find = this.findStorage(stor.getProduct());
        if (find == null)
            return new Storage(stor);
        else
            return new Storage(stor.getProduct(), find.subtractOutside(stor).get());
    }
    internal void subtract(PrimitiveStorageSet set, bool showMessageAboutNegativeValue = true)
    {
        foreach (Storage stor in set)
            this.subtract(stor, showMessageAboutNegativeValue);
    }
    internal void subtract(List<Storage> set, bool showMessageAboutNegativeValue = true)
    {
        foreach (Storage stor in set)
            this.subtract(stor, showMessageAboutNegativeValue);
    }
    internal PrimitiveStorageSet subtractOuside(PrimitiveStorageSet substracting)
    {
        PrimitiveStorageSet result = new PrimitiveStorageSet();
        foreach (Storage stor in substracting)
            result.add(this.subtractOutside(stor));
        return result;
    }

    internal void copyDataFrom(PrimitiveStorageSet consumed)
    {
        foreach (Storage stor in consumed)
            //if (stor.get() > 0f)
            this.set(stor);
        // SetZero();
    }


    internal void sendAll(PrimitiveStorageSet toWhom)
    {
        toWhom.add(this);
        this.setZero();
    }

    internal float sum()
    {
        float result = 0f;
        foreach (var item in container)
            result += item.get();
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
