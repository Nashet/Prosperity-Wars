using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents anyone who can produce, store and sell product (1 product)
    /// also linked to Province
    /// </summary>
    public abstract class Producer : Consumer, ICanSell, IHasGetProvince
    {
        /// <summary>How much was gained (before any payments). Not money!! Generally, gets value in PopUnit.produce and Factore.Produce </summary>
        private Storage gainGoodsThisTurn;

        /// <summary>How much product actually left for now. Stores food, except for Artisans</summary>    
        public Storage storage;

        /// <summary>How much sent to market, Some other amount could be consumedTotal or stored for future </summary>
        private Storage sentToMarket;

        private readonly Province province;
        /// <summary> /// Return in pieces  /// </summary>    
        //public abstract float getLocalEffectiveDemand(Product product);

        /// <summary>
        /// Just adds statistics
        /// </summary>
        abstract public void produce();


        protected Producer(Province province) : base(province.Country)
        {
            this.province = province;
        }
       
        public Province Province
        {
            get { return province; }
        }
        public void calcStatistics()
        {
            Country.producedTotalAdd(gainGoodsThisTurn);
        }
        override public void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            if (gainGoodsThisTurn != null)
                gainGoodsThisTurn.SetZero();
            if (sentToMarket != null)
                sentToMarket.SetZero();
        }
        //todo put it and duplicate in market?
        public void getMoneyForSoldProduct()
        {
            if (sentToMarket.get() > 0f)
            {
                Value DSB = new Value(Game.market.getDemandSupplyBalance(sentToMarket.Product));
                if (DSB.get() == Options.MarketInfiniteDSB)
                    DSB.SetZero(); // real DSB is unknown
                else
                if (DSB.get() > Options.MarketEqualityDSB)
                    DSB.Set(Options.MarketEqualityDSB);
                Storage realSold = new Storage(sentToMarket);
                realSold.Multiply(DSB);
                if (realSold.isNotZero())
                {
                    ReadOnlyValue cost = Game.market.getCost(realSold);

                    // adding unsold product
                    // assuming gainGoodsThisTurn & realSold have same product
                    if (storage.isExactlySameProduct(gainGoodsThisTurn))
                        storage.add(gainGoodsThisTurn);
                    else
                        storage = new Storage(gainGoodsThisTurn);
                    storage.Subtract(realSold.get());

                    if (Game.market.CanPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
                    {
                        Game.market.Pay(this, cost);
                        
                    }
                    else if (Game.market.HowMuchLacksMoneyCashOnly(cost).get() > 10f && Game.devMode)
                        Debug.Log("Failed market - can't pay " + Game.market.HowMuchLacksMoneyCashOnly(cost)
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
            Game.market.sentToMarket.Add(what);
        }
        /// <summary> Do checks outside</summary>
        public void consumeFromItself(Storage what)
        {
            getConsumed().Add(what);
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
        /// <summary>
        /// New value
        /// </summary>        
        public Storage getGainGoodsThisTurn()
        {
            return gainGoodsThisTurn.Copy();
        }
        public void addProduct(Storage howMuch)
        {
            gainGoodsThisTurn.add(howMuch);
        }
    }
}