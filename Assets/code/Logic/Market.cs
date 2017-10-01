using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Represent World market (should be only static)
/// </summary>
public class Market : Agent//: PrimitiveStorageSet
{
    internal PrimitiveStorageSet marketPrice = new PrimitiveStorageSet();

    // todo make Better class for it?
    DateTime dateOfDSB = new DateTime(int.MaxValue);
    PrimitiveStorageSet DSBbuffer = new PrimitiveStorageSet();

    DateTime dateOfgetSupplyOnMarket = new DateTime(int.MaxValue);
    PrimitiveStorageSet supplyOnMarket = new PrimitiveStorageSet();

    DateTime dateOfgetTotalProduction = new DateTime(int.MaxValue);
    PrimitiveStorageSet totalProduction = new PrimitiveStorageSet();

    DateTime dateOfgetTotalConsumption = new DateTime(int.MaxValue);
    PrimitiveStorageSet totalConsumption = new PrimitiveStorageSet();

    DateTime dateOfgetBought = new DateTime(int.MaxValue);
    PrimitiveStorageSet bought = new PrimitiveStorageSet();

    internal PricePool priceHistory;
    internal PrimitiveStorageSet sentToMarket = new PrimitiveStorageSet();
    public Market() : base(0f, null, null)
    { }
    internal Storage findPrice(Product whom)
    {
        return marketPrice.findStorage(whom);
    }
    internal void initialize()
    {
        priceHistory = new PricePool();
    }
    /// <summary>
    /// Including potentially unsold goods
    /// Basing on last turn production
    /// </summary>    
    //float getTotalDemand(Product pro)
    //{

    //}
    internal Value getCost(PrimitiveStorageSet need)
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
    //internal Value getCost(Storage need)
    //{
    //    float cost = 0;
    //    // float price;

    //    return new Value(cost);
    //}

    internal Value getCost(List<Storage> need)
    {//todo convert return in Value??
        Value cost = new Value(0f);
        // float price;
        foreach (Storage stor in need)
        {
            //price = Game.market.findPrice(stor.getProduct()).get();
            //cost += getCost(stor);
            cost.add(getCost(stor));
        }
        //return new Value(cost);
        return cost;
    }
    internal Value getCost(Storage need)
    {
        //float price;
        //price = Game.market.findPrice(need.getProduct()).get();
        //cost = need.get() * price;

        return need.multiplyOutside(Game.market.findPrice(need.getProduct())) as Value;
    }
    /// <summary>
    /// Meaning demander actually can pay for item in current prices
    /// Basing on current prices and needs
    /// Not counting ConsumedInMarket
    /// </summary>    
    internal float getBouth(Product pro, bool takeThisTurnData)
    {
        float result = 0f;
        if (takeThisTurnData)
        {
            foreach (Country country in Country.getExisting())
            {
                foreach (Province province in country.ownedProvinces)
                    foreach (Producer producer in province.getBuyers())
                    {
                        //if (any.c.getProduct() == sup.getProduct()) //sup.getProduct()
                        {
                            Storage re = producer.consumedInMarket.findStorage(pro);
                            if (re != null)
                                result += re.get();
                        }
                    }
                Storage countryStor = country.consumedInMarket.findStorage(pro);
                if (countryStor != null)
                    result += countryStor.get();
            }
            return result;
        }
        if (dateOfgetBought != Game.date)
        {
            //recalculate supply buffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.getExisting())
                {
                    foreach (Province province in country.ownedProvinces)
                        foreach (Producer producer in province.getBuyers())
                        {
                            //if (any.c.getProduct() == sup.getProduct()) //sup.getProduct()
                            {
                                Storage re = producer.consumedInMarket.findStorage(sup.getProduct());
                                if (re != null)
                                    result += re.get();
                            }
                        }
                    Storage countryStor = country.consumedInMarket.findStorage(sup.getProduct());
                    if (countryStor != null)
                        result += countryStor.get();
                }
                bought.set(new Storage(sup.getProduct(), result));
            }
            dateOfgetBought = Game.date;
        }
        Storage tmp = bought.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return bought.findStorage(pro).get();
        //float result = 0f;
        //foreach (Country country in Country.allCountries)
        //    foreach (Province province in country.ownedProvinces)
        //    {
        //        foreach (Producer shownFactory in province)
        //            //result += shownFactory.getLocalEffectiveDemand(pro);
        //            if (shownFactory.consumedInMarket.findStorage(pro) != null)
        //                result += shownFactory.consumedInMarket.findStorage(pro).get();


