using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// Allows to keep info about how much product was taken from StorageSet
/// !!! if someone would change returning object (Storage) then country takenAway logic would be broken!!
/// </summary>
public class CountryStorageSet : StorageSet, IHasStatistics
{
    /// <summary>
    /// Counts how much products was taken from country storage 
    /// for consumption or some spending
    /// Used to determinate how much to buy deficit or sell extra products
    /// </summary>
    public readonly StorageSet takenAway = new StorageSet();

    //internal Value getConsumption(Product whom)
    //{
    //    foreach (Storage stor in takenAwayLastTurn)
    //        if (stor.getProduct() == whom)
    //            return stor;
    //    return new Value(0f);
    //}
    public void setStatisticToZero()
    {
        takenAway.setZero();
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
    /// Supports takenAway
    /// </summary>   
    public bool send(Producer whom, Storage what)
    {
        if (base.send(whom, what))
        {
            takenAway.add(what);
            return true;
        }
        else
            return false;
    }
    /// <summary>
    /// Do checks outside Check for taken away
    /// </summary>   
    //public bool send(Producer whom, List<Storage> what)
    //{
    //    bool result = true;
    //    foreach (var item in what)
    //    {
    //        if (!send(whom, item))
    //            result = false;
    //    }
    //    return result;           
    //}
    /// <summary>
    /// Do checks outside
    /// Supports takenAway
    /// </summary>   
    public bool send(StorageSet whom, StorageSet what)
    {
        return send(whom, what.getContainer());
    }
    /// <summary>
    /// Do checks outside
    /// Supports takenAway
    /// </summary>   
    public bool send(StorageSet whom, List<Storage> what)
    {
        takenAway.add(what);
        return base.send(whom, what);
    }
    /// <summary>
    /// 
    /// /// Supports takenAway
    /// </summary>

    override public bool subtract(Storage stor, bool showMessageAboutNegativeValue = true)
    {
        if (base.subtract(stor, showMessageAboutNegativeValue))
        {
            takenAway.add(stor);
            return true;
        }
        else
            return false;
    }
    /// <summary>
    /// //todo !!! if someone would change returning object then country consumption logic would be broken!!
    /// </summary>    
    //internal Value getStorage(Product whom)
    //{
    //    return base.getStorage(whom);
    //}

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



    //internal Storage subtractOutside(Storage stor)
    //{
    //    Storage find = this.findStorage(stor.getProduct());
    //    if (find == null)
    //        return new Storage(stor);
    //    else
    //        return new Storage(stor.getProduct(), find.subtractOutside(stor).get());
    //}
    //internal void subtract(StorageSet set, bool showMessageAboutNegativeValue = true)
    //{
    //    base.subtract(set, showMessageAboutNegativeValue);
    //    throw new DontUseThatMethod();
    //}
    internal void copyDataFrom(StorageSet consumed)
    {
        base.copyDataFrom(consumed);
        throw new DontUseThatMethod();
    }
    internal void sendAll(StorageSet toWhom)
    {
        takenAway.add(this);
        base.sendAll(toWhom);
    }

    
}

public class Storage : Value
{
    static public readonly Storage EmptyProduct = new Storage(Product.Grain, 0f);

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
        float sumX = x.get() * Game.market.getPrice(x.getProduct()).get();
        float sumY = y.get() * Game.market.getPrice(y.getProduct()).get();
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
        base.add(invalue, showMessageAboutNegativeValue);
        //throw new DontUseThatMethod(); temporally
    }
    public void add(Storage storage, bool showMessageAboutNegativeValue = true)
    {
        if (this.isExactlySameProduct(storage))
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
    public void sendAll(StorageSet whom)
    {
        this.send(whom, this);
    }
    public void sendAll(Storage another)
    {
        if (!isExactlySameProduct(another))
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
    public void send(StorageSet whom, Storage howMuch)
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
        if (!isExactlySameProduct(reciever))
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
    /// <summary> Returns true if has that good or it's substitute</summary>    
    public bool has(Storage storage)
    {
        if (!isSameProductType(storage))
        {
            // Debug.Log("Attempted to pay wrong product!");
            return false;
        }
        else
            return isBiggerOrEqual(storage);
    }
    //public bool hasSubstitute(Storage storage)
    //{
    //    if (!isSubstituteProduct(storage.getProduct()))
    //    {
    //        // Debug.Log("Attempted to pay wrong product!");
    //        return false;
    //    }
    //    else
    //        return isBiggerOrEqual(storage);
    //}

    /// <summary> Returns true if products exactly same or this is substitute for anotherStorage</summary>    
    internal bool isSameProductType(Storage anotherStorage)
    {
        return this.getProduct().isSameProduct(anotherStorage.getProduct());
    }
    /// <summary> Returns true if products exactly same or this is substitute for anotherProduct</summary>
    public bool isSameProductType(Product anotherProduct)
    {
        return this.getProduct().isSameProduct(anotherProduct);
    }
    /// <summary> Returns true only if products exactly same. Does not coiunt substitutes</summary>    
    internal bool isExactlySameProduct(Storage anotherStorage)
    {
        return this.getProduct() == anotherStorage.getProduct();
    }
    /// <summary> Returns true only if products exactly same. Does not coiunt substitutes</summary>    
    public bool isExactlySameProduct(Product anotherProduct)
    {
        return this.getProduct() == anotherProduct;
    }

    //internal bool isSubstituteProduct(Product product)
    //{
    //    return this.getProduct().isSubstituteFor(product);
    //}
    internal bool isAbstractProduct()
    {
        return getProduct().isAbstract();
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
    public Storage subtractOutside(Storage storage, bool showMessageAboutNegativeValue = true)
    {
        //if (!this.isSameProductType(storage.getProduct()))
        if (!storage.isSameProductType(this.getProduct()))
        {
            Debug.Log("Storage subtrackOutside failed - wrong product");
            return new Storage(getProduct(), 0f);
        }
        if (storage.isBiggerThan(this))
        {
            if (showMessageAboutNegativeValue)
                Debug.Log("Storage subtrackOutside failed");
            return new Storage(getProduct(), 0f);
        }
        else
            return new Storage(getProduct(), this.get() - storage.get());
    }
}
