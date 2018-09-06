using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{


    /// <summary>
    /// Had to be class representing ability to sell more than 1 product
    /// but actually it contains statistics for Country
    /// </summary>
    public abstract class MultiSeller : Staff, IStatisticable, ISeller
    {
        public readonly CountryStorageSet countryStorageSet = new CountryStorageSet();
        private List<KeyValuePair<Market, Storage>> sentToMarket = new List<KeyValuePair<Market, Storage>>();

        private readonly Dictionary<Product, Storage> sellIfMoreLimits = new Dictionary<Product, Storage>();
        private readonly Dictionary<Product, Storage> buyIfLessLimits = new Dictionary<Product, Storage>();

        /// <summary> Including enterprises, government and everything    </summary>
        private readonly Dictionary<Product, Value> producedTotal = new Dictionary<Product, Value>();

        /// <summary> Shows actual sells, not sent to market   </summary>
        private readonly Dictionary<Product, Value> soldByGovernment = new Dictionary<Product, Value>();

        public MultiSeller(float money, Country place) : base(place)
        {
            //Country = place;
            foreach (var item in Product.AllNonAbstract())
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

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            sentToMarket = new List<KeyValuePair<Market, Storage>>();// .Clear;
            foreach (var item in producedTotal)
                item.Value.Set(ReadOnlyValue.Zero);
            foreach (var item in soldByGovernment)
                item.Value.Set(Value.Zero);
        }

        //public Storage getSentToMarket(Product product)
        //{
        //    return sentToMarket.GetFirstSubstituteStorage(product);
        //}

        /// <summary> Assuming product is abstract product</summary>
        //public Storage getSentToMarketIncludingSubstituts(Product product)
        //{
        //    var res = new Value(0f);
        //    foreach (var item in product.getSubstitutes())
        //        if (item.isTradable())
        //        {
        //            res.Add(sentToMarket.GetFirstSubstituteStorage(item));
        //        }
        //    return new Storage(product, res);
        //}

        /// <summary>
        /// Do checks outside
        /// </summary>
        public void SendToMarket(Storage what)
        {
            var market = Market.GetReachestMarket(what);
            if (market == null)
                market = Country.market;
            sentToMarket.Add(new KeyValuePair<Market, Storage>(market, what));
            //countryStorageSet.subtract(what);
            countryStorageSet.subtractNoStatistic(what); // to avoid getting what in "howMuchUsed" statistics

            market.ReceiveProducts(what);
        }



        public void producedTotalAdd(Storage produced)
        {
            producedTotal.AddAndSum(produced.Product, produced);
        }

        public ReadOnlyValue getProducedTotal(Product product)
        {
            //if (producedTotal.ContainsKey(product))
            return producedTotal[product];
            //else
            //    return Value.Zero;
        }

        /// <summary>
        /// not used really
        /// </summary>        
        public ReadOnlyValue getSoldByGovernment(Product product)
        {
            if (soldByGovernment.ContainsKey(product))
                return soldByGovernment[product];
            else
                return ReadOnlyValue.Zero;
        }
        /// <summary>
        /// !!
        /// </summary>        
        public MoneyView getCostOfAllSellsByGovernment()
        {
            var res = new Money(0m);
            foreach (var item in soldByGovernment)
            {
                res.Add(Game.Player.market.getCost(new Storage(item.Key, item.Value)));
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
            Storage worldProduction = new Storage(product);
            foreach (var item in World.AllMarkets)
            {
                worldProduction.Add(item.getProductionTotal(product, true));
            }

            if (worldProduction.isZero())
                return Procent.ZeroProcent.Copy();
            else
                return new Procent(getProducedTotal(product), worldProduction);
        }

        public IEnumerable<Market> AllTradeMarkets()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<KeyValuePair<Market, Storage>> AllSellDeals()
        {
            foreach (var item in sentToMarket)
            {
                yield return item;
            }
        }

        public Storage HowMuchSentToMarket(Market market, Product product)
        {
            var pair = sentToMarket.Where(x => x.Key == market && x.Value.Product == product).FirstOrDefault();

            if (pair.Value != null)
            {
                return pair.Value;
            }
            else
                return new Storage(product);// empty storage
        }
    }
}