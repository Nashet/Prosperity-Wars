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

// don't really need it. Consumption stored in Consumer.consumedTotal
//public class CountryStorageSet : PrimitiveStorageSet
//{
//    /// <summary>
//    /// Used only in non-market economies. Count as much products country consumed or spent
//    /// Why do I need this???
//    /// </summary>
//    private readonly PrimitiveStorageSet usedLastTurn = new PrimitiveStorageSet();

//    internal Value getConsumption(Product whom)
//    {
//        foreach (Storage stor in usedLastTurn)
//            if (stor.getProduct() == whom)
//                return stor;
//        return new Value(0f);
//    }
//    internal void setStatisticToZero()
//    {
//        usedLastTurn.setZero();
//    }

//    /// / next - inherited


//    public void set(Storage inn)
//    {
//        base.set(inn);
//        throw new DontUseThatMethod();
//    }
//    ///// <summary>
//    ///// If duplicated than adds
//    ///// </summary>
//    //internal void add(Storage need)
//    //{
//    //    base.add(need);
//    //    consumedLastTurn.add(need)
//    //}

//    ///// <summary>
//    ///// If duplicated than adds
//    ///// </summary>
//    //internal void add(PrimitiveStorageSet need)
//    //{ }

//    /// <summary>
//    /// Do checks outside
//    /// </summary>   
//    public bool send(Producer whom, Storage what)
//    {
//        if (base.send(whom, what))
//        {
//            usedLastTurn.add(what); //??!!?
//            return true;
//        }
//        else
//            return false;
//    }
//    /// <summary>
//    /// Do checks outside
//    /// </summary>   
//    public bool send(Producer whom, List<Storage> what)
//    {
//        bool result = true;
//        foreach (var item in what)
//        {
//            if (!send(whom, item))
//                result = false;
//        }
//        return result;
//    }

//    /// <summary>
//    /// //todo !!! if someone would change returning object then country consumption logic would be broken!!
//    /// </summary>    
//    internal Value getStorage(Product whom)
//    {
//        return base.getStorage(whom);
//    }

//    internal void SetZero()
//    {
//        base.setZero();
//        throw new DontUseThatMethod();
//    }
//    //internal PrimitiveStorageSet Divide(float v)
//    //{
//    //    PrimitiveStorageSet result = new PrimitiveStorageSet();
//    //    foreach (Storage stor in container)
//    //        result.Set(new Storage(stor.getProduct(), stor.get() / v));
//    //    return result;
//    //}

//    internal bool subtract(Storage stor, bool showMessageAboutNegativeValue)
//    {
//        if (base.subtract(stor, showMessageAboutNegativeValue))
//        {
//            usedLastTurn.add(stor);
//            return true;
//        }
//        else
//            return false;
//    }

//    //internal Storage subtractOutside(Storage stor)
//    //{
//    //    Storage find = this.findStorage(stor.getProduct());
//    //    if (find == null)
//    //        return new Storage(stor);
//    //    else
//    //        return new Storage(stor.getProduct(), find.subtractOutside(stor).get());
//    //}
//    internal void subtract(PrimitiveStorageSet set)
//    {
//        base.subtract(set, true);
//        throw new DontUseThatMethod();
//    }
//    internal void copyDataFrom(PrimitiveStorageSet consumed)
//    {
//        base.copyDataFrom(consumed);
//        throw new DontUseThatMethod();
//    }
//    internal void sendAll(PrimitiveStorageSet toWhom)
//    {
//        usedLastTurn.add(this);
//        base.sendAll(toWhom);
//    }

//}

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
        if (this.isSameProduct(storage))
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
        return get() + " " + getProduct();

    }
    public void sendAll(PrimitiveStorageSet whom)
    {
        this.send(whom, this);
    }
    public void sendAll(Storage another)
    {
        if (!isSameProduct(another))
            Debug.Log("Attempt to give wrong product");
        else
        {
            another.add(this);
            this.setZero();
        }
    }
    /// <summary>
    /// checks inside (duplicates?)
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
    /// checks inside (duplicates?),
    /// </summary>    
    //public void send(Storage another, float amount)
    //{
    //    if (this.getProduct() != another.getProduct())
    //        Debug.Log("Attempt to give wrong product");
    //    else
    //        base.send(another, amount);
    //}

    /// <summary>
    /// checks inside (duplicates?), returns true if succeeded
    /// </summary>    
    public bool send(Storage reciever, Storage amountToSend, bool showMessageAboutOperationFails = true)
    {
        if (!isSameProduct(reciever))
        {
            Debug.Log("Attempt to give wrong product");
            return false;
        }
        else
        {
            if (this.isBiggerOrEqual(amountToSend))
            {
                subtract(amountToSend);
                reciever.add(amountToSend);
                return true;
            }
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("No enough value to send");
                sendAll(reciever);
                return false;
            }
        }
    }
    public bool has(Product product, Value HowMuch)
    {
        if (!isSameProduct(product))
        {
            // Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        else
            return isBiggerOrEqual(HowMuch);
    }
    public bool has(Storage storage)
    {
        if (!isSameProduct(storage))
        {
            // Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        else
            return isBiggerOrEqual(storage);
    }
    public bool hasSubstitute(Storage storage)
    {
        if (!isSubstituteProduct(storage.getProduct()))
        {
            // Debug.Log("Attempted to pay wrong product!");
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
    public Storage multiplyOutside(Value invalue)
    {
        return new Storage(this.getProduct(), get() * invalue.get());
    }

    internal bool isSameProduct(Storage anotherStorage)
    {
        return this.getProduct() == anotherStorage.getProduct();
    }
    internal bool isSubstituteProduct(Product  product)
    {
        return this.getProduct().isSubstituteFor(product);
    }
    internal bool isAbstractProduct()
    {
        return getProduct().isAbstract();
    }
    internal bool isSameProduct(Product anotherProduct)
    {
        return this.getProduct() == anotherProduct;
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
