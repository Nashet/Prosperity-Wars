using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Had to be class representing ability to sell more than 1 product
/// but actually it contains statistics for Country
/// </summary>
public abstract class MultiSeller : Staff, IHasStatistics
{
    public readonly CountryStorageSet countryStorageSet = new CountryStorageSet();
    private readonly StorageSet sentToMarket = new StorageSet();

    private readonly Dictionary<Product, Storage> sellIfMoreLimits = new Dictionary<Product, Storage>();
    private readonly Dictionary<Product, Storage> buyIfLessLimits = new Dictionary<Product, Storage>();
    /// <summary> Including enterprises, government and everything    </summary>
    private readonly Dictionary<Product, Value> producedTotal = new Dictionary<Product, Value>();
    /// <summary> Shows actual sells, not sent to market   </summary>
    //private readonly Dictionary<Product, Value> soldByGovernment = new Dictionary<Product, Value>();

    public MultiSeller(Country place) : base(place)
    {
        foreach (var item in Product.getAllNonAbstract())
        {
            if (item == Product.Grain)
            {
                buyIfLessLimits.Add(item, new Storage(item, Options.CountryMaxStorage));
                sellIfMoreLimits.Add(item, new Storage(item, Options.CountryMaxStorage));
            }
            else
            {
                buyIfLessLimits.Add(item, new Storage(item, Value.Zero));
                sellIfMoreLimits.Add(item, new Storage(item, Options.CountryMaxStorage));
            }
            producedTotal.Add(item, new Value(0f));
        }
    }
    //bool wantsToBuy?
    /// <summary>
    /// returns exception if failed
    /// </summary>    
    public Storage getSellIfMoreLimits(Product product)
    {
        return sellIfMoreLimits[product];
    }
    /// <summary>
    /// returns exception if failed
    /// </summary>    
    public Storage getBuyIfLessLimits(Product product)
    {
        return buyIfLessLimits[product];
    }
    /// <summary>
    /// returns exception if failed
    /// </summary>    
    public void setSellIfMoreLimits(Product product, float value)
    {
        sellIfMoreLimits[product].set(value);
    }
    /// <summary>
    /// returns exception if failed
    /// </summary>    
    public void setBuyIfLessLimits(Product product, float value)
    {
        buyIfLessLimits[product].set(value);
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        sentToMarket.setZero();
        foreach (var item in producedTotal)
            item.Value.set(Value.Zero);
        //foreach (var item in soldByGovernment)
        //    item.Value.set(Value.Zero);              
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
        //countryStorageSet.subtract(what);
        countryStorageSet.subtractNoStatistic(what); // to avoid getting what in "howMuchUsed" statistics
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
    internal void producedTotalAdd(Storage produced)
    {
        producedTotal.addMy(produced.getProduct(), produced);
    }
    public Value getProducedTotal(Product product)
    {
        return producedTotal[product];
    }
}