        //        //foreach (PopUnit pop in province.allPopUnits)
        //        //    //result += pop.getLocalEffectiveDemand(pro);
        //        //    if (pop.consumedInMarket.findStorage(pro) != null)
        //        //        result += pop.consumedInMarket.findStorage(pro).get();
        //        // todo add same for country and any demander
        //    }
        //return result;
    }
    /// <summary>
    /// Meaning demander actually can pay for item in current prices
    /// Basing on current prices and needs
    /// Not counting ConumedInMarket
    /// </summary>    
    internal float getTotalConsumption(Product pro, bool takeThisTurnData)
    {
        float result = 0f;
        if (takeThisTurnData)
        {
            foreach (Country country in Country.getExisting())
            {
                foreach (Province province in country.ownedProvinces)
                    foreach (Producer producer in province.getConsumers())
                    {
                        //if (any.gainGoodsThisTurn.getProduct() == sup.getProduct()) //sup.getProduct()
                        {
                            var re = producer.consumedTotal.findStorage(pro);
                            if (re != null)
                                result += re.get();
                        }
                    }
                Storage countryStor = country.consumedTotal.findStorage(pro);
                if (countryStor != null)
                    result += countryStor.get();
            }
            return result;
        }
        if (dateOfgetTotalConsumption != Game.date)
        {
            //recalculate buffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.getExisting())
                {
                    foreach (Province province in country.ownedProvinces)
                        foreach (Producer producer in province.getConsumers())
                        {
                            //if (any.gainGoodsThisTurn.getProduct() == sup.getProduct()) //sup.getProduct()
                            {
                                var re = producer.consumedTotal.findStorage(sup.getProduct());
                                if (re != null)
                                    result += re.get();
                            }
                        }
                    Storage countryStor = country.consumedTotal.findStorage(sup.getProduct());
                    if (countryStor != null)
                        result += countryStor.get();
                }
                totalConsumption.set(new Storage(sup.getProduct(), result));
            }
            dateOfgetTotalConsumption = Game.date;
        }
        Storage tmp = totalConsumption.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return totalConsumption.findStorage(pro).get();
        return result;
        ////////////
        //float result = 0f;
        //foreach (Country country in Country.allCountries)
        //    foreach (Province province in country.ownedProvinces)
        //    {
        //        foreach (Producer shownFactory in province)
        //            //result += shownFactory.getLocalEffectiveDemand(pro);
        //            if (shownFactory.consumedTotal.findStorage(pro) != null)
        //                result += shownFactory.consumedTotal.findStorage(pro).get();


        //        //foreach (PopUnit pop in province.allPopUnits)
        //        //    //result += pop.getLocalEffectiveDemand(pro);
        //        //    if (pop.consumedTotal.findStorage(pro) != null)
        //        //        result += pop.consumedTotal.findStorage(pro).get();
        //        // todo add same for country and any demander
        //    }
        //return result;
    }

    internal bool isAvailable(Product item)
    {
        var DSB = getDemandSupplyBalance(item);
        if (DSB != Options.MarketInfiniteDSB && DSB != Options.MarketEqualityDSB)
            return true;
        else
            return false;
    }

    //internal float getGlobalEffectiveDemandOlder(Product pro)
    //{
    //    float result = 0f;
    //    foreach (Country country in Country.getExisting())
    //        foreach (Province province in country.ownedProvinces)
    //        {
    //            foreach (Factory factory in province.allFactories)
    //                result += factory.getLocalEffectiveDemand(pro);
    //            //if (shownFactory.consumedTotal.findStorage(pro) != null)
    //            //    result += shownFactory.consumedTotal.findStorage(pro).get();


    //            foreach (PopUnit pop in province.allPopUnits)
    //                result += pop.getLocalEffectiveDemand(pro);
    //            //if (pop.consumedTotal.findStorage(pro) != null)
    //            //    result += pop.consumedTotal.findStorage(pro).get();
    //            // todo add same for country and any demander
    //        }
    //    return result;
    //}



    /// <summary>
    /// Only goods sent to market
    /// Based  on last turn data
    /// </summary>    
    internal float getSupply(Product pro, bool takeThisTurnData)
    {
        float result = 0f;
        if (takeThisTurnData)
        {
            foreach (Country country in Country.getExisting())
                foreach (Province province in country.ownedProvinces)
                    foreach (Producer producer in province.getProducers())
                        if (producer.sentToMarket.getProduct() == pro) //sup.getProduct()
                            result += producer.sentToMarket.get();
            return result;
        }
        if (dateOfgetSupplyOnMarket != Game.date)
        {
            //recalculate supply buffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.getExisting())
                    foreach (Province province in country.ownedProvinces)
                        foreach (Producer producer in province.getProducers())
                            if (producer.sentToMarket.getProduct() == sup.getProduct()) //sup.getProduct()
                                result += producer.sentToMarket.get();

                supplyOnMarket.set(new Storage(sup.getProduct(), result));
            }
            dateOfgetSupplyOnMarket = Game.date;
        }
        Storage tmp = supplyOnMarket.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return supplyOnMarket.findStorage(pro).get();
        //return result;
    }
    /// <summary>
    /// All produced supplies
    /// Based  on last turn data
    /// </summary>    
    internal float getProductionTotal(Product pro, bool takeThisTurnData)
    {
        float result = 0f;
        if (takeThisTurnData)
        {
            foreach (Country country in Country.getExisting())
                foreach (Province province in country.ownedProvinces)
                {
                    foreach (Producer producer in province.getProducers())
                    {
                        if (producer.gainGoodsThisTurn.getProduct() == pro) //sup.getProduct()
                            result += producer.gainGoodsThisTurn.get();
                    }
                }
            return result;
        }
        if (dateOfgetTotalProduction != Game.date)
        {
            //recalculate Production buffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.getExisting())
                    foreach (Province province in country.ownedProvinces)
                    {
                        foreach (Producer producer in province.getProducers())
                        {
                            if (producer.gainGoodsThisTurn.getProduct() == sup.getProduct()) //sup.getProduct()
                                result += producer.gainGoodsThisTurn.get();
                        }
                    }
                totalProduction.set(new Storage(sup.getProduct(), result));
            }
            dateOfgetTotalProduction = Game.date;
        }
        Storage tmp = totalProduction.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return totalProduction.findStorage(pro).get();
        return result;
    }
    internal void ForceDSBRecalculation()
    {

        //dateOfDSB--;//!!! Warning! This need to be uncommented to work properly
        getDemandSupplyBalance(null);
    }


    /// <summary>
    /// per 1 unit
    /// </summary>
    //Value defaultPrice = new Value(2f);
    public void SetDefaultPrice(Product pro, float inprice)
    {
        marketPrice.set(new Storage(pro, inprice));
    }

    /// <summary>
    /// returns how much was sold de-facto
    /// new version of buy-old,
    /// real deal
    /// </summary>   
    internal Storage buy(Consumer buyer, Storage whatWantedToBuy)
    {
        if (whatWantedToBuy.isNotZero())
        {
            Storage buying;
            if (whatWantedToBuy.getProduct().isAbstract())
            {
                buying = getCheapestSubstitute(whatWantedToBuy);
                if (buying == null)//no substitution available on market
                    return new Storage(whatWantedToBuy.getProduct());
            }
            else
                buying = whatWantedToBuy;
            Storage howMuchCanConsume;
            Storage price = findPrice(buying.getProduct());
            Value cost;
            if (Game.market.sentToMarket.has(buying))
            {
                cost = buying.multiplyOutside(price);
                if (buyer.canPay(cost))
                {
                    buyer.pay(Game.market, cost);
                    Game.market.sentToMarket.subtract(buying);
                    buyer.consumedTotal.add(buying);
                    buyer.consumedInMarket.add(buying);

                    if (buyer is SimpleProduction)
                        (buyer as SimpleProduction).getInputProductsReserve().add(buying);
                    howMuchCanConsume = buying;
                }
                else
                {
                    float val = buyer.cash.get() / price.get();
                    val = Mathf.Floor(val * Value.precision) / Value.precision;
                    howMuchCanConsume = new Storage(price.getProduct(), val);
                    buyer.pay(Game.market, howMuchCanConsume.multiplyOutside(price));
                    Game.market.sentToMarket.subtract(howMuchCanConsume);
                    buyer.consumedTotal.add(howMuchCanConsume);
                    buyer.consumedInMarket.add(howMuchCanConsume);
                    if (buyer is SimpleProduction)
                        (buyer as SimpleProduction).getInputProductsReserve().add(howMuchCanConsume);
                }
            }
            else
            {
                // assuming available < buying
                Storage howMuchAvailable = Game.market.HowMuchAvailable(buying);
                if (howMuchAvailable.get() > 0f)
                {
                    cost = howMuchAvailable.multiplyOutside(price);
                    if (buyer.canPay(cost))
                    {
                        buyer.pay(Game.market, cost);
                        Game.market.sentToMarket.subtract(howMuchAvailable);
                        buyer.consumedTotal.add(howMuchAvailable);
                        buyer.consumedInMarket.add(howMuchAvailable);
                        if (buyer is SimpleProduction)
                            (buyer as SimpleProduction).getInputProductsReserve().add(howMuchAvailable);
                        howMuchCanConsume = howMuchAvailable;
                    }
                    else
                    {
                        howMuchCanConsume = new Storage(price.getProduct(), buyer.cash.get() / price.get());
                        if (howMuchCanConsume.get() > howMuchAvailable.get())
                            howMuchCanConsume.set(howMuchAvailable.get()); // you don't buy more than there is
                        buyer.sendAllAvailableMoney(Game.market); //pay all money cause you don't have more
                        Game.market.sentToMarket.subtract(howMuchCanConsume);
                        buyer.consumedTotal.add(howMuchCanConsume);
                        buyer.consumedInMarket.add(howMuchCanConsume);
                        if (buyer is SimpleProduction)
                            (buyer as SimpleProduction).getInputProductsReserve().add(howMuchCanConsume);
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

    public Storage getCheapestSubstitute(Storage abstractProduct)
    {
        // assuming substitutes are sorted in cheap-expensive order
        foreach (var item in abstractProduct.getProduct().getSubstitutes())
        {
            Storage substitute = new Storage(item, abstractProduct);
            // check for availability
            if (Game.market.sentToMarket.has(substitute))
                return substitute;
        }
        return null;
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
    internal void buy(Consumer buyer, PrimitiveStorageSet buying, Country subsidizer)
    {
        foreach (Storage input in buying)
        {
            buy(buyer, input, subsidizer);
        }
    }
    /// <summary>
    /// Buying needs in circle, by Procent in time
    /// return true if buying is zero (bought all what it wanted)
    /// </summary>    
    internal bool buy(Producer buyer, PrimitiveStorageSet stillHaveToBuy, Procent buyInTime, PrimitiveStorageSet ofWhat)
    {
        bool buyingIsFinished = true;
        foreach (Storage what in ofWhat)
        {
            Storage consumeOnThisEteration = new Storage(what.getProduct(), what.get() * buyInTime.get());
            if (consumeOnThisEteration.isZero())
                return true;
            // check if buying still have enough to subtract consumeOnThisEteration
            if (!stillHaveToBuy.has(consumeOnThisEteration))
                consumeOnThisEteration = stillHaveToBuy.getStorage(what.getProduct());
            var reallyBought = buy(buyer, consumeOnThisEteration, null);

            stillHaveToBuy.subtract(reallyBought);

            if (stillHaveToBuy.getStorage(what.getProduct()).isNotZero())
                buyingIsFinished = false;
        }
        return buyingIsFinished;
    }

    /// <summary>
    /// Date actual for how much produced on turn start, not how much left
    /// </summary>   
    internal bool HasProducedThatMuch(Storage need)
    {
        //Storage availible = findStorage(need.getProduct());
        //if (availible.get() >= need.get()) return true;
        //else return false;
        Storage availible = HowMuchProduced(need.getProduct());
        if (availible.get() >= need.get()) return true;
        else return false;
    }
    /// <summary>
    /// Must be safe - returns new Storage
    /// Date actual for how much produced on turn start, not how much left
    /// </summary>
    internal Storage HowMuchProduced(Product need)
    {
        //return findStorage(need.getProduct());
        // here DSB is based not on last turn data, but on this turn.
        return new Storage(need, getSupply(need, false));
    }
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
        Storage result = sentToMarket.findStorage(need.getProduct());
        if (result == null)
            return new Storage(need.getProduct(), 0f);
        else
            return new Storage(result);

        //BuyingAmountAvailable = need.get() / DSB;

        //float DSB = getDemandSupplyBalance(need.getProduct());
        //float BuyingAmountAvailable = 0;

        //if (DSB < 1f) DSB = 1f;
        //BuyingAmountAvailable = need.get() / DSB;

        //return new Storage(need.getProduct(), BuyingAmountAvailable);
    }
    /// <summary>    
    /// Based on DSB, shows how much you can get assuming you have enough money

    /// </summary>
    internal List<Storage> CutToAvailable(List<Storage> need)
    {
        foreach (Storage stor in need)
        {
            stor.set(HowMuchAvailable(stor));
        }

        return need;
    }
    /// <summary>
    /// Result > 1 mean demand is higher, price should go up   Result fewer 1 mean supply is higher, price should go down
    /// based on last turn data    
    internal float getDemandSupplyBalance(Product pro)
    {
        float balance;
        //if (dateOfDSB != Game.date)
        if (dateOfDSB != Game.date)
        // recalculate DSBbuffer
        {

            foreach (Storage stor in marketPrice)
            {
                getProductionTotal(pro, false); // for pre-turn initialization
                getTotalConsumption(pro, false);// for pre-turn initialization
                float supply = getSupply(stor.getProduct(), false);
                float demand = getBouth(stor.getProduct(), false);
                //if (demand == 0) getTotalConsumption(stor.getProduct());
                //else
                ////if (demand == 0) balance = 1f;
                ////else
                //if (supply == 0) balance = 2f;
                //else

                if (supply == 0)
                    balance = Options.MarketInfiniteDSB;
                else
                    balance = demand / supply;

                //if (balance > 1f) balance = 1f;
                //&& supply == 0
                if (demand == 0) balance = 0f; // otherwise - furniture bag
                                               // else
                if (supply == 0)
                    balance = Options.MarketInfiniteDSB;

                DSBbuffer.set(new Storage(stor.getProduct(), balance));
            }
            dateOfDSB = Game.date;
        }
        Storage tmp = DSBbuffer.findStorage(pro);

        if (tmp == null)
            return float.NaN;
        else
            return tmp.get();
    }
    /// <summary>
    /// Result > 1 mean demand is higher, price should go up   Result fewer 1 mean supply is higher, price should go down
    /// based on last turn data    
    //internal float getDemandSupplyBalanceOldNOLder(Product pro)
    //{
    //    float balance;
    //    if (dateOfDSB != Game.date)
    //    // recalculate DSBbuffer
    //    {

    //        foreach (Storage stor in marketPrice)
    //        {

    //            float supply = getSupply(stor.getProduct());
    //            float demand = getBouth(stor.getProduct());
    //            //if (demand == 0) getTotalConsumption(stor.getProduct());
    //            //else
    //            ////if (demand == 0) balance = 1f;
    //            ////else
    //            //if (supply == 0) balance = 2f;
    //            //else

    //            balance = demand / supply;

    //            //if (balance > 1f) balance = 1f;
    //            //&& supply == 0
    //            if (demand == 0) balance = 0f; // otherwise - furniture bag
    //                                           // else
    //            if (supply == 0) balance = float.MaxValue;
    //            //if (float.IsInfinity(balance)) // if divided by zero, s = zero
    //            //    balance = float.NaN;
    //            DSBbuffer.Set(new Storage(stor.getProduct(), balance));
    //        }
    //        dateOfDSB = Game.date;
    //    }
    //    Storage tmp = DSBbuffer.findStorage(pro);

    //    if (tmp == null)
    //        return float.NaN;
    //    else
    //        return tmp.get();
    //}

    /// <summary>
    /// Changes price for every product in market
    /// That's first call for DSB in tick
    /// </summary>
    public void simulatePriceChangeBasingOnLastTurnDate()
    {
        float balance;
        float priceChangeSpeed = 0;
        float highestChangingSpeed = 0.2f; //%
        float highChangingSpeed = 0.04f;//%
        float antiBalance;
        foreach (Storage price in this.marketPrice)
            if (price.getProduct() != Product.Gold)
            {
                balance = getDemandSupplyBalance(price.getProduct());
                /// Result > 1 mean demand is higher, price should go up  
                /// Result fewer 1 mean supply is higher, price should go down               
                //if (getSupply(price.getProduct()) == 0)
                //    balance = 1;
                //if (balance < 1f) antiBalance = 1 / balance;
                //else antiBalance = balance;
                priceChangeSpeed = 0;
                if (balance == 1f)
                    priceChangeSpeed = 0.001f + price.get() * 0.1f;
                else
                {
                    //if (balance > 1f && getSupply(price.getProduct()) == 0f) priceChangeSpeed = 0;
                    // else
                    //(0.0001f <= balance &&
                    if (balance <= 0.75f)
                        priceChangeSpeed = -0.001f + price.get() * -0.02f;
                    else // balance > 1
                    {
                        priceChangeSpeed = 0.001f + price.get() * 0.01f;
                        //if (balance > 1f) // including infinity!
                        //   priceChangeSpeed = price.getProduct().getDefaultPrice().get() - price.get();                        
                    }
                }
                // antiBalance = price.get();
                //if (antiBalance > 10) antiBalance = 10;
                // old DSB
                //if (balance >= 4) priceChangeSpeed = antiBalance * highestChangingSpeed * 10f;
                //else
                //    if (balance > 2) priceChangeSpeed = antiBalance * highChangingSpeed * 10f;
                //else
                //    if (balance > 1 && balance <= 2) priceChangeSpeed = 0.01f * 10f;
                //else
                //    if (balance > 0.5 && balance < 1) priceChangeSpeed = -0.01f;
                //else
                //    if (balance <= 0.5 && balance > 0.25f) priceChangeSpeed = antiBalance * highChangingSpeed * -1f;
                //else
                //    if (balance <= 0.25) priceChangeSpeed = antiBalance * highestChangingSpeed * -1f;
                //else
                //    if (balance == float.NaN) priceChangeSpeed = 0.04f;

                // if (balance > 1) priceChangeSpeed = 1;
                //if (balance < 1) balance = 1 / balance;
                // if (priceChangeSpeed != 0f)
                ChangePrice(price, priceChangeSpeed);
                //if (balance < 1f || balance == float.NaN) ChangePrice(price, -0.01f);
                //else
                //    if (balance > 1f) ChangePrice(price, 0.01f);
                // do nothing if balance is perfect "1"
            }
    }

    private void ChangePrice(Storage price, float HowMuch)
    {
        float newValue = HowMuch + price.get();
        if (newValue <= 0) newValue = Options.minPrice;
        if (newValue >= Options.maxPrice)
        {
            newValue = Options.maxPrice;
            //if (getBouth(price.getProduct()) != 0) newValue = Game.maxPrice / 20f;
        }
        price.set(newValue);
        priceHistory.addData(price.getProduct(), price);
    }

}