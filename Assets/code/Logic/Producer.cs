using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiSeller : Staff, IHasStatistics
{
    public readonly CountryStorageSet countryStorageSet = new CountryStorageSet();
    private readonly StorageSet sentToMarket = new StorageSet();
    public MultiSeller(Country place) : base(place)
    {
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        sentToMarket.setZero();
    }
    public float getSentToMarket(Product product)
    {
        return sentToMarket.getFirstStorage(product).get();
    }
    /// <summary>
    /// Do checks outside
    /// </summary>    
    public void sell(Storage what)
    {
        sentToMarket.add(what);
        countryStorageSet.subtract(what);
        Game.market.sentToMarket.add(what);
    }
    public void getMoneyForSoldProduct()
    {
        foreach (var sent in sentToMarket)
            if (sent.isNotZero())
            {
                Value DSB = new Value(Game.market.getDemandSupplyBalance(sent.getProduct()));
                if (DSB.get() > 1f)
                    DSB.set(1f);
                Storage realSold = new Storage(sent);
                realSold.multiply(DSB);
                Value cost = Game.market.getCost(realSold);

                //returning back unsold product
                if (sent.isBiggerThan(realSold))
                {
                    var unSold = sent.subtractOutside(realSold);
                    countryStorageSet.add(unSold);
                }


                if (Game.market.canPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
                {
                    Game.market.pay(this, cost);
                    //Game.market.sentToMarket.subtract(realSold);
                }
                else if (Game.market.howMuchMoneyCanNotPay(cost).get() > 10f)
                    Debug.Log("Failed market - producer payment: " + Game.market.howMuchMoneyCanNotPay(cost)); // money in market ended... Only first lucky get money
            }
    }
}

/// <summary>
/// Represents anyone who can produce, store and sell product (1 product)
/// also linked to Province
/// </summary>
public abstract class Producer : Consumer
{
    /// <summary>How much was gained (before any payments). Not money!! Generally, gets value in PopUnit.produce and Factore.Produce </summary>
    public Storage gainGoodsThisTurn;

    /// <summary>How much product actually left for now. Stores food, except for Artisans</summary>    
    public Storage storage;

    /// <summary>How much sent to market, Some other amount could be consumedTotal or stored for future </summary>
    public Storage sentToMarket;

    /// <summary> /// Return in pieces  /// </summary>    
    //public abstract float getLocalEffectiveDemand(Product product);

    public abstract void produce();
    public abstract void payTaxes();

    protected Producer(Province province) : base(province.getCountry().getBank(), province)
    {
    }
    //protected Producer() : base(null, null)
    //{
    //}
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        if (gainGoodsThisTurn != null)
            gainGoodsThisTurn.setZero();
        if (sentToMarket != null)
            sentToMarket.setZero();
    }
    public Value getProducing()
    {
        return gainGoodsThisTurn;
    }
    public void getMoneyForSoldProduct()
    {
        if (sentToMarket.get() > 0f)
        {
            Value DSB = new Value(Game.market.getDemandSupplyBalance(sentToMarket.getProduct()));
            if (DSB.get() > 1f) DSB.set(1f);
            Storage realSold = new Storage(sentToMarket);
            realSold.multiply(DSB);
            Value cost = Game.market.getCost(realSold);

            // adding unsold product
            // assuming gainGoodsThisTurn & realSold have same product
            if (storage.isExactlySameProduct(gainGoodsThisTurn))
                storage.add(gainGoodsThisTurn);
            else
                storage = new Storage(gainGoodsThisTurn);
            storage.subtract(realSold.get());

            if (Game.market.canPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
            {
                Game.market.pay(this, cost);
                //Game.market.sentToMarket.subtract(realSold);
            }
            else if (Game.market.howMuchMoneyCanNotPay(cost).get() > 10f)
                Debug.Log("Failed market - producer payment: " + Game.market.howMuchMoneyCanNotPay(cost)); // money in market ended... Only first lucky get money
        }
    }
    /// <summary>
    /// Do checks outside
    /// </summary>    
    public void sell(Storage what)
    {
        sentToMarket.set(what);
        storage.subtract(what);
        Game.market.sentToMarket.add(what);
    }
    /// <summary> Do checks outside</summary>
    public void consumeFromItself(Storage what)
    {
        getConsumed().add(what);
        storage.subtract(what);
    }
}


