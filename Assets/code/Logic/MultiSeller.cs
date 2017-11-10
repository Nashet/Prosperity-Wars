using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiSeller : Staff, IHasStatistics
{
    public readonly CountryStorageSet countryStorageSet = new CountryStorageSet();
    private readonly StorageSet sentToMarket = new StorageSet();

    private readonly Dictionary<Product, Storage> sellIfMoreLimits = new Dictionary<Product, Storage>();
    private readonly Dictionary<Product, Storage> buyIfLessLimits = new Dictionary<Product, Storage>();

    public MultiSeller(Country place) : base(place)
    {
        foreach (var item in Product.getAllNonAbstract())
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
