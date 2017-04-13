using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Wallet// : Value // : Storage
{
    /// <summary>
    /// Must be filled together with wallet
    /// </summary>
    public Value moneyIncomethisTurn = new Value(0);
    internal Value haveMoney = new Value(0);

    public Wallet(float inAmount) //: base (inAmount)//: base(Product.findByName("Gold"), inAmount)
    {
        haveMoney.set(inAmount);
    }
    ///public Wallet() : base(Product.findByName("Gold"), 0f)
    //public Wallet() : base(Product.findByName("Gold"), 20f) 

    //}

    internal bool CanAfford(Storage need)
    {
        if (need.get() == HowMuchCanAfford(need).get())
            return true;
        else
            return false;
    }

    internal bool CanAfford(PrimitiveStorageSet need)
    {
        foreach (Storage stor in need)
        {
            if (HowMuchCanAfford(stor).get() < stor.get())
                return false;
        }
        return true;
    }
    /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
    
    internal Value HowMuchCanNotAfford(PrimitiveStorageSet need)
    {
        return new Value (Game.market.getCost(need).get() - this.haveMoney.get());
    }

    internal Storage HowMuchCanAfford(Storage need)
    {
        float price = Game.market.findPrice(need.getProduct()).get();
        float cost = need.get() * price;
        if (cost <= haveMoney.get())
            return new Storage(need.getProduct(), need.get());
        else
            return new Storage(need.getProduct(), haveMoney.get() / price);
    }

    //private float get()
    //{
    //    throw new NotImplementedException();
    //}

    internal bool canPay(Value howMuchPay)
    {
        if (this.haveMoney.get() >= howMuchPay.get())
            return true;
        else return false;
    }
    internal bool canPay(float howMuchPay)
    {
        if (this.haveMoney.get() >= howMuchPay)
            return true;
        else
            return false;
    }

    //internal void pay(Wallet whom, float howMuch)
    //{
    //    if (canPay(howMuch))
    //    {
    //        whom.haveMoney.add(howMuch);
    //        whom.moneyIncomethisTurn.add(howMuch);
    //        this.haveMoney.subtract(howMuch);

    //    }
    //    else
    //        Debug.Log("Failed payment in wallet");
    //}
    internal void payWithoutRecord(Wallet whom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            whom.haveMoney.add(howMuch);
            //whom.moneyIncomethisTurn.add(howMuch);
            this.haveMoney.subtract(howMuch);
        }
        else
            Debug.Log("Failed payment in wallet");
    }
    internal void pay(Wallet whom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            whom.haveMoney.add(howMuch);
            whom.moneyIncomethisTurn.add(howMuch);
            this.haveMoney.subtract(howMuch);
        }
        else
            Debug.Log("Failed payment in wallet");
    }
    internal void sendAll(Wallet whom)
    {
        whom.haveMoney.add(this.haveMoney);
        whom.moneyIncomethisTurn.add(this.haveMoney);
        this.haveMoney.set(0);
    }
    public void ConvertFromGoldAndAdd(Value gold)
    {
        float coins = gold.get() * Game.goldToCoinsConvert;
        this.haveMoney.add(coins);
        this.moneyIncomethisTurn.add(coins);
        gold.set(0);

    }

    override public string ToString()
    {
        return haveMoney.get() + " coins";
    }
}
public class CountryStorage : PrimitiveStorageSet
{
    static uint reserveMultiplier = 10;
    Value consumedLastTurn = new Value(0);
    Value wantedConsumeLastTurn = new Value(0);
    public Value HowMuchWantsToBuy()
    {
        return wantedConsumeLastTurn.multiple(reserveMultiplier);
    }
    //public CountryStorage(Product inProduct, float inAmount) : base(inProduct, inAmount)
    //{

    //}

    //public CountryStorage(Product product) : base(product)
    //{

