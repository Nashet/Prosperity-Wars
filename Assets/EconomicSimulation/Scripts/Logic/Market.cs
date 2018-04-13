using System;
using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represent World market, currently exists only in 1 instance (World.market)
    /// </summary>
    public class Market : Agent//: PrimitiveStorageSet
    {
        private readonly StorageSet marketPrice = new StorageSet();

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
        internal StorageSet sentToMarket = new StorageSet();

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
        internal MoneyView getCost(List<Storage> need)
        {
            Money cost = new Money(0m);
            foreach (Storage stor in need)
                cost.Add(getCost(stor));
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
            return new MoneyView((decimal)marketPrice.getCheapestStorage(whom).get());
        }
        /// <summary>
        /// Just transfers it to StorageSet.convertToCheapestStorageProduct(Storage)
        /// </summary>
        internal Storage GetRandomCheapestSubstitute(Storage need)
        {
            return marketPrice.ConvertToRandomCheapestStorageProduct(need);
        }

        //todo change it to 1 run by every products, not run for every product
        private Storage recalculateProductForConsumers(Product product, Func<Consumer, StorageSet> selector)
        {
            Storage result = new Storage(product);
            foreach (Country country in World.getAllExistingCountries())
            {
                foreach (Province province in country.getAllProvinces())
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
                foreach (Province province in country.getAllProvinces())
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
                foreach (Province province in country.getAllProvinces())
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
                foreach (Province province in country.getAllProvinces())
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
                foreach (Storage recalculatingProduct in marketPrice)
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
                foreach (Storage recalculatingProduct in marketPrice)
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
                foreach (Storage recalculatingProduct in marketPrice)
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
                foreach (Storage recalculatingProduct in marketPrice)
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
            marketPrice.Set(new Storage(pro, inprice));
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
            buyer.Pay(World.market, cost);
            buyer.consumeFromMarket(sale);
            var isSP = buyer as SimpleProduction;
            if (isSP != null)
                isSP.getInputProductsReserve().Add(sale);
        }

        /// <summary>
        /// returns how much was sold de facto
        /// new version of buy-old,
        /// real deal. If not enough money to buy (including deposits) then buys some part of desired
        /// </summary>
        internal Storage Sell(Consumer buyer, Storage whatWantedToBuy)
        {
            if (whatWantedToBuy.isNotZero())
            {

                Storage sale;
                if (whatWantedToBuy.Product.isAbstract())
                {
                    sale = marketPrice.ConvertToRandomCheapestExistingSubstitute(whatWantedToBuy);
                    if (sale == null)//no substitution available on market
                        return new Storage(whatWantedToBuy.Product);
                    else if (sale.isZero())
                        return sale;
                }
                else
                    sale = whatWantedToBuy;

                Storage howMuchCanConsume;
                MoneyView price = getCost(sale.Product);
                MoneyView cost;

                if (World.market.sentToMarket.has(sale))
                {
                    cost = getCost(sale);

                    if (buyer.CanPay(cost))
                    {
                        Ssssel(buyer, cost, sale);
                        return sale;
                    }
                    else
                    {
                        float val = (float)(buyer.getMoneyAvailable().Get() / price.Get());
                        howMuchCanConsume = new Storage(sale.Product, val);
                        howMuchCanConsume.Subtract(0.001f, false); // to fix percision bug
                        if (howMuchCanConsume.isZero())
                            return howMuchCanConsume;
                        else
                        {
                            
                            Ssssel(buyer, getCost(howMuchCanConsume), howMuchCanConsume);
                            return howMuchCanConsume;
                        }
                    }
                }
                else
                {
                    // assuming available < buying
                    Storage howMuchAvailable = new Storage(World.market.HowMuchAvailable(sale));
                    if (howMuchAvailable.isNotZero())
                    {
                        cost = getCost(howMuchAvailable);
                        if (buyer.CanPay(cost))
                        {
                            Ssssel(buyer, cost, howMuchAvailable);
                            return howMuchAvailable;
                        }
                        else
                        {
                            howMuchCanConsume = new Storage(howMuchAvailable.Product, (float)(buyer.getMoneyAvailable().Get() / price.Get()));

                            if (howMuchCanConsume.get() > howMuchAvailable.get())
                                howMuchCanConsume.Set(howMuchAvailable.get()); // you don't buy more than there is

                            howMuchCanConsume.Subtract(0.001f, false); // to fix percision bug
                            if (howMuchCanConsume.isNotZero())
                            {
                                Ssssel(buyer, buyer.getMoneyAvailable(), howMuchCanConsume);//pay all money cause you don't have more                                                                        
                                return howMuchCanConsume;
                            }
                            else
                                return howMuchCanConsume;
                        }
                    }
                    else
                        return howMuchAvailable;
                }
            }
            else
                return whatWantedToBuy; // assuming buying is empty here            
        }

        /// <summary>
        /// Buys, returns actually bought, subsidizations allowed, uses deposits if available
        /// </summary>
        public Storage Sell(Consumer toWhom, Storage need, Country subsidizer)
        {
            if (toWhom.CanAfford(need) || subsidizer == null)
            {
                return Sell(toWhom, need);
            }
            //todo fix that
            else if (subsidizer.GiveFactorySubsidies(toWhom, toWhom.HowMuchLacksMoneyIncludingDeposits(getCost(need))))
            {
                return Sell(toWhom, need);
            }
            else
                return new Storage(need.Product, 0f);
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
            return sentToMarket.getBiggestStorage(need.Product);

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
                foreach (Storage nextProduct in marketPrice)
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

            foreach (Storage price in marketPrice)
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
            sentToMarket.setZero();
        }

        public override string ToString()
        {
            return "Global market";
        }
    }
}