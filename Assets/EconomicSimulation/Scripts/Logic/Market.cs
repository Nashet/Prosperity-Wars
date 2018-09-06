using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represent markets. Each country can have one
    /// </summary>
    public class Market : Agent//: PrimitiveStorageSet
    {
        public readonly StorageSet prices = new StorageSet();

        // todo make Better class for it? - yes
        private Date dateOfDSB = Date.Never.Copy();

        private readonly StorageSet DSBbuffer = new StorageSet();
        Dictionary<Product, Value> marketSupply = new Dictionary<Product, Value>();
        Dictionary<Product, Value> boughtOnMarket = new Dictionary<Product, Value>();

        private Date dateOfgetSupplyOnMarket = Date.Never.Copy();
        private readonly StorageSet supplyOnMarket = new StorageSet();

        private Date dateOfgetTotalProduction = Date.Never.Copy();
        private readonly StorageSet totalProduction = new StorageSet();

        private Date dateOfgetTotalConsumption = Date.Never.Copy();
        private readonly StorageSet totalConsumption = new StorageSet();

        private Date dateOfgetBought = Date.Never.Copy();
        private readonly StorageSet bought = new StorageSet();

        public PricePool priceHistory;
        private StorageSet receivedGoods = new StorageSet();

        public static Market TemporalSingleMarket { get; internal set; }

        //private Dictionary<Producer, Storage> sentToMarket;

        public Market() : base(null)
        {
            TemporalSingleMarket = this;
        }



        public void Initialize(Country country)
        {
            priceHistory = new PricePool();
            foreach (var item in Product.AllNonAbstract())
                if (item != Product.Gold)
                {
                    prices.Set(new Storage(item, (float)item.defaultPrice.Get()));
                }
            Country = country;
        }

        /// <summary>
        /// new value
        /// </summary>        
        public MoneyView getCost(StorageSet need)
        {
            Money cost = new Money(0m);
            // float price;
            foreach (Storage stor in need)
            {
                //price = Country.market.findPrice(stor.Product).get();
                cost.Add(getCost(stor));
            }
            return cost;
        }

        /// <summary>
        /// returns new Value
        /// </summary>
        public MoneyView getCost(IEnumerable<Storage> need)
        {
            Money cost = new Money(0m);
            foreach (Storage stor in need)
                cost.Add(getCost(stor));
            return cost;
        }

        /// <summary>
        /// New value
        /// </summary>
        public MoneyView getCost(Storage need)
        {
            if (need.Product == Product.Gold)
            {
                //var res = need.Copy().Multiply(Options.goldToCoinsConvert);
                //res.Multiply(Options.GovernmentTakesShareOfGoldOutput);
                //return res;
                return new MoneyView((decimal)need.get());
            }
            else
                return getCost(need.Product).Copy().Multiply((decimal)need.get());
        }
        /// <summary>
        /// new value. Cost in that particular market. Cheapest if there are several products
        /// </summary>
        public MoneyView getCost(Product product)
        {
            if (product == Product.Gold)
            {
                //var res = need.Copy().Multiply(Options.goldToCoinsConvert);
                //res.Multiply(Options.GovernmentTakesShareOfGoldOutput);
                //return res;
                return new MoneyView(1);// cost of 1 gold
            }
            else
                return new MoneyView((decimal)prices.getCheapestStorage(product, this).get());
        }

        /// <summary>
        /// Just transfers it to StorageSet.convertToCheapestStorageProduct(Storage)
        /// </summary>
        public Storage GetRandomCheapestSubstitute(Storage need)
        {
            return prices.ConvertToRandomCheapestStorageProduct(need, this);
        }

        //todo change it to 1 run by every products, not run for every product
        private Storage recalculateProductForConsumers(Product product, Func<Consumer, IEnumerable<Storage>> selector)
        {
            Storage result = new Storage(product); // too big circle - 22k per frame, 11Mb memory
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (Province province in country.AllProvinces)
                    foreach (Consumer consumer in province.AllAgents)
                    {
                        Storage found = selector(consumer).GetFirstSubstituteStorage(product);
                        result.add(found);
                    }
                Storage countryStor = selector(country).GetFirstSubstituteStorage(product);
                result.add(countryStor);
            }
            return result;
        }

        private Storage recalculateProductForBuyers(Product product, Func<Consumer, IEnumerable<Storage>> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (Province province in country.AllProvinces)
                    foreach (Consumer consumer in province.AllConsumers)
                    {
                        Storage re = selector(consumer).GetFirstSubstituteStorage(product);
                        result.add(re);
                    }
                Storage countryStor = selector(country).GetFirstSubstituteStorage(product);
                result.add(countryStor);
            }
            return result;
        }

        //todo change it to 1 run by every products, not run for every product
        private Storage recalculateProductForSellers(Product product, Func<ISeller, Storage> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (Province province in country.AllProvinces)
                    foreach (ISeller producer in province.AllProducers)
                    {
                        var found = selector(producer);
                        if (found.isExactlySameProduct(product))
                            result.add(found);
                    }
                result.add(selector(country));
            }
            return result;
        }

        //todo change it to 1 run by every products, not run for every product
        private Storage recalculateProductForProducers(Product product, Func<Producer, Storage> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (Province province in country.AllProvinces)
                    foreach (Producer producer in province.AllProducers)
                    {
                        var found = selector(producer);
                        if (found.isExactlySameProduct(product))
                            result.add(found);
                    }
            }
            return result;
        }

        public Storage getBouthOnMarket(Product product, bool takeThisTurnData)
        {
            if (takeThisTurnData)
            {
                // recalculate only 1 product
                return recalculateProductForBuyers(product, x => x.AllConsumedInMarket(this));
            }
            if (!dateOfgetBought.IsToday)
            {
                //recalculate all products
                foreach (Storage recalculatingProduct in prices)
                    if (recalculatingProduct.Product.isTradable())
                    {
                        var result = recalculateProductForConsumers(recalculatingProduct.Product, x => x.AllConsumedInMarket(this));

                        bought.Set(new Storage(recalculatingProduct.Product, result));
                    }
                dateOfgetBought.set(Date.Today);
            }
            return bought.GetFirstSubstituteStorage(product);
        }

        public Storage getTotalConsumption(Product product, bool takeThisTurnData)
        {
            if (takeThisTurnData)
            {
                return recalculateProductForConsumers(product, x => x.getConsumed());
            }
            if (!dateOfgetTotalConsumption.IsToday)
            {
                //recalculate buffer
                foreach (Storage recalculatingProduct in prices)
                    if (recalculatingProduct.Product.isTradable())
                    {
                        var result = recalculateProductForConsumers(recalculatingProduct.Product, x => x.getConsumed());
                        totalConsumption.Set(new Storage(recalculatingProduct.Product, result));
                    }
                dateOfgetTotalConsumption.set(Date.Today);
            }
            return totalConsumption.GetFirstSubstituteStorage(product);
        }

        /// <summary>
        /// Only goods sent to market
        /// Based  on last turn data
        /// </summary>
        public Storage getMarketSupply(Product product, bool takeThisTurnData)
        {
            if (takeThisTurnData)
            {
                return recalculateProductForSellers(product, x => x.HowMuchSentToMarket(this, product));
            }
            if (!dateOfgetSupplyOnMarket.IsToday)
            {
                //recalculate supply buffer
                foreach (Storage recalculatingProduct in prices)
                    if (recalculatingProduct.Product.isTradable())
                    {
                        var result = recalculateProductForSellers(recalculatingProduct.Product, x => x.HowMuchSentToMarket(this, recalculatingProduct.Product));
                        supplyOnMarket.Set(new Storage(recalculatingProduct.Product, result));
                    }
                dateOfgetSupplyOnMarket.set(Date.Today);
            }
            return supplyOnMarket.GetFirstSubstituteStorage(product);
        }

        /// <summary>
        /// All produced supplies
        /// Based  on last turn data
        /// </summary>
        public Storage getProductionTotal(Product product, bool takeThisTurnData)
        {
            if (takeThisTurnData)
            {
                return recalculateProductForProducers(product, x => x.getGainGoodsThisTurn());
            }
            if (!dateOfgetTotalProduction.IsToday)
            {
                //recalculate Production buffer
                foreach (Storage recalculatingProduct in prices)
                    if (recalculatingProduct.Product.isTradable())
                    {
                        var result = recalculateProductForProducers(recalculatingProduct.Product, x => x.getGainGoodsThisTurn());
                        totalProduction.Set(new Storage(recalculatingProduct.Product, result));
                    }
                dateOfgetTotalProduction.set(Date.Today);
            }

            return totalProduction.GetFirstSubstituteStorage(product);
        }

        //public void ForceDSBRecalculation()
        //{
        //    //dateOfDSB--;//!!! Warning! This need to be uncommented to work properly
        //    getDemandSupplyBalance(null);
        //}


        public bool isAvailable(Product product)
        {
            if (product.isAbstract())
            {
                foreach (var substitute in product.getSubstitutes())
                    if (substitute.isTradable()) //it would be faster to. skip it Or not
                    {
                        var DSB = getDemandSupplyBalance(substitute, false);
                        if (DSB != Options.MarketInfiniteDSB && DSB < Options.MarketEqualityDSB)
                            return true;
                    }
                return false;
            }
            else
            {
                var DSB = getDemandSupplyBalance(product, false);
                if (DSB != Options.MarketInfiniteDSB && DSB < Options.MarketEqualityDSB)
                    return true;
                else
                    return false;
            }
        }


        /// <summary>
        /// Buying PrimitiveStorageSet, subsidizations allowed
        /// </summary>
        //public void SellList(Consumer buyer, StorageSet buying, Country subsidizer)
        //{
        //    foreach (Storage item in buying)
        //        if (item.isNotZero())
        //            buy(buyer, item, subsidizer);
        //}



        /// <summary>
        /// Date actual for how much produced on turn start, not how much left
        /// </summary>
        //public bool HasProducedThatMuch(Storage need)
        //{
        //    //Storage availible = findStorage(need.Product);
        //    //if (availible.get() >= need.get()) return true;
        //    //else return false;
        //    Storage availible = HowMuchProduced(need.Product);
        //    if (availible.get() >= need.get()) return true;
        //    else return false;
        //}
        /// <summary>
        /// Must be safe - returns new Storage
        /// Date actual for how much produced on turn start, not how much left
        /// </summary>
        //public Storage HowMuchProduced(Product need)
        //{
        //    //return findStorage(need.Product);
        //    // here DSB is based not on last turn data, but on this turn.
        //    return new Storage(need, getMarketSupply(need, false));
        //}

        /// <summary>
        /// Based on DSB, assuming you have enough money
        /// </summary>
        public bool HasAvailable(Storage need)
        {
            //Storage availible = findStorage(need.Product);
            //if (availible.get() >= need.get()) return true;
            //else return false;
            Storage availible = HowMuchAvailable(need);
            if (availible.get() >= need.get())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Based on DSB, shows how much you can get assuming you have enough money
        /// </summary>
        public Storage HowMuchAvailable(Storage need)
        {
            //float BuyingAmountAvailable = 0;
            return receivedGoods.getBiggestStorage(need.Product);

            //BuyingAmountAvailable = need.get() / DSB;

            //float DSB = getDemandSupplyBalance(need.Product);
            //float BuyingAmountAvailable = 0;

            //if (DSB < 1f) DSB = 1f;
            //BuyingAmountAvailable = need.get() / DSB;

            //return new Storage(need.Product, BuyingAmountAvailable);
        }

        public void ForceDSBRecalculation2()
        {
            // get all MarketSupply            
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (var agent in country.Provinces.AllAgents)
                {
                    //if (found.isExactlySameProduct(product))
                    var isSeller = agent as Producer;
                    if (isSeller != null)
                        foreach (var deal in isSeller.AllSellDeals())
                        {
                            if (deal.Key == this)// && deal.Value.Product.isTradable())
                                marketSupply.AddAndSum(deal.Value.Product, deal.Value);
                        }
                    var isConsumer = agent as Consumer;
                    if (isConsumer!=null)
                        foreach (var deal in isConsumer.AllConsumedInMarket(this))
                        {
                            //if (deal.Product.isTradable())
                            boughtOnMarket.AddAndSum(deal.Product, deal);
                        }
                }
            }

            // get all getBoughtOnMarket            
            //foreach (Country country in World.getAllExistingCountries())
            //{
            //    foreach (var consumer in country.AllConsumers())
            //    {
            //        //if (found.isExactlySameProduct(product))
            //        foreach (var deal in consumer.AllConsumedInMarket(this))
            //        {
            //            //if (deal.Product.isTradable())
            //            boughtOnMarket.AddAndSum(deal.Product, deal);
            //        }
            //    }
            //}

            //calculate DSB

            foreach (var product in Product.AllNonAbstract())
            {
                float balance, demand = 0f, supply = 0f;

                Value demandValue;
                if (boughtOnMarket.TryGetValue(product, out demandValue))
                    demand = demandValue.get();

                Value supplyValue;
                if (marketSupply.TryGetValue(product, out supplyValue))
                    supply = supplyValue.get();


                if (supply == 0)
                    balance = Options.MarketInfiniteDSB; // supply zero
                else
                {
                    if (demand == 0f) // demand zero
                        balance = Options.MarketZeroDSB; // otherwise - furniture bag
                    else
                        balance = demand / supply;
                }


                if (supply != 0f && demand == 0f)
                    balance = Options.MarketZeroDSB; // Options.MarketInfiniteDSB; // supply zero
                else if (supply == 0f && demand == 0f)
                    balance = Options.MarketInfiniteDSB; // Options.MarketInfiniteDSB; // supply zero
                else
                {
                    if (demand == 0f) // demand zero
                        balance = Options.MarketZeroDSB; // otherwise - furniture bag
                    else
                        balance = demand / supply;
                }
                DSBbuffer.Set(new Storage(product, balance));

            }
            dateOfDSB.set(Date.Today);
        }
        public void ForceDSBRecalculation()
        {
            // get all MarketSupply            
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (var seller in country.Provinces.AllSellers)
                {
                    //if (found.isExactlySameProduct(product))
                    foreach (var deal in seller.AllSellDeals())
                    {
                        if (deal.Key == this)// && deal.Value.Product.isTradable())
                            marketSupply.AddAndSum(deal.Value.Product, deal.Value);
                    }
                }
            }

            // get all getBoughtOnMarket            
            foreach (Country country in World.AllExistingCountries())
            {
                foreach (var consumer in country.Provinces.AllConsumers)
                {
                    //if (found.isExactlySameProduct(product))
                    foreach (var deal in consumer.AllConsumedInMarket(this))
                    {
                        //if (deal.Product.isTradable())
                        boughtOnMarket.AddAndSum(deal.Product, deal);
                    }
                }
            }

            //calculate DSB

            foreach (var product in Product.AllNonAbstract())
            {
                float balance, demand = 0f, supply = 0f;

                Value demandValue;
                if (boughtOnMarket.TryGetValue(product, out demandValue))
                    demand = demandValue.get();

                Value supplyValue;
                if (marketSupply.TryGetValue(product, out supplyValue))
                    supply = supplyValue.get();


                if (supply == 0)
                    balance = Options.MarketInfiniteDSB; // supply zero
                else
                {
                    if (demand == 0f) // demand zero
                        balance = Options.MarketZeroDSB; // otherwise - furniture bag
                    else
                        balance = demand / supply;
                }


                if (supply != 0f && demand == 0f)
                    balance = Options.MarketZeroDSB; // Options.MarketInfiniteDSB; // supply zero
                else if (supply == 0f && demand == 0f)
                    balance = Options.MarketInfiniteDSB; // Options.MarketInfiniteDSB; // supply zero
                else
                {
                    if (demand == 0f) // demand zero
                        balance = Options.MarketZeroDSB; // otherwise - furniture bag
                    else
                        balance = demand / supply;
                }
                DSBbuffer.Set(new Storage(product, balance));

            }
            dateOfDSB.set(Date.Today);
        }

        /// <summary>
        /// Result > 1 mean demand is higher, price should go up   Result fewer 1 mean supply is higher, price should go down
        /// based on last turn data
        ///</summary>
        public float getDemandSupplyBalance(Product product, bool forceDSBRecalculation)
        {
            if (product == Product.Gold)
                return Options.MarketInfiniteDSB;
            //Debug.Log("I'm in DSBBalancer, dateOfDSB = " + dateOfDSB);            

            if (!dateOfDSB.IsToday || forceDSBRecalculation)
            // recalculate DSBbuffer
            {
                ForceDSBRecalculation();
                //Debug.Log("Recalculation of DSB started");
                //foreach (Storage nextProduct in prices)
                //    if (nextProduct.Product.isTradable())
                //    {
                //        float balance;
                //        //getProductionTotal(product, false); // for pre-turn initialization
                //        //getTotalConsumption(product, false);// for pre-turn initialization
                //        float supply = getMarketSupply(nextProduct.Product, forceDSBRecalculation).get();
                //        float demand = getBouthOnMarket(nextProduct.Product, forceDSBRecalculation).get();

                //        if (supply == 0)
                //            balance = Options.MarketInfiniteDSB; // supply zero
                //        else
                //        {
                //            if (demand == 0f) // demand zero
                //                balance = Options.MarketZeroDSB; // otherwise - furniture bag
                //            else
                //                balance = demand / supply;
                //        }

                //        //if (supply == 0 && demand == 0) // both zero
                //        //    balance = Options.MarketInfiniteDSB;
                //        //else
                //        //{
                //        if (supply != 0f && demand == 0f)
                //            balance = Options.MarketZeroDSB; // Options.MarketInfiniteDSB; // supply zero
                //        else if (supply == 0f && demand == 0f)
                //            balance = Options.MarketInfiniteDSB; // Options.MarketInfiniteDSB; // supply zero
                //        else
                //        {
                //            if (demand == 0f) // demand zero
                //                balance = Options.MarketZeroDSB; // otherwise - furniture bag
                //            else
                //                balance = demand / supply;
                //        }
                //        //}
                //        DSBbuffer.Set(new Storage(nextProduct.Product, balance));
                //    }
                //dateOfDSB.set(Date.Today);
            }
            if (product == null)
                return 0f;
            else
                return DSBbuffer.GetFirstSubstituteStorage(product).get();
        }

        /// <summary>
        /// Changes price for every product in market
        /// That's first call for DSB in tick
        /// </summary>
        public void simulatePriceChangeBasingOnLastTurnData()
        {
            float balance;
            float priceChangeSpeed;
            //float highestChangingSpeed = 0.2f; //%
            //float highChangingSpeed = 0.04f;//%

            foreach (Storage price in prices)
                if (price.Product.isTradable())
                {
                    // first call of DSB, based on last turn data
                    balance = getDemandSupplyBalance(price.Product, false);
                    /// Result > 1 mean demand is higher, price should go up
                    /// Result fewer 1 mean supply is higher, price should go down

                    priceChangeSpeed = 0;
                    if (balance >= 0.95f)//1f) //some goods can't achieve 1 dsb, like cotton for example
                        priceChangeSpeed = 0.001f + price.get() * 0.1f;
                    else
                    {
                        if (balance <= 0.8f)
                            //priceChangeSpeed = -0.001f + price.get() * -0.02f;
                            priceChangeSpeed = -0.001f + price.get() * -0.1f;// - 0.1f; caused dsb 1-0 fluctuation, destroying industry
                    }
                    ChangePrice(price, priceChangeSpeed);
                }
        }

        private void ChangePrice(Storage price, float HowMuch)
        {
            float newValue = HowMuch + price.get();
            if (newValue <= 0f)
                newValue = (float)Options.minPrice.Get();
            if (newValue >= (float)Options.maxPrice.Get())
            {
                newValue = (float)Options.maxPrice.Get();
                //if (getBouth(price.Product) != 0) newValue = Game.maxPrice / 20f;
            }
            price.Set(newValue);
            priceHistory.addData(price.Product, price);
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            receivedGoods.setZero();

            foreach (var item in marketSupply)
            {
                item.Value.SetZero();
            }

            foreach (var item in boughtOnMarket)
            {
                item.Value.SetZero();
            }
        }

        public override string ToString()
        {
            return "Single market";//Country + "'s market";
        }

        public static Storage GiveTotalSoldProduct(ISeller seller, Product product)
        {
            var res = new Storage(product);
            foreach (var deal in seller.AllSellDeals().Where(x => x.Value.Product == product))
            {
                // Key is a market, Value is a Storage
                var market = deal.Key;
                var sentToMarket = deal.Value;
                if (sentToMarket.get() > 0f)
                {
                    Value DSB = new Value(market.getDemandSupplyBalance(sentToMarket.Product, false));
                    if (DSB.get() == Options.MarketInfiniteDSB)
                        DSB.SetZero(); // real DSB is unknown
                    else if (DSB.get() > Options.MarketEqualityDSB)
                        DSB.Set(Options.MarketEqualityDSB);

                    var realSold = sentToMarket.Multiply(DSB);

                    if (realSold.isNotZero())
                    {
                        res.Add(realSold);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Brings money for sold product
        /// </summary>
        public static void GiveMoneyForSoldProduct(ISeller seller)
        {
            foreach (var deal in seller.AllSellDeals())
            {
                // Key is a market, Value is a Storage
                var market = deal.Key;
                var sentToMarket = deal.Value;
                if (sentToMarket.get() > 0f)
                {
                    Value DSB = new Value(market.getDemandSupplyBalance(sentToMarket.Product, false));
                    if (DSB.get() == Options.MarketInfiniteDSB)
                        DSB.SetZero(); // real DSB is unknown
                    else if (DSB.get() > Options.MarketEqualityDSB)
                        DSB.Set(Options.MarketEqualityDSB);

                    decimal realSold = (decimal)sentToMarket.get();
                    realSold *= (decimal)DSB.get();

                    if (realSold > 0m)
                    {
                        MoneyView cost = market.getCost(sentToMarket.Product).Copy().Multiply(realSold);

                        // adding unsold product
                        // assuming gainGoodsThisTurn & realSold have same product
                        //if (storage.isExactlySameProduct(gainGoodsThisTurn))
                        //    storage.add(gainGoodsThisTurn);
                        //else
                        //    storage = new Storage(gainGoodsThisTurn);
                        //storage.Subtract((float)realSold);


                        if (market.CanPay(cost)) //&& Country.market.tmpMarketStorage.has(realSold))
                        {
                            market.Pay(seller as Agent, cost, Register.Account.MarketOperations);
                        }
                        else
                        {
                            if (Game.devMode)// && Country.market.HowMuchLacksMoneyIncludingDeposits(cost).Get() > 10m)
                                Debug.Log("Failed market - lacks " + market.HowMuchLacksMoneyIncludingDeposits(cost)
                                        + " for " + realSold + " " + sentToMarket.Product + " " + seller + " trade: " + cost); // money in market ended... Only first lucky get money
                            market.PayAllAvailableMoney(seller as Agent, Register.Account.MarketOperations);

                        }
                    }
                }
            }
        }
        public void ReceiveProducts(Storage what)
        {
            receivedGoods.Add(what);
        }
        public void SendGoods(Storage what)
        {
            receivedGoods.Subtract(what);
        }

        public static Market GetReachestMarket(Storage need)
        {
            return World.AllMarkets.MaxBy(x => x.getCost(need.Product).Get());
            //.Where(x => x.getDemandSupplyBalance(need.Product, false) != Options.MarketEqualityDSB)
        }
        public static Market GetCheapestMarket(Storage need)
        {
            return World.AllMarkets.MinBy(x => x.getCost(need.Product).Get());
        }
    }
}