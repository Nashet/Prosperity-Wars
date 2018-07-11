using System;
using System.Collections.Generic;
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

        private Date dateOfgetSupplyOnMarket = Date.Never.Copy();
        private readonly StorageSet supplyOnMarket = new StorageSet();

        private Date dateOfgetTotalProduction = Date.Never.Copy();
        private readonly StorageSet totalProduction = new StorageSet();

        private Date dateOfgetTotalConsumption = Date.Never.Copy();
        private readonly StorageSet totalConsumption = new StorageSet();

        private Date dateOfgetBought = Date.Never.Copy();
        private readonly StorageSet bought = new StorageSet();

        internal PricePool priceHistory;
        private StorageSet receivedGoods = new StorageSet();

        //private Dictionary<Producer, Storage> sentToMarket;

        public Market() : base(null)
        {
        }



        internal void initialize()
        {
            priceHistory = new PricePool();
        }

        /// <summary>
        /// new value
        /// </summary>
        /// <param name="need"></param>
        /// <returns></returns>
        internal MoneyView getCost(StorageSet need)
        {
            Money cost = new Money(0m);
            // float price;
            foreach (Storage stor in need)
            {
                //price = World.market.findPrice(stor.Product).get();
                cost.Add(getCost(stor));
            }
            return cost;
        }

        /// <summary>
        /// returns new Value
        /// </summary>
        internal MoneyView getCost(IEnumerable<Storage> need)
        {
            Money cost = new Money(0m);
            foreach (Storage stor in need)
                cost.Add(getCost(stor));
            return cost;
        }

        /// <summary>
        /// New value
        /// </summary>
        internal MoneyView getCost(Storage need)
        {
            if (need.Product == Product.Gold)
            {
                //var res = need.Copy().Multiply(Options.goldToCoinsConvert);
                //res.Multiply(Options.GovernmentTakesShareOfGoldOutput);
                //return res;
                return new MoneyView((decimal)need.get());
            }
            else
                return World.market.getCost(need.Product).Copy().Multiply((decimal)need.get());
        }
        /// <summary>
        /// new value
        /// </summary>
        internal MoneyView getCost(Product whom)
        {
            return new MoneyView((decimal)prices.getCheapestStorage(whom).get());
        }
        /// <summary>
        /// Just transfers it to StorageSet.convertToCheapestStorageProduct(Storage)
        /// </summary>
        internal Storage GetRandomCheapestSubstitute(Storage need)
        {
            return prices.ConvertToRandomCheapestStorageProduct(need);
        }

        //todo change it to 1 run by every products, not run for every product
        private Storage recalculateProductForConsumers(Product product, Func<Consumer, StorageSet> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.getAllExistingCountries())
            {
                foreach (Province province in country.AllProvinces())
                    foreach (Consumer consumer in province.getAllAgents())
                    {
                        Storage found = selector(consumer).GetFirstSubstituteStorage(product);
                        result.add(found);
                    }
                Storage countryStor = selector(country).GetFirstSubstituteStorage(product);
                result.add(countryStor);
            }
            return result;
        }

        private Storage recalculateProductForBuyers(Product product, Func<Consumer, StorageSet> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.getAllExistingCountries())
            {
                foreach (Province province in country.AllProvinces())
                    foreach (Consumer consumer in province.getAllBuyers())
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
        private Storage recalculateProductForSellers(Product product, Func<ICanSell, Storage> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.getAllExistingCountries())
            {
                foreach (Province province in country.AllProvinces())
                    foreach (ICanSell producer in province.getAllProducers())
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
            foreach (Country country in World.getAllExistingCountries())
            {
                foreach (Province province in country.AllProvinces())
                    foreach (Producer producer in province.getAllProducers())
                    {
                        var found = selector(producer);
                        if (found.isExactlySameProduct(product))
                            result.add(found);
                    }
            }
            return result;
        }

        internal Storage getBouthOnMarket(Product product, bool takeThisTurnData)
        {
            if (takeThisTurnData)
            {
                // recalculate only 1 product
                return recalculateProductForBuyers(product, x => x.getConsumedInMarket());
            }
            if (!dateOfgetBought.IsToday)
            {
                //recalculate all products
                foreach (Storage recalculatingProduct in prices)
                    if (recalculatingProduct.Product.isTradable())
                    {
                        var result = recalculateProductForConsumers(recalculatingProduct.Product, x => x.getConsumedInMarket());

                        bought.Set(new Storage(recalculatingProduct.Product, result));
                    }
                dateOfgetBought.set(Date.Today);
            }
            return bought.GetFirstSubstituteStorage(product);
        }

        internal Storage getTotalConsumption(Product product, bool takeThisTurnData)
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
        internal Storage getMarketSupply(Product product, bool takeThisTurnData)
        {
            if (takeThisTurnData)
            {
                return recalculateProductForSellers(product, x => x.getSentToMarket(product));
            }
            if (!dateOfgetSupplyOnMarket.IsToday)
            {
                //recalculate supply buffer
                foreach (Storage recalculatingProduct in prices)
                    if (recalculatingProduct.Product.isTradable())
                    {
                        var result = recalculateProductForSellers(recalculatingProduct.Product, x => x.getSentToMarket(recalculatingProduct.Product));
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
        internal Storage getProductionTotal(Product product, bool takeThisTurnData)
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

        //internal void ForceDSBRecalculation()
        //{
        //    //dateOfDSB--;//!!! Warning! This need to be uncommented to work properly
        //    getDemandSupplyBalance(null);
        //}
        /// <summary>
        /// per 1 unit
        /// </summary>
        public void SetDefaultPrice(Product pro, float inprice)
        {
            prices.Set(new Storage(pro, inprice));
        }

        internal bool isAvailable(Product product)
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


        private void Ssssel(Consumer buyer, MoneyView cost, Storage sale)
        {

            buyer.BuyFromMarket(sale);

        }





        /// <summary>
        /// Buying PrimitiveStorageSet, subsidizations allowed
        /// </summary>
        //internal void SellList(Consumer buyer, StorageSet buying, Country subsidizer)
        //{
        //    foreach (Storage item in buying)
        //        if (item.isNotZero())
        //            buy(buyer, item, subsidizer);
        //}

        /// <summary>
        /// Buying needs in circle, by Procent in time
        /// return true if buying is zero (bought all what it wanted)
        /// </summary>
        internal bool Sell(Producer buyer, StorageSet stillHaveToBuy, Procent buyInTime, List<Storage> ofWhat)
        {
            bool buyingIsFinished = true;
            foreach (Storage what in ofWhat)
            {
                Storage consumeOnThisIteration = new Storage(what.Product, what.get() * buyInTime.get());
                if (consumeOnThisIteration.isZero())
                    return true;

                // check if consumeOnThisIteration is not bigger than stillHaveToBuy
                if (!stillHaveToBuy.has(consumeOnThisIteration))
                    consumeOnThisIteration = stillHaveToBuy.getBiggestStorage(what.Product);
                var reallyBought = Sell(buyer, consumeOnThisIteration, null);

                stillHaveToBuy.Subtract(reallyBought);

                if (stillHaveToBuy.getBiggestStorage(what.Product).isNotZero())
                    buyingIsFinished = false;
            }
            return buyingIsFinished;
        }

        /// <summary>
        /// Date actual for how much produced on turn start, not how much left
        /// </summary>
        //internal bool HasProducedThatMuch(Storage need)
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
        //internal Storage HowMuchProduced(Product need)
        //{
        //    //return findStorage(need.Product);
        //    // here DSB is based not on last turn data, but on this turn.
        //    return new Storage(need, getMarketSupply(need, false));
        //}
        /// <summary>
        /// Based on DSB, assuming you have enough money
        /// </summary>
        internal bool HasAvailable(Storage need)
        {
            //Storage availible = findStorage(need.Product);
            //if (availible.get() >= need.get()) return true;
            //else return false;
            Storage availible = HowMuchAvailable(need);
            if (availible.get() >= need.get()) return true;
            else return false;
        }

        /// <summary>
        /// Based on DSB, shows how much you can get assuming you have enough money
        /// </summary>
        internal Storage HowMuchAvailable(Storage need)
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

        /// <summary>
        /// Result > 1 mean demand is higher, price should go up   Result fewer 1 mean supply is higher, price should go down
        /// based on last turn data
        ///</summary>
        internal float getDemandSupplyBalance(Product product, bool forceDSBRecalculation)
        {
            if (product == Product.Gold)
                return Options.MarketInfiniteDSB;
            //Debug.Log("I'm in DSBBalancer, dateOfDSB = " + dateOfDSB);
            float balance;

            if (!dateOfDSB.IsToday || forceDSBRecalculation)
            // recalculate DSBbuffer
            {
                //Debug.Log("Recalculation of DSB started");
                foreach (Storage nextProduct in prices)
                    if (nextProduct.Product.isTradable())
                    {
                        //getProductionTotal(product, false); // for pre-turn initialization
                        //getTotalConsumption(product, false);// for pre-turn initialization
                        float supply = getMarketSupply(nextProduct.Product, forceDSBRecalculation).get();
                        float demand = getBouthOnMarket(nextProduct.Product, forceDSBRecalculation).get();

                        //if (supply == 0 && demand == 0) // both zero
                        //    balance = Options.MarketInfiniteDSB;
                        //else
                        //{
                        if (supply == 0)
                            balance = Options.MarketInfiniteDSB; // supply zero
                        else
                        {
                            if (demand == 0) // demand zero
                                balance = Options.MarketZeroDSB; // otherwise - furniture bag
                            else
                                balance = demand / supply;
                        }
                        //}
                        DSBbuffer.Set(new Storage(nextProduct.Product, balance));
                    }
                dateOfDSB.set(Date.Today);
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
                    // first call of DSB
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
        }

        public override string ToString()
        {
            return "Global market";
        }
        /// <summary>
        /// Brings money for sold product
        /// </summary>
        public static void GiveMoneyForSoldProduct(ICanSell seller)
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

                        if (market.CanPay(cost)) //&& World.market.tmpMarketStorage.has(realSold))
                        {
                            market.Pay(seller, cost);
                        }
                        else
                        {
                            //if (Game.devMode)// && World.market.HowMuchLacksMoneyIncludingDeposits(cost).Get() > 10m)
                            Debug.Log("Failed market - lacks " + market.HowMuchLacksMoneyIncludingDeposits(cost)
                                    + " for " + realSold + " " + sentToMarket.Product + " " + seller + " trade: " + cost); // money in market ended... Only first lucky get money
                            market.PayAllAvailableMoney(seller);

                        }
                    }
                }
            }
        }
        public void ReceiveProducts(Storage what)
        {
            receivedGoods.Add(what);
        }

        public static Market GetReachestMarket(Storage need)
        {
            return World.AllMarkets().MaxBy(x => x.getCost(need.Product).Get());
        }
    }
}