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

    public void take(Storage fromHhom, Value howMuch)
    {
        base.take(fromHhom, howMuch);
        throw new DontUseThatMethod();
    }
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
    public void Sort(Comparison<Storage> comparison)
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
    public IEnumerator<Storage> GetEnumerator()
    {
        for (int i = 0; i < container.Count; i++)
        {
            yield return container[i];
        }
    }
    //// Implementing the enumerable pattern
    //public IEnumerable SampleIterator(int start, int end)
    //{
    //    for (int i = start; i <= end; i++)
    //    {
    //        yield return i;
    //    }
    //}

    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, Storage what)
    {
        Storage storage = findStorage(what.getProduct());
        return storage.send(whom.storageNow, what);
    }

    public void take(Storage fromHhom, Value howMuch)
    {
        Storage stor = findStorage(fromHhom.getProduct());
        if (stor == null)
        {
            stor = new Storage(fromHhom.getProduct());
            container.Add(stor);
        }

        fromHhom.send(stor, howMuch);
        //fromHhom.


        //stor.pay(fromHhom, howMuchPay);
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

        if (container.Count > 0)
        {
            Game.threadDangerSB.Clear();
            foreach (Storage stor in container)
                if (stor.get() > 0)
                {
                    Game.threadDangerSB.Append(stor.ToString());
                    Game.threadDangerSB.Append("; ");
                }
            return Game.threadDangerSB.ToString();
        }
        else return "none";
    }
    public string ToStringWithLines()
    {

        if (container.Count > 0)
        {
            Game.threadDangerSB.Clear();
            foreach (Storage stor in container)
                if (stor.get() > 0)
                {
                    Game.threadDangerSB.AppendLine();
                    Game.threadDangerSB.Append(stor.ToString());
                }
            return Game.threadDangerSB.ToString();
        }
        else return "none";
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
    internal void subtract(PrimitiveStorageSet set, bool showMessageAboutNegativeValue)
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

    internal void add(object p)
    {
        throw new NotImplementedException();
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
    public Storage(Product inProduct, float inAmount) : base(inAmount)
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

        //value = new Value(0);
    }
    public Storage(Storage storage) : this(storage.getProduct(), storage.get())
    {
        //    this.Storage();
        //value = new Value(0);
    }
    public void set(Product inProduct, float inAmount)
    {
        product = inProduct;
        //value = new Value(inAmount);
        set(inAmount);
    }
    //public void set(Value inAmount)
    //{
    //    //value = inAmount;
    //    set(inAmount);
    //}
    //public void set(float inAmount)
    //{
    //    set(inAmount);
    //    //value = new Value(inAmount);
    //}
    public Product getProduct()
    {
        return product;
    }
    //public float getValue()
    //{
    //    return value.get();
    //}
    //public void add(float amount)
    //{
    //    this.value += amount;
    //}
    //void setValue(float value)
    //{
    //    this.value = value; ;
    //}
    override public string ToString()
    {
        return get() + " " + getProduct().getName();

    }
    public void sendAll(PrimitiveStorageSet storage)
    {
        storage.take(this, this);
    }
    public void sendAll(Storage another)
    {
        if (this.getProduct() != another.getProduct())
            Debug.Log("Attempt to give wrong product");
        else
            base.sendAll(another);
    }
    public void send(PrimitiveStorageSet whom, Value HowMuch)
    {
        whom.take(this, HowMuch);
    }
    /// <summary>
    /// Checks inside
    /// </summary>   
    public void send(Storage another, float amount)
    {
        if (this.getProduct() != another.getProduct())
            Debug.Log("Attempt to give wrong product");
        else
            base.send(another, amount);
        //{
        //    if (this.get() >= amount)
        //    {
        //        another.add(amount);
        //        this.subtract(amount);

        //    }
        //    else
        //        Debug.Log("value payment failed");
        //}
    }
    /// <summary>
    /// checks inside, returns true if succeeded
    /// </summary>    
    //public bool send(Producer toWhom, Storage what)
    //{
    //    if (this.getProduct() != toWhom.storageNow.getProduct())
    //    {
    //        Debug.Log("Attempt to give wrong product in bool send(Producer toWhom, Storage what)");
    //        return false;
    //    }
    //    if (this.get() >= what.get())
    //    {
    //        toWhom.storageNow.add(what);
    //        this.subtract(what);
    //        return true;
    //    }
    //    else
    //    {
    //        Debug.Log("value payment failed");
    //        return false;
    //    }

    //}
    /// <summary>
    /// checks inside, returns true if succeeded
    /// </summary>    
    public bool send(Storage toWhom, Value amount)
    {
        if (this.getProduct() != toWhom.getProduct())
        {
            Debug.Log("Attempt to give wrong product");
            return false;
        }
        else
        {
            return base.send(toWhom, amount);
        }
        //if (this.get() >= amount.get())
        //{
        //    toWhom.add(amount);
        //    this.subtract(amount);
        //    return true;
        //}
        //else
        //{
        //    Debug.Log("value payment failed");
        //    return false;
        //}

    }

    //public void pay(Storage another, float amount)
    //{
    //    if (this.get() >= amount)
    //    {
    //        this.subtract(amount);
    //        another.add(amount);
    //    }
    //    else Debug.Log("value payment failed");
    //}
    public bool has(Storage Whom, Value HowMuch)
    {
        if (this.getProduct() != Whom.getProduct())
        {
            Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        else
            return has(HowMuch);
        //if (this.get() < HowMuch.get()) return false;
        //else return true;


    }
    /*public String toString(){
   return getProduct().getName();

}*/
}
