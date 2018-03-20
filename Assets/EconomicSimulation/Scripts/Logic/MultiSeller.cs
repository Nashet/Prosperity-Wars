using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
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
    public abstract class MultiSeller : Staff, IStatisticable, ICanSell
    {
        public readonly CountryStorageSet countryStorageSet = new CountryStorageSet();
        private readonly StorageSet sentToMarket = new StorageSet();

        private readonly Dictionary<Product, Storage> sellIfMoreLimits = new Dictionary<Product, Storage>();
        private readonly Dictionary<Product, Storage> buyIfLessLimits = new Dictionary<Product, Storage>();
        /// <summary> Including enterprises, government and everything    </summary>
        private readonly Dictionary<Product, Value> producedTotal = new Dictionary<Product, Value>();
        /// <summary> Shows actual sells, not sent to market   </summary>
        private readonly Dictionary<Product, Value> soldByGovernment = new Dictionary<Product, Value>();

        public MultiSeller(float money, Country place) : base(place)
        {
            foreach (var item in Product.getAll().Where(x => !x.isAbstract()))
                //if (item != Product.Gold)
                {
                    if (item == Product.Grain)
                    {
                        buyIfLessLimits.Add(item, new Storage(item, Options.CountryMaxStorage));
                        sellIfMoreLimits.Add(item, new Storage(item, Options.CountryMaxStorage));
                    }
                    else
                    {
                        buyIfLessLimits.Add(item, new Storage(item, ReadOnlyValue.Zero));
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
            sellIfMoreLimits[product].Set(value);
        }
        /// <summary>
        /// returns exception if failed
        /// </summary>    
        public void setBuyIfLessLimits(Product product, float value)
        {
            buyIfLessLimits[product].Set(value);
        }
        override public void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            sentToMarket.setZero();
            foreach (var item in producedTotal)
                item.Value.Set(ReadOnlyValue.Zero);
            foreach (var item in soldByGovernment)
                item.Value.Set(Value.Zero);
        }

        public Storage getSentToMarket(Product product)
        {
            return sentToMarket.GetFirstSubstituteStorage(product);
        }
        /// <summary> Assuming product is abstract product</summary>
        public Storage getSentToMarketIncludingSubstituts(Product product)
        {
            var res = new Value(0f);
            foreach (var item in product.getSubstitutes())
                if (item.isTradable())
                {
                    res.Add(sentToMarket.GetFirstSubstituteStorage(item));
                }
            return new Storage(product, res);
        }

        /// <summary>
        /// Do checks outside
        /// </summary>    
        public void sell(Storage what)
        {
            sentToMarket.Add(what);
            //countryStorageSet.subtract(what);
            countryStorageSet.subtractNoStatistic(what); // to avoid getting what in "howMuchUsed" statistics
            Game.market.sentToMarket.Add(what);
        }
        public void getMoneyForSoldProduct()
        {
            foreach (var sent in sentToMarket)
                if (sent.isNotZero())
                {
                    Value DSB = new Value(Game.market.getDemandSupplyBalance(sent.Product));
                    if (DSB.get() == Options.MarketInfiniteDSB)
                        DSB.SetZero();// real DSB is unknown
                    else
                    if (DSB.get() > Options.MarketEqualityDSB)
                        DSB.Set(Options.MarketEqualityDSB);
                    Storage realSold = new Storage(sent);
                    realSold.Multiply(DSB);
                    if (realSold.isNotZero())
                    {
                        MoneyView cost = Game.market.getCost(realSold);
                        //soldByGovernment.addMy(realSold.Product, realSold);
                        soldByGovernment[realSold.Product].Set(realSold);
                        //returning back unsold product
                        //if (sent.isBiggerThan(realSold))
                        //{
                        //    var unSold = sent.subtractOutside(realSold);
                        //    countryStorageSet.add(unSold);
                        //}


                        if (Game.market.CanPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
                        {
                            Game.market.Pay(this, cost);
                            //Game.market.sentToMarket.subtract(realSold);
                        }
                        else if (Game.market.HowMuchLacksMoneyIncludingDeposits(cost).Get() > 10m && Game.devMode)
                            Debug.Log("Failed market - can't pay " + Game.market.HowMuchLacksMoneyIncludingDeposits(cost)
                                + " for " + realSold); // money in market ended... Only first lucky get money
                    }
                }
        }
        internal void producedTotalAdd(Storage produced)
        {            
            producedTotal.addMy(produced.Product, produced);
        }
        public ReadOnlyValue getProducedTotal(Product product)
        {
            //if (producedTotal.ContainsKey(product))
                return producedTotal[product];
            //else
            //    return Value.Zero;
        }
        public ReadOnlyValue getSoldByGovernment(Product product)
        {
            if (soldByGovernment.ContainsKey(product))
                return soldByGovernment[product];
            else
                return Value.Zero;            
        }
        public MoneyView getCostOfAllSellsByGovernment()
        {
            var res = new Money(0m);
            foreach (var item in soldByGovernment)
            {
                res.Add(Game.market.getCost(new Storage(item.Key, item.Value)));
            }
            return res;
        }
        /// <summary> Assuming product is abstract product</summary>
        public ReadOnlyValue getProducedTotalIncludingSubstitutes(Product product)
        {
            var res = new Value(0f);
            foreach (var item in product.getSubstitutes())
                if (item.isTradable())
                {
                    res.Add(producedTotal[item]);
                }
            return new Storage(product, res);
        }
        /// <summary>
        /// new value
        /// </summary>        
        public Procent getWorldProductionShare(Product product)
        {
            var worldProduction = Game.market.getProductionTotal(product, true);
            if (worldProduction.isZero())
                return Procent.ZeroProcent.Copy();
            else
                return new Procent(getProducedTotal(product), worldProduction);
        }
    }
}