using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ICanSell
{
    Storage getSentToMarket(Product product);
    void sell(Storage what);
    void getMoneyForSoldProduct();
}
/// <summary>
/// Had to be class representing ability to sell more than 1 product
/// but actually it contains statistics for Country
/// </summary>
public abstract class MultiSeller : Staff, IHasStatistics, ICanSell
{
    public readonly CountryStorageSet countryStorageSet = new CountryStorageSet();
    private readonly StorageSet sentToMarket = new StorageSet();

    private readonly Dictionary<Product, Storage> sellIfMoreLimits = new Dictionary<Product, Storage>();
    private readonly Dictionary<Product, Storage> buyIfLessLimits = new Dictionary<Product, Storage>();
    /// <summary> Including enterprises, government and everything    </summary>
    private readonly Dictionary<Product, Value> producedTotal = new Dictionary<Product, Value>();
    /// <summary> Shows actual sells, not sent to market   </summary>
    private readonly Dictionary<Product, Value> soldByGovernment = new Dictionary<Product, Value>();

    public MultiSeller(Country place) : base(place)
    {
        foreach (var item in Product.getAllNonAbstract())
            if (item != Product.Gold)
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
                soldByGovernment.Add(item, new Value(0f));
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
        foreach (var item in soldByGovernment)
            item.Value.set(Value.Zero);
    }

    public Storage getSentToMarket(Product product)
    {
        return sentToMarket.getFirstStorage(product);
    }
    /// <summary> Assuming product is abstract product</summary>
    public Storage getSentToMarketIncludingSubstituts(Product product)
    {
        var res = new Value(0f);
        foreach (var item in product.getSubstitutes())
            if (item.isTradable())
            {
                res.add(sentToMarket.getFirstStorage(item));
            }
        return new Storage(product, res);
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
                if (DSB.get() == Options.MarketInfiniteDSB)
                    DSB.setZero();// real DSB is unknown
                else
                if (DSB.get() > Options.MarketEqualityDSB)
                    DSB.set(Options.MarketEqualityDSB);
                Storage realSold = new Storage(sent);
                realSold.multiply(DSB);
                if (realSold.isNotZero())
                {
                    Value cost = Game.market.getCost(realSold);
                    //soldByGovernment.addMy(realSold.getProduct(), realSold);
                    soldByGovernment[realSold.getProduct()].set(realSold);
                    //returning back unsold product
                    //if (sent.isBiggerThan(realSold))
                    //{
                    //    var unSold = sent.subtractOutside(realSold);
                    //    countryStorageSet.add(unSold);
                    //}


                    if (Game.market.canPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
                    {
                        Game.market.pay(this, cost);
                        //Game.market.sentToMarket.subtract(realSold);
                    }
                    else if (Game.market.howMuchMoneyCanNotPay(cost).get() > 10f)
                        Debug.Log("Failed market - can't pay " + Game.market.howMuchMoneyCanNotPay(cost)
                            + " for " + realSold); // money in market ended... Only first lucky get money
                }
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
    public Value getSoldByGovernment(Product product)
    {
        return soldByGovernment[product];
    }
    public Value getCostOfAllSellsByGovernment()
    {
        var res = new Value(0f);
        foreach (var item in soldByGovernment)
        {
            res.add(Game.market.getCost(new Storage(item.Key, item.Value)));
        }
        return res;
    }
    /// <summary> Assuming product is abstract product</summary>
    public Value getProducedTotalIncludingSubstitutes(Product product)
    {
        var res = new Value(0f);
        foreach (var item in product.getSubstitutes())
            if (item.isTradable())
            {
                res.add(producedTotal[item]);
            }
        return new Storage(product, res);
    }
    public Procent getWorldProductionShare(Product product)
    {
        var worldProduction = Game.market.getProductionTotal(product, true);
        if (worldProduction.isZero())
            return Procent.ZeroProcent;
        else
            return Procent.makeProcent(getProducedTotal(product), worldProduction);
    }
}
