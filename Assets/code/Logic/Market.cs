using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Represent World market, currently exists only in 1 instance (Game.market)
/// </summary>
public class Market : Agent//: PrimitiveStorageSet
{
    private readonly StorageSet marketPrice = new StorageSet();

    // todo make Better class for it? - yes
    private MyDate dateOfDSB = new MyDate(int.MaxValue);
    private readonly StorageSet DSBbuffer = new StorageSet();

    private MyDate dateOfgetSupplyOnMarket = new MyDate(int.MaxValue);
    private readonly StorageSet supplyOnMarket = new StorageSet();

    private MyDate dateOfgetTotalProduction = new MyDate(int.MaxValue);
    private readonly StorageSet totalProduction = new StorageSet();

    private MyDate dateOfgetTotalConsumption = new MyDate(int.MaxValue);
    private readonly StorageSet totalConsumption = new StorageSet();

    private MyDate dateOfgetBought = new MyDate(int.MaxValue);
    private readonly StorageSet bought = new StorageSet();

    internal PricePool priceHistory;
    internal StorageSet sentToMarket = new StorageSet();
    public Market() : base(0f, null, null)
    { }
    internal Value getPrice(Product whom)
    {
        return marketPrice.getCheapestStorage(whom);
    }
    internal void initialize()
    {
        priceHistory = new PricePool();
    }

    internal Value getCost(StorageSet need)
    {
        Value cost = new Value(0f);
        // float price;
        foreach (Storage stor in need)
        {
            //price = Game.market.findPrice(stor.getProduct()).get();
            cost.add(getCost(stor));
        }
        return cost;
    }

    /// <summary>
    /// returns new Value
    /// </summary>
    internal Value getCost(List<Storage> need)
    {
        Value cost = new Value(0f);
        foreach (Storage stor in need)
            cost.add(getCost(stor));
        return cost;
    }
    /// <summary>
    /// returns new Value
    /// </summary>
    internal Value getCost(Storage need)
    {
        // now its fixed - getPrice() takes cheapest substitute product price instead of abstract
        //if (need.isAbstractProduct())
        //    Debug.Log("Can't determinate price of abstract product " + need.getProduct());
        return need.multiplyOutside(Game.market.getPrice(need.getProduct()));
    }




