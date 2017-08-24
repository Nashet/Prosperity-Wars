using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//public class CountryWallet : Wallet
//{
//    public CountryWallet(float inAmount, Bank bank) : base(inAmount, bank)
//    {
//       // setBank(country.bank);
//    }


//}

public class CountryStorageSet : PrimitiveStorageSet
{
    PrimitiveStorageSet consumedLastTurn = new PrimitiveStorageSet();

    internal Value getConsumption(Product whom)
    {
        foreach (Storage stor in consumedLastTurn)
            if (stor.getProduct() == whom)
                return stor;
        return new Value(0f);
    }
    internal void setStatisticToZero()
    {
        consumedLastTurn.setZero();
    }

    /// / next - inherited


    public void set(Storage inn)
    {
        base.set(inn);
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
    /// </summary>   
    public bool send(Producer whom, Storage what)
    {
        if (base.send(whom, what))
        {
            consumedLastTurn.add(what);
            return true;
        }
        else
            return false;
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
    //public void take(Storage fromHhom, Storage howMuch)
    //{
    //    base.take(fromHhom, howMuch);
    //    throw new DontUseThatMethod();
    //}
    /// <summary>
    /// //todo !!! if someone would change returning object then country consumption logic would be broken!!
    /// </summary>    
    internal Value getStorage(Product whom)
    {
        return base.getStorage(whom);
    }

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

    internal bool subtract(Storage stor, bool showMessageAboutNegativeValue)
    {
        if (base.subtract(stor, showMessageAboutNegativeValue))
        {
            consumedLastTurn.add(stor);
            return true;
        }
        else
            return false;
    }

    //internal Storage subtractOutside(Storage stor)
    //{
    //    Storage find = this.findStorage(stor.getProduct());
    //    if (find == null)
    //        return new Storage(stor);
    //    else
    //        return new Storage(stor.getProduct(), find.subtractOutside(stor).get());
    //}
    internal void subtract(PrimitiveStorageSet set)
    {
        base.subtract(set, true);
        throw new DontUseThatMethod();
    }
    internal void copyDataFrom(PrimitiveStorageSet consumed)
    {
        base.copyDataFrom(consumed);
        throw new DontUseThatMethod();
    }
    internal void sendAll(PrimitiveStorageSet toWhom)
    {
        consumedLastTurn.add(this);
        base.sendAll(toWhom);
    }

}
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
    /// <param name="inn"></param>
    public void set(Storage inn)
    {
        Storage find = this.findStorage(inn.getProduct());
        if (find == null)
            container.Add(new Storage(inn));
        else
            find.set(inn);
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
            return storage.send(whom.storageNow, what);
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

    public bool has(Storage what)
    {
        Storage foundStorage = findStorage(what.getProduct());
        if (foundStorage != null)
            return (foundStorage.get() >= what.get()) ? true : false;
        else return false;
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
    internal Storage findStorage(Product whom)
    {
        foreach (Storage stor in container)
            if (stor.getProduct() == whom)
                return stor;
        return null;
    }
    /// <summary>Returns NULL if search is failed</summary>
    internal Storage findStorage(Storage whom)
    {
        foreach (Storage stor in container)
            if (stor.getProduct() == whom.getProduct())
                return stor;
        return null;
    }
    /// <summary>Returns NEW empty storage if search is failed</summary>
    internal Storage getStorage(Product whom)
    {
        foreach (Storage stor in container)
            if (stor.getProduct() == whom)
                return stor;
        return new Storage(whom, 0f);
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



    //internal PrimitiveStorageSet Copy()
    //{
    //    oldList.ForEach((item) =>
    //    {
    //        newList.Add(new YourType(item));
    //    });
    //}
}
public class Storage : Value
{
    private Product product;
    // protected  Value value;
    //public Value value;
    //public Storage(JSONObject jsonObject)
    //{
    //    //  Auto-generated constructor stub
    //}
    public Storage(Product inProduct, float inAmount, bool showMessageAboutNegativeValue = true) : base(inAmount, showMessageAboutNegativeValue)
    {
        product = inProduct;
        //value = new Value(inAmount);
        // TODO exceptions!!
    }
    public Storage(Product inProduct, Value inAmount) : base(inAmount)
    {
        product = inProduct;
    }

    public Storage(Product product) : this(product, 0f)
    {

    }
    public Storage(Storage storage) : this(storage.getProduct(), storage)
    {

    }
    static public int CostOrder(Storage x, Storage y)
    {
        //eats less memory
        float sumX = x.get() * Game.market.findPrice(x.getProduct()).get();
        float sumY = y.get() * Game.market.findPrice(y.getProduct()).get();
        return sumX.CompareTo(sumY);

        //return Game.market.getCost(x).get().CompareTo(Game.market.getCost(y).get());
    }
    public void set(Product inProduct, float inAmount, bool showMessageAboutNegativeValue = true)
    {
        product = inProduct;
        set(inAmount, showMessageAboutNegativeValue);
    }
    public void set(Storage storage)
    {
        product = storage.getProduct();
        base.set(storage);
    }
    //[System.Obsolete("Method is deprecated, need product specified")]
    //override public void set(Value invalue)
    //{
    //    throw new DontUseThatMethod();
    //}
    //[System.Obsolete("Method is deprecated, need product specified")]
    //override public void set(float inAmount, bool showMessageAboutOperationFails = true)
    //{
    //    // need product specified
    //    throw new DontUseThatMethod();
    //}
    [System.Obsolete("Method is deprecated, need product specified")]
    override public void add(Value invalue, bool showMessageAboutNegativeValue = true)
    {
        throw new DontUseThatMethod();
    }
    [System.Obsolete("Method is deprecated, need product specified")]
    override public void add(float invalue, bool showMessageAboutNegativeValue = true)
    {
        throw new DontUseThatMethod();
    }
    public void add(Storage storage, bool showMessageAboutNegativeValue = true)
    {
        if (storage.getProduct() == this.getProduct())
            base.add(storage, showMessageAboutNegativeValue);
        else
        {
            if (showMessageAboutNegativeValue)
                Debug.Log("Attempt to add wrong product to Storage");
        }
    }
    public Product getProduct()
    {
        return product;
    }

    override public string ToString()
    {
        return get() + " " + getProduct().getName();

    }
    public void sendAll(PrimitiveStorageSet whom)
    {
        this.send(whom, this);
    }
    public void sendAll(Storage another)
    {
        if (this.getProduct() != another.getProduct())
            Debug.Log("Attempt to give wrong product");
        else
            base.sendAll(another);
    }
    /// <summary>
    /// checks inside (duplicates?), returns true if succeeded
    /// </summary>    
    public void send(PrimitiveStorageSet whom, Storage howMuch)
    {
        if (has(howMuch))
        {
            Storage targetStorage = new Storage(howMuch);
            whom.add(targetStorage);
            this.subtract(howMuch);
        }
    }
    /// <summary>
    /// checks inside (duplicates?), returns true if succeeded
    /// </summary>    
    public void send(Storage another, float amount)
    {
        if (this.getProduct() != another.getProduct())
            Debug.Log("Attempt to give wrong product");
        else
            base.send(another, amount);
    }

    /// <summary>
    /// checks inside (duplicates?), returns true if succeeded
    /// </summary>    
    public bool send(Storage toWhom, Storage amount, bool showMessageAboutOperationFails = true)
    {
        if (this.getProduct() != toWhom.getProduct())
        {
            Debug.Log("Attempt to give wrong product");
            return false;
        }
        else
        {
            //base.send(toWhom, amount, showMessageAboutOperationFails);
            //return true;
            if (this.get() >= amount.get())
            {
                subtract(amount);
                toWhom.add(amount);
                return true;
            }
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("No enough value to send");
                sendAll(toWhom);
                return false;
            }
        }
    }
    public bool has(Product product, Value HowMuch)
    {
        if (this.getProduct() != product)
        {
            Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        else
            return isBiggerOrEqual(HowMuch);
    }
    public bool has(Storage storage)
    {
        if (this.getProduct() != storage.getProduct())
        {
            Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        else
            return isBiggerOrEqual(storage);
    }
    internal Storage multiplyOutside(float invalue, bool showMessageAboutOperationFails = true)
    {
        if (invalue < 0f)
        {
            if (showMessageAboutOperationFails)
                Debug.Log("Storage multiply failed");
            return new Storage(this.getProduct(), 0f);
        }
        else
            return new Storage(this.getProduct(), get() * invalue);
    }
    /// <summary>
    /// returns new value
    /// </summary>    
    public Storage multiplyOutside(Value invalue, bool showMessageAboutNegativeValue = true)
    {
        if (invalue.get() < 0)
        {
            if (showMessageAboutNegativeValue)
                Debug.Log("Value multiply failed");
            return new Storage(this.getProduct(), 0f);
        }
        else
            return new Storage(this.getProduct(), get() * invalue.get());
    }

    internal bool isSameProduct(Storage anotherStorage)
    {
        return this.getProduct() == anotherStorage.getProduct();
    }
    //[System.Obsolete("Method is deprecated, need product specified")]
    //override public Value multipleOutside(float invalue, bool showMessageAboutOperationFails = true)
    //{
    //    throw new DontUseThatMethod();       
    //}    
    //[System.Obsolete("Method is deprecated, need product specified")]
    //override public Value multipleOutside(Value invalue, bool showMessageAboutNegativeValue = true)
    //{     
    //    throw new DontUseThatMethod();        
    //}
}
