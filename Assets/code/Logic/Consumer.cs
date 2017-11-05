using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represent anyone who can consume (but can't produce by itself)
/// Stores data about last consumption
/// </summary>
public abstract class Consumer : Agent
{
    
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
    public StorageSet getConsumed()
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
    // Do I use where need to? Yes, I do. It goes to Market.Buy()
    public void consumeFromMarket(Storage what)
    {            
        consumed.add(what);
        consumedInMarket.add(what);
        Game.market.sentToMarket.subtract(what);       
    }
   
    public void consumeFromCountryStorage(List<Storage> what, Country country)
    {
        consumed.add(what);
        country.countryStorageSet.subtract(what);
        
    }
    public void consumeFromCountryStorage(Storage what, Country country)
    {
        consumed.add(what);
        country.countryStorageSet.subtract(what);        
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        consumedLastTurn.copyDataFrom(consumed); // temp   
        consumed.setZero();
        consumedInMarket.setZero();
    }

}