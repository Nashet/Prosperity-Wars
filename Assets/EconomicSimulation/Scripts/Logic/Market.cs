using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represent World market, currently exists only in 1 instance (Game.market)
    /// </summary>
    public class Market : Agent//: PrimitiveStorageSet
    {
        private readonly StorageSet marketPrice = new StorageSet();

        // todo make Better class for it? - yes
        private Date dateOfDSB = new Date(int.MaxValue);
        private readonly StorageSet DSBbuffer = new StorageSet();

        private Date dateOfgetSupplyOnMarket = new Date(int.MaxValue);
        private readonly StorageSet supplyOnMarket = new StorageSet();

        private Date dateOfgetTotalProduction = new Date(int.MaxValue);
        private readonly StorageSet totalProduction = new StorageSet();

        private Date dateOfgetTotalConsumption = new Date(int.MaxValue);
        private readonly StorageSet totalConsumption = new StorageSet();

        private Date dateOfgetBought = new Date(int.MaxValue);
        private readonly StorageSet bought = new StorageSet();

        internal PricePool priceHistory;
        internal StorageSet sentToMarket = new StorageSet();
        public Market() : base(null)
        {

        }
        internal Value getPrice(Product whom)
        {
            return marketPrice.getCheapestStorage(whom);
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
        internal Money getCost(StorageSet need)
        {
            Money cost = new Money(0f);
            // float price;
            foreach (Storage stor in need)
            {
                //price = Game.market.findPrice(stor.Product).get();
                cost.Add(getCost(stor));
            }
            return cost;
        }

        /// <summary>
        /// returns new Value
        /// </summary>
        internal Money getCost(List<Storage> need)
        {
            Money cost = new Money(0f);
            foreach (Storage stor in need)
                cost.Add(getCost(stor));
            return cost;
        }
        /// <summary>
        /// 
        /// </summary>
        internal ReadOnlyValue getCost(Storage need)
        {
            // now its fixed - getPrice() takes cheapest substitute product price instead of abstract
            //if (need.isAbstractProduct())
            //    Debug.Log("Can't determinate price of abstract product " + need.Product);
            if (need.Product == Product.Gold)
            {
                var res = need.Copy().Multiply(Options.goldToCoinsConvert);
                res.Multiply(Options.GovernmentTakesShareOfGoldOutput);
                return res;
            }
            else
                return need.Copy().Multiply(Game.market.getPrice(need.Product));
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
                        var DSB = getDemandSupplyBalance(substitute);
                        if (DSB != Options.MarketInfiniteDSB && DSB < Options.MarketEqualityDSB)
                            return true;
                    }
                return false;
            }
            else
            {
                var DSB = getDemandSupplyBalance(product);
                if (DSB != Options.MarketInfiniteDSB && DSB < Options.MarketEqualityDSB)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// returns how much was sold de facto
        /// new version of buy-old,
        /// real deal. If not enough money to buy (including deposits) then buys some part of desired
        /// </summary>   
        internal Storage buy(Consumer buyer, Storage whatWantedToBuy)
        {
            if (whatWantedToBuy.isNotZero())
            {
                Storage buying;
                if (whatWantedToBuy.Product.isAbstract())
                {
                    buying = marketPrice.ConvertToRandomCheapestExistingSubstitute(whatWantedToBuy);
                    if (buying == null)//no substitution available on market
                        return new Storage(whatWantedToBuy.Product);
                }
                else
                    buying = whatWantedToBuy;
                Storage howMuchCanConsume;
                Value price = getPrice(buying.Product);
                Value cost;
                if (Game.market.sentToMarket.has(buying))
                {
                    cost = buying.Copy().Multiply(price);
                    //if (cost.isNotZero())
                    //{
                    if (buyer.CanPay(cost))
                    {
                        buyer.Pay(Game.market, cost);
                        buyer.consumeFromMarket(buying);
                        if (buyer is SimpleProduction)
                            (buyer as SimpleProduction).getInputProductsReserve().Add(buying);
                        howMuchCanConsume = buying;
                    }
                    else
                    {
                        float val = buyer.Cash.get() / price.get();
                        val = Mathf.Floor(val * Value.Precision) / Value.Precision;
                        howMuchCanConsume = new Storage(buying.Product, val);
                        buyer.Pay(Game.market, howMuchCanConsume.Copy().Multiply(price));
                        buyer.consumeFromMarket(howMuchCanConsume);
                        if (buyer is SimpleProduction)
                            (buyer as SimpleProduction).getInputProductsReserve().Add(howMuchCanConsume);
                    }
                    //}
                    //else
                    //    return new Storage(buying.Product, 0f);
                }
                else
                {
                    // assuming available < buying
                    Storage howMuchAvailable = new Storage(Game.market.HowMuchAvailable(buying));
                    if (howMuchAvailable.get() > 0f)
                    {
                        cost = howMuchAvailable.Copy().Multiply(price);
                        if (buyer.CanPay(cost))
                        {
                            buyer.Pay(Game.market, cost);
                            buyer.consumeFromMarket(howMuchAvailable);
                            if (buyer is SimpleProduction)
                                (buyer as SimpleProduction).getInputProductsReserve().Add(howMuchAvailable);
                            howMuchCanConsume = howMuchAvailable;
                        }
                        else
                        {
                            howMuchCanConsume = new Storage(howMuchAvailable.Product, buyer.Cash.get() / price.get());
                            if (howMuchCanConsume.get() > howMuchAvailable.get())
                                howMuchCanConsume.Set(howMuchAvailable.get()); // you don't buy more than there is
                            if (howMuchCanConsume.isNotZero())
                            {
                                buyer.PayAllAvailableMoney(Game.market); //pay all money cause you don't have more                                                                     
                                buyer.consumeFromMarket(howMuchCanConsume);
                                if (buyer is SimpleProduction)
                                    (buyer as SimpleProduction).getInputProductsReserve().Add(howMuchCanConsume);
                            }
                        }
                    }
                    else
                        howMuchCanConsume = new Storage(buying.Product, 0f);
                }
                return howMuchCanConsume;
            }
            else
                return whatWantedToBuy; // assuming buying is empty here
        }


        /// <summary>
        /// Buys, returns actually bought, subsidizations allowed, uses deposits if available
        /// </summary>    
        public Storage buy(Consumer forWhom, Storage need, Country subsidizer)
        {
            if (forWhom.CanAfford(need) || subsidizer == null)
                return buy(forWhom, need);
            //todo fix that
            else if (subsidizer.GiveFactorySubsidies(forWhom, forWhom.HowMuchLacksMoneyIncludingDeposits(need)))
                return buy(forWhom, need);
            else
                return new Storage(need.Product, 0f);
        }

        /// <summary>
        /// Buying PrimitiveStorageSet, subsidizations allowed
        /// </summary>
        internal void buy(Consumer buyer, StorageSet buying, Country subsidizer)
        {
            foreach (Storage item in buying)
                if (item.isNotZero())
                    buy(buyer, item, subsidizer);
        }


        /// <summary>
        /// Buying needs in circle, by Procent in time
        /// return true if buying is zero (bought all what it wanted)
        /// </summary>    
        internal bool buy(Producer buyer, StorageSet stillHaveToBuy, Procent buyInTime, List<Storage> ofWhat)
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
                var reallyBought = buy(buyer, consumeOnThisIteration, null);

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
        internal float getDemandSupplyBalance(Product product)
        {
            if (product == Product.Gold)
                return Options.MarketInfiniteDSB;
            //Debug.Log("I'm in DSBBalancer, dateOfDSB = " + dateOfDSB);
            float balance;

            if (!dateOfDSB.IsToday)
            // recalculate DSBbuffer
            {
                //Debug.Log("Recalculation of DSB started");
                foreach (Storage nextProduct in marketPrice)
                    if (nextProduct.Product.isTradable())
                    {
                        getProductionTotal(product, false); // for pre-turn initialization
                        getTotalConsumption(product, false);// for pre-turn initialization
                        float supply = getMarketSupply(nextProduct.Product, false).get();
                        float demand = getBouthOnMarket(nextProduct.Product, false).get();

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

            foreach (Storage price in this.marketPrice)
                if (price.Product.isTradable())
                {
                    balance = getDemandSupplyBalance(price.Product);
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
            if (newValue <= 0)
                newValue = Options.minPrice;
            if (newValue >= Options.maxPrice)
            {
                newValue = Options.maxPrice;
                //if (getBouth(price.Product) != 0) newValue = Game.maxPrice / 20f;
            }
            price.Set(newValue);
            priceHistory.addData(price.Product, price);
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }
    }
}