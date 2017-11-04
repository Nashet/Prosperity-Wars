using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represent anyone who can consume (but can't produce by itself)
/// Stores data about last consumption
/// </summary>
public abstract class Consumer : Agent
{
    /// <summary>How much product actually left for now. Stores food, except for Artisans</summary>
    // may move it back to Producer
    public Storage storage;
    private readonly StorageSet consumed = new StorageSet();
    private readonly StorageSet consumedLastTurn = new StorageSet();
    private readonly StorageSet consumedInMarket = new StorageSet();
    /// <summary>
    /// Represents buying and/or cinsuming needs
    /// </summary>
    public abstract void consumeNeeds();    
    public abstract List<Storage> getRealAllNeeds();

    protected Consumer(Bank bank, Province province) : base(0f, bank, province)
    {

    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public StorageSet getConsumedTotal()
    {
        return consumed;
    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public StorageSet getConsumedLastTurn()
    {
        return consumedLastTurn;
    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public StorageSet getConsumedInMarket()
    {
        return consumedInMarket;
    }
    public void consumeFromMarket(Storage what)
    {
        //pay(Game.market, what.multiplyOutside(price));
        //if (fromMarket)
        ///{
        consumed.add(what);
        consumedInMarket.add(what);
        Game.market.sentToMarket.subtract(what);
        //}        

        // from Market
        //if (this is SimpleProduction)
        //    (this as SimpleProduction).getInputProductsReserve().add(what);
    }
    /// <summary> Do checks outside</summary>
    public void consumeFromItself(Storage what)
    {
        consumed.add(what);
        storage.subtract(what);
    }
    //public void consumeFromCountryStorage(List<Storage> what, Country country)
    //{
    //    consumed.add(what);
    //    country.storageSet.subtract(what);
    //    // to track country expenses
    //    if (this != country)
    //        country.consumed.add(what);
    //}
    //public void consumeFromCountryStorage(Storage what, Country country)
    //{
    //    consumed.add(what);
    //    country.storageSet.subtract(what);
    //    // to track country expenses
    //    if (this != country)
    //        country.consumed.add(what);
    //}
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        consumedLastTurn.copyDataFrom(consumed); // temp   
        consumed.setZero();
        consumedInMarket.setZero();
    }

}