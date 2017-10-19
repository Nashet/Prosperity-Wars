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
    private readonly StorageSet consumedTotal = new StorageSet();
    private readonly StorageSet consumedLastTurn = new StorageSet();
    private readonly StorageSet consumedInMarket = new StorageSet();
    public abstract void consumeNeeds();
    public abstract List<Storage> getRealNeeds();

    protected Consumer(Bank bank, Province province) : base(0f, bank, province)
    {

    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public StorageSet getConsumedTotal()
    {
        return consumedTotal;
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
        consumedTotal.add(what);
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
        consumedTotal.add(what);
        storage.subtract(what);
    }
    public void consumeFromCountryStorage(List<Storage> what, Country country)
    {
        consumedTotal.add(what);
        country.storageSet.subtract(what);
    }                            
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        consumedLastTurn.copyDataFrom(consumedTotal); // temp   
        consumedTotal.setZero();
        consumedInMarket.setZero();
    }

}