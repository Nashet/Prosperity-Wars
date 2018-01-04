using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents anyone who can produce, store and sell product (1 product)
    /// also linked to Province
    /// </summary>
    public abstract class Producer : Consumer, ICanSell
    {
        /// <summary>How much was gained (before any payments). Not money!! Generally, gets value in PopUnit.produce and Factore.Produce </summary>
        private Storage gainGoodsThisTurn;

        /// <summary>How much product actually left for now. Stores food, except for Artisans</summary>    
        public Storage storage;

        /// <summary>How much sent to market, Some other amount could be consumedTotal or stored for future </summary>
        private Storage sentToMarket;

        /// <summary> /// Return in pieces  /// </summary>    
        //public abstract float getLocalEffectiveDemand(Product product);


        public abstract void payTaxes();

        /// <summary>
        /// Just adds statistics
        /// </summary>
        abstract public void produce();


        protected Producer(Province province) : base(province.getCountry().getBank(), province)
        {
        }
        //protected Producer() : base(null, null)
        //{
        //}
        public void calcStatistics()
        {
            getCountry().producedTotalAdd(gainGoodsThisTurn);
        }
        override public void setStatisticToZero()
        {
            base.setStatisticToZero();
            if (gainGoodsThisTurn != null)
                gainGoodsThisTurn.setZero();
            if (sentToMarket != null)
                sentToMarket.setZero();
        }
        //public Value getProducing()
        //{
        //    return gainGoodsThisTurn;
        //}
        public void getMoneyForSoldProduct()
        {
            if (sentToMarket.get() > 0f)
            {
                Value DSB = new Value(Game.market.getDemandSupplyBalance(sentToMarket.getProduct()));
                if (DSB.get() == Options.MarketInfiniteDSB)
                    DSB.setZero(); // real DSB is unknown
                else
                if (DSB.get() > Options.MarketEqualityDSB)
                    DSB.set(Options.MarketEqualityDSB);
                Storage realSold = new Storage(sentToMarket);
                realSold.multiply(DSB);
                if (realSold.isNotZero())
                {
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
                    else if (Game.market.howMuchMoneyCanNotPay(cost).get() > 10f && Game.devMode)
                        Debug.Log("Failed market - can't pay " + Game.market.howMuchMoneyCanNotPay(cost)
                                + " for " + realSold); // money in market ended... Only first lucky get money
                }
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

        public Storage getSentToMarket(Product product)
        {
            return sentToMarket;
        }
        public Storage getSentToMarket()
        {
            return sentToMarket;
        }
        protected void changeProductionType(Product product)
        {
            storage = new Storage(product);
            gainGoodsThisTurn = new Storage(product);
            sentToMarket = new Storage(product);
        }
        public Storage getGainGoodsThisTurn()
        {
            return gainGoodsThisTurn;
        }
        public void addProduct(Storage howMuch)
        {
            gainGoodsThisTurn.add(howMuch);
        }
    }
}