    /// <summary>
    /// Just transfers it to StorageSet.convertToCheapestStorageProduct(Storage)
    /// </summary>    
    internal Storage getCheapestSubstitute(Storage need)
    {
        return marketPrice.convertToCheapestStorageProduct(need);
    }
    private Storage recalculateProductForConsumers(Product product, Func<Consumer, StorageSet> selector)
    {
        Storage result = new Storage(product);
        foreach (Country country in Country.getAllExisting())
        {
            foreach (Province province in country.ownedProvinces)
                foreach (Consumer consumer in province.getAllAgents())
                {
                    Storage re = selector(consumer).getFirstStorage(product);
                    result.add(re);
                }
            Storage countryStor = selector(country).getFirstStorage(product);
            result.add(countryStor);
        }
        return result;
    }
    private Storage recalculateProductForBuyers(Product product, Func<Consumer, StorageSet> selector)
    {
        Storage result = new Storage(product);
        foreach (Country country in Country.getAllExisting())
        {
            foreach (Province province in country.ownedProvinces)
                foreach (Consumer consumer in province.getAllBuyers())
                {
                    Storage re = selector(consumer).getFirstStorage(product);
                    result.add(re);
                }
            Storage countryStor = selector(country).getFirstStorage(product);
            result.add(countryStor);
        }
        return result;
    }
    private Storage recalculateProductForSellers(Product product, Func<ICanSell, Storage> selector)
    {
        Storage result = new Storage(product);
        foreach (Country country in Country.getAllExisting())
        {
            foreach (Province province in country.ownedProvinces)
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
    private Storage recalculateProductForProducers(Product product, Func<Producer, Storage> selector)
    {
        Storage result = new Storage(product);
        foreach (Country country in Country.getAllExisting())
        {
            foreach (Province province in country.ownedProvinces)
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
        if (dateOfgetBought != Game.date)
        {
            //recalculate all products
            foreach (Storage recalculatingProduct in marketPrice)
                if (recalculatingProduct.getProduct().isTradable())
                {
                    var result = recalculateProductForConsumers(recalculatingProduct.getProduct(), x => x.getConsumedInMarket());

                    bought.set(new Storage(recalculatingProduct.getProduct(), result));
                }
            dateOfgetBought.set(Game.date);
        }
        return bought.getFirstStorage(product);
    }
    internal Storage getTotalConsumption(Product product, bool takeThisTurnData)
    {
        if (takeThisTurnData)
        {
            return recalculateProductForConsumers(product, x => x.getConsumed());
        }
        if (dateOfgetTotalConsumption != Game.date)
        {
            //recalculate buffer
            foreach (Storage recalculatingProduct in marketPrice)
                if (recalculatingProduct.getProduct().isTradable())
                {
                    var result = recalculateProductForConsumers(recalculatingProduct.getProduct(), x => x.getConsumed());
                    totalConsumption.set(new Storage(recalculatingProduct.getProduct(), result));
                }
            dateOfgetTotalConsumption.set(Game.date);
        }
        return totalConsumption.getFirstStorage(product);
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
        if (dateOfgetSupplyOnMarket != Game.date)
        {
            //recalculate supply buffer
            foreach (Storage recalculatingProduct in marketPrice)
                if (recalculatingProduct.getProduct().isTradable())
                {
                    var result = recalculateProductForSellers(recalculatingProduct.getProduct(), x => x.getSentToMarket(recalculatingProduct.getProduct()));
                    supplyOnMarket.set(new Storage(recalculatingProduct.getProduct(), result));
                }
            dateOfgetSupplyOnMarket.set(Game.date);
        }
        return supplyOnMarket.getFirstStorage(product);
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
        if (dateOfgetTotalProduction != Game.date)
        {
            //recalculate Production buffer
            foreach (Storage recalculatingProduct in marketPrice)
                if (recalculatingProduct.getProduct().isTradable())
                {
                    var result = recalculateProductForProducers(recalculatingProduct.getProduct(), x => x.getGainGoodsThisTurn());
                    totalProduction.set(new Storage(recalculatingProduct.getProduct(), result));
                }
            dateOfgetTotalProduction.set(Game.date);
        }

        return totalProduction.getFirstStorage(product);
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
        marketPrice.set(new Storage(pro, inprice));
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
            if (whatWantedToBuy.getProduct().isAbstract())
            {
                buying = marketPrice.convertToCheapestExistingSubstitute(whatWantedToBuy);
                if (buying == null)//no substitution available on market
                    return new Storage(whatWantedToBuy.getProduct());
            }
            else
                buying = whatWantedToBuy;
            Storage howMuchCanConsume;
            Value price = getPrice(buying.getProduct());
            Value cost;
            if (Game.market.sentToMarket.has(buying))
            {
                cost = buying.multiplyOutside(price);
                //if (cost.isNotZero())
                //{
                if (buyer.canPay(cost))
                {
                    buyer.pay(Game.market, cost);
                    buyer.consumeFromMarket(buying);
                    if (buyer is SimpleProduction)
                        (buyer as SimpleProduction).getInputProductsReserve().add(buying);
                    howMuchCanConsume = buying;
                }
                else
                {
                    float val = buyer.cash.get() / price.get();
                    val = Mathf.Floor(val * Value.precision) / Value.precision;
                    howMuchCanConsume = new Storage(buying.getProduct(), val);
                    buyer.pay(Game.market, howMuchCanConsume.multiplyOutside(price));
                    buyer.consumeFromMarket(howMuchCanConsume);
                    if (buyer is SimpleProduction)
                        (buyer as SimpleProduction).getInputProductsReserve().add(howMuchCanConsume);
                }
                //}
                //else
                //    return new Storage(buying.getProduct(), 0f);
            }
            else
            {
                // assuming available < buying
                Storage howMuchAvailable = new Storage(Game.market.HowMuchAvailable(buying));
                if (howMuchAvailable.get() > 0f)
                {
                    cost = howMuchAvailable.multiplyOutside(price);
                    if (buyer.canPay(cost))
                    {
                        buyer.pay(Game.market, cost);
                        buyer.consumeFromMarket(howMuchAvailable);
                        if (buyer is SimpleProduction)
                            (buyer as SimpleProduction).getInputProductsReserve().add(howMuchAvailable);
                        howMuchCanConsume = howMuchAvailable;
                    }
                    else
                    {
                        howMuchCanConsume = new Storage(howMuchAvailable.getProduct(), buyer.cash.get() / price.get());
                        if (howMuchCanConsume.get() > howMuchAvailable.get())
                            howMuchCanConsume.set(howMuchAvailable.get()); // you don't buy more than there is
                        if (howMuchCanConsume.isNotZero())
                        {
                            buyer.sendAllAvailableMoney(Game.market); //pay all money cause you don't have more                                                                     
                            buyer.consumeFromMarket(howMuchCanConsume);
                            if (buyer is SimpleProduction)
                                (buyer as SimpleProduction).getInputProductsReserve().add(howMuchCanConsume);
                        }
                    }
                }
                else
                    howMuchCanConsume = new Storage(buying.getProduct(), 0f);
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
        if (forWhom.canAfford(need) || subsidizer == null)
            return buy(forWhom, need);
        else
        {
            subsidizer.takeFactorySubsidies(forWhom, forWhom.howMuchMoneyCanNotPay(need));
            return buy(forWhom, need);
        }
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
    internal bool buy(Producer buyer, StorageSet stillHaveToBuy, Procent buyInTime, StorageSet ofWhat)
    {
        bool buyingIsFinished = true;
        foreach (Storage what in ofWhat)
        {
            Storage consumeOnThisIteration = new Storage(what.getProduct(), what.get() * buyInTime.get());
            if (consumeOnThisIteration.isZero())
                return true;

            // check if consumeOnThisIteration is not bigger than stillHaveToBuy
            if (!stillHaveToBuy.has(consumeOnThisIteration))
                consumeOnThisIteration = stillHaveToBuy.getBiggestStorage(what.getProduct());
            var reallyBought = buy(buyer, consumeOnThisIteration, null); 

            stillHaveToBuy.subtract(reallyBought);

            if (stillHaveToBuy.getBiggestStorage(what.getProduct()).isNotZero())
                buyingIsFinished = false;
        }
        return buyingIsFinished;
    }

    /// <summary>
    /// Date actual for how much produced on turn start, not how much left
    /// </summary>   
    //internal bool HasProducedThatMuch(Storage need)
    //{
    //    //Storage availible = findStorage(need.getProduct());
    //    //if (availible.get() >= need.get()) return true;
    //    //else return false;
    //    Storage availible = HowMuchProduced(need.getProduct());
    //    if (availible.get() >= need.get()) return true;
    //    else return false;
    //}
    /// <summary>
    /// Must be safe - returns new Storage
    /// Date actual for how much produced on turn start, not how much left
    /// </summary>
    //internal Storage HowMuchProduced(Product need)
    //{
    //    //return findStorage(need.getProduct());
    //    // here DSB is based not on last turn data, but on this turn.
    //    return new Storage(need, getMarketSupply(need, false));
    //}
    /// <summary>
    /// Based on DSB, assuming you have enough money
    /// </summary>    
    internal bool HasAvailable(Storage need)
    {
        //Storage availible = findStorage(need.getProduct());
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
        return sentToMarket.getBiggestStorage(need.getProduct());

        //BuyingAmountAvailable = need.get() / DSB;

        //float DSB = getDemandSupplyBalance(need.getProduct());
        //float BuyingAmountAvailable = 0;

        //if (DSB < 1f) DSB = 1f;
        //BuyingAmountAvailable = need.get() / DSB;

        //return new Storage(need.getProduct(), BuyingAmountAvailable);
    }

    /// <summary>
    /// Result > 1 mean demand is higher, price should go up   Result fewer 1 mean supply is higher, price should go down
    /// based on last turn data   
    ///</summary>
    internal float getDemandSupplyBalance(Product product)
    {
        //Debug.Log("I'm in DSBBalancer, dateOfDSB = " + dateOfDSB);
        float balance;

        if (dateOfDSB != Game.date)
        // recalculate DSBbuffer
        {
            //Debug.Log("Recalculation of DSB started");
            foreach (Storage nextProduct in marketPrice)
                if (nextProduct.getProduct().isTradable())
                {
                    getProductionTotal(product, false); // for pre-turn initialization
                    getTotalConsumption(product, false);// for pre-turn initialization
                    float supply = getMarketSupply(nextProduct.getProduct(), false).get();
                    float demand = getBouthOnMarket(nextProduct.getProduct(), false).get();

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
                    DSBbuffer.set(new Storage(nextProduct.getProduct(), balance));
                }
            dateOfDSB.set(Game.date);
        }
        return DSBbuffer.getFirstStorage(product).get();
    }

    /// <summary>
    /// Changes price for every product in market
    /// That's first call for DSB in tick
    /// </summary>
    public void simulatePriceChangeBasingOnLastTurnData()
    {
        float balance;
        float priceChangeSpeed = 0;
        float highestChangingSpeed = 0.2f; //%
        float highChangingSpeed = 0.04f;//%
        float antiBalance;
        foreach (Storage price in this.marketPrice)
            if (price.getProduct().isTradable())
            {
                balance = getDemandSupplyBalance(price.getProduct());
                /// Result > 1 mean demand is higher, price should go up  
                /// Result fewer 1 mean supply is higher, price should go down              

                //if (balance < 1f) antiBalance = 1 / balance;
                //else antiBalance = balance;
                priceChangeSpeed = 0;
                if (balance >= 1f)//0.95f)
                    priceChangeSpeed = 0.001f + price.get() * 0.1f;
                else
                {
                    if (balance <= 0.8f)
                        priceChangeSpeed = -0.001f + price.get() * -0.02f;
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
            //if (getBouth(price.getProduct()) != 0) newValue = Game.maxPrice / 20f;
        }
        price.set(newValue);
        priceHistory.addData(price.getProduct(), price);
    }

    public override void simulate()
    {
        throw new NotImplementedException();
    }
}