    //}
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
    public void Set(Storage inn)
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
    public System.Collections.IEnumerator GetEnumerator()
    {
        for (int i = 0; i < container.Count; i++)
        {
            yield return container[i];
        }
    }
    //// Implementing the enumerable pattern
    //public System.Collections.IEnumerable SampleIterator(int start, int end)
    //{
    //    for (int i = start; i <= end; i++)
    //    {
    //        yield return i;
    //    }
    //}
    //public System.Collections.IEnumerator GetEnumerator2()
    //{
    //    yield return "With an iterator, ";
    //    yield return "more than one ";
    //    yield return "value can be returned";
    //    yield return ".";
    //}
    /// <summary>
    /// Do checks outside
    /// </summary>
    /// <param name="whom"></param>
    /// <param name="howMuchPay"></param>
    public void pay(Storage whom, Value howMuchPay)
    {
        Storage storage = findStorage(whom.getProduct());
        storage.pay(whom, howMuchPay);
    }

    public void take(Storage fromHhom, Value howMuch)
    {
        Storage stor = findStorage(fromHhom.getProduct());
        if (stor == null)
        {
            stor = new Storage(fromHhom.getProduct());
            container.Add(stor);
        }

        fromHhom.pay(stor, howMuch);
        //fromHhom.


        //stor.pay(fromHhom, howMuchPay);
    }
    public bool has(Storage what)
    {
        Storage foundStorage = findStorage(what.getProduct());
        if (foundStorage != null)
            return (foundStorage.get() >= what.get()) ? true : false;
        else return false;
        //if (t == null) return false;
        //if (t.get() < what.get()) return false;
        //else return true;


    }
    internal Storage findStorage(Product whom)
    {
        foreach (Storage stor in container)
            if (stor.getProduct() == whom)
                return stor;
        return null;
    }
    override public string ToString()
    {
        
        if (container.Count > 0)
        {
            string result = "";
            foreach (Storage stor in container)
                if (stor.get() > 0)
                    result += stor.get() + " " + stor.getProduct().ToString() + ";";
            return result;
        }
        else return "none";
    }

    internal void SetZero()
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
            result.Set(new Storage(stor.getProduct(), stor.get() / v));
        return result;
    }

    internal void subtract(Storage stor)
    {
        Storage find = this.findStorage(stor.getProduct());
        if (find == null)
            ;//container.Add(value);
        else
            find.subtract(stor);
    }

    internal void copyDataFrom(PrimitiveStorageSet consumed)
    {
        foreach (Storage stor in consumed)
            //if (stor.get() > 0f)
                this.Set(stor);
        // SetZero();
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
            Debug.Log("Attemp to give wrong product");
        this.pay(another, this);

    }
    public void pay(PrimitiveStorageSet whom, Value HowMuch)
    {
        whom.take(this, HowMuch);
    }
    /// <summary>
    /// Checks inside
    /// </summary>
    /// <param name="another"></param>
    /// <param name="amount"></param>
    public void pay(Storage another, float amount)
    {
        if (this.getProduct() != another.getProduct())
            Debug.Log("Attemp to give wrong product");
        if (this.get() >= amount)
        {
            another.add(amount);
            this.subtract(amount);

        }
        else
            Debug.Log("value payment failed");
    }
    /// <summary>
    /// checks inside
    /// </summary>
    /// <param name="another"></param>
    /// <param name="amount"></param>
    public void pay(Storage another, Value amount)
    {
        if (this.getProduct() != another.getProduct())
            Debug.Log("Attemp to give wrong product");
        if (this.get() >= amount.get())
        {
            another.add(amount);
            this.subtract(amount);

        }
        else Debug.Log("value payment failed");
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
    public bool canPay(Storage Whom, Value HowMuch)
    {
        if (this.getProduct() != Whom.getProduct())
        {
            Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        if (this.get() < HowMuch.get()) return false;
        else return true;


    }
    /*public String toString(){
   return getProduct().getName();

}*/
}
