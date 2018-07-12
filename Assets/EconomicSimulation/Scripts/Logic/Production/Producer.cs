using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents anyone who can produce, store and sell product (1 product)
    /// also linked to Province
    /// </summary>
    public abstract class Producer : Consumer, IHasProvince, ISeller
    {
        /// <summary>How much was gained (before any payments). Not money!! Generally, gets value in PopUnit.produce and Factore.Produce </summary>
        private Storage gainGoodsThisTurn;

        /// <summary>How much product actually left for now. Stores food, except for Artisans</summary>
        public Storage storage;

        /// <summary>How much sent to market, Some other amount could be consumedTotal or stored for future </summary>
        //private Storage sentToMarket;
        //public Storage SentToMarket { get { return sentToMarket; } }
        private Dictionary<Market, Storage> sentToMarket = new Dictionary<Market, Storage>();


        private readonly Province province;
        /// <summary> /// Return in pieces  /// </summary>
        //public abstract float getLocalEffectiveDemand(Product product);

        /// <summary>
        /// Just adds statistics
        /// </summary>
        public abstract void produce();

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

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            if (gainGoodsThisTurn != null)
                gainGoodsThisTurn.SetZero();
            if (sentToMarket != null)
                sentToMarket.Clear();
        }




        /// <summary>
        /// Do checks outside. Currently sends only to 1 market
        /// </summary>
        public void SendToMarket(Storage what)
        {
            var market = Market.GetReachestMarket(what);
            if (market == null)
                market = Country.market;
            sentToMarket.Add(market, what);
            storage.subtract(what);
            market.ReceiveProducts(what);
            //if (Game.logMarket)
            //    Debug.Log(this + " sent to market " + what + " costing " + Country.market.getCost(what));
        }

        /// <summary> Do checks outside</summary>
        public void consumeFromItself(Storage what)
        {
            consumed.Add(what);
            storage.subtract(what);
        }


        protected void changeProductionType(Product product)
        {
            storage = new Storage(product);
            gainGoodsThisTurn = new Storage(product);
            //sentToMarket = new Storage(product);
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

        public IEnumerable<Market> AllTradeMarkets()
        {
            return sentToMarket.Keys;
        }

        public IEnumerable<KeyValuePair<Market, Storage>> AllSellDeals()
        {
            foreach (var item in sentToMarket)
            {
                yield return item;
            }
        }

        /// <summary>        
        /// Returns null 
        /// </summary>        
        public Storage HowMuchSentToMarket(Market market, Product product)
        {
            Storage has;
            sentToMarket.TryGetValue(market, out has);
            if (has != null && has.Product == product)
            {
                return sentToMarket[market];
            }
            return new Storage(product);// empty storage
        }

    }
}