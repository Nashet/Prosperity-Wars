using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// directly itself it contains goods storage (amount). No Nahuya?
/// </summary>
public class Market : Owner//: PrimitiveStorageSet
{

    internal PrimitiveStorageSet marketPrice = new PrimitiveStorageSet();
    uint dateOfDSB = uint.MaxValue;
    PrimitiveStorageSet DSBbuffer = new PrimitiveStorageSet();
    uint dateOfgetSupply = uint.MaxValue;
    uint dateOfgetProductionTotal = uint.MaxValue;
    uint dateOfgetTotalConsumption = uint.MaxValue;
    uint dateOfgetBouth = uint.MaxValue;
    PrimitiveStorageSet getSupplyBuffer = new PrimitiveStorageSet();
    PrimitiveStorageSet getProductionTotalBuffer = new PrimitiveStorageSet();
    PrimitiveStorageSet getTotalConsumptionBuffer = new PrimitiveStorageSet();
    PrimitiveStorageSet getBouthBuffer = new PrimitiveStorageSet();
    internal PricePool priceHistory;
    internal PrimitiveStorageSet tmpMarketStorage = new PrimitiveStorageSet();
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
        float cost = 0;
        // float price;
        foreach (Storage stor in need)
        {
            //price = Game.market.findPrice(stor.getProduct()).get();
            cost += getCost(stor);
        }
        return new Value(cost);
    }
    internal float getCost(List<Storage> need)
    {
        float cost = 0;
        // float price;
        foreach (Storage stor in need)
        {
            //price = Game.market.findPrice(stor.getProduct()).get();
            cost += getCost(stor);
        }
        //return new Value(cost);
        return cost;
    }
    internal float getCost(Storage need)
    {
        float cost = 0;
        float price;

        price = Game.market.findPrice(need.getProduct()).get();
        cost = need.get() * price;

        return cost;
    }
    /// <summary>
    /// Meaning demander actyally can pay for item in current prices
    /// Basing on current prices and needs
    /// Not encounting ConsumedInMarket
    /// </summary>    
    internal float getBouth(Product pro)
    {
        float result = 0f;
        if (dateOfgetBouth != Game.date)
        {
            //recalculate supplybuffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.allCountries)
                    foreach (Province province in country.ownedProvinces)
                    {
                        foreach (Producer any in province)
                        {
                            //if (any.c.getProduct() == sup.getProduct()) //sup.getProduct()
                            {
                                Storage re = any.consumedInMarket.findStorage(sup.getProduct());
                                if (re != null)
                                    result += re.get();
                            }
                        }

                    }
                getBouthBuffer.Set(new Storage(sup.getProduct(), result));
            }
            dateOfgetBouth = Game.date;
        }
        Storage tmp = getBouthBuffer.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return getBouthBuffer.findStorage(pro).get();
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
    /// Meaning demander actyally can pay for item in current prices
    /// Basing on current prices and needs
    /// Not encounting ConumedInMarket
    /// </summary>    
    internal float getTotalConsumption(Product pro)
    {
        float result = 0f;
        if (dateOfgetTotalConsumption != Game.date)
        {
            //recalculate supplybuffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.allCountries)
                    foreach (Province province in country.ownedProvinces)
                    {
                        foreach (Producer any in province)
                        {
                            //if (any.gainGoodsThisTurn.getProduct() == sup.getProduct()) //sup.getProduct()
                            {
                                var re = any.consumedTotal.findStorage(sup.getProduct());
                                if (re != null)
                                    result += re.get();
                            }
                        }

                    }
                getTotalConsumptionBuffer.Set(new Storage(sup.getProduct(), result));
            }
            dateOfgetTotalConsumption = Game.date;
        }
        Storage tmp = getTotalConsumptionBuffer.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return getTotalConsumptionBuffer.findStorage(pro).get();
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
    internal float getGlobalEffectiveDemandOlder(Product pro)
    {
        float result = 0f;
        foreach (Country country in Country.allCountries)
            foreach (Province province in country.ownedProvinces)
            {
                foreach (Factory factory in province.allFactories)
                    result += factory.getLocalEffectiveDemand(pro);
                //if (shownFactory.consumedTotal.findStorage(pro) != null)
                //    result += shownFactory.consumedTotal.findStorage(pro).get();


                foreach (PopUnit pop in province.allPopUnits)
                    result += pop.getLocalEffectiveDemand(pro);
                //if (pop.consumedTotal.findStorage(pro) != null)
                //    result += pop.consumedTotal.findStorage(pro).get();
                // todo add same for country and any demander
            }
        return result;
    }



    /// <summary>
    /// Only goods sent to market
    /// Based  on last turn data or on this turn data, respectively
    /// </summary>    
    internal float getSupply(Product pro)
    {
        float result = 0f;
        if (dateOfgetSupply != Game.date)
        {
            //recalculate supplybuffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.allCountries)
                    foreach (Province province in country.ownedProvinces)
                    {
                        foreach (Producer factory in province)
                        {
                            if (factory.sentToMarket.getProduct() == sup.getProduct()) //sup.getProduct()
                                result += factory.sentToMarket.get();
                        }

                        //foreach (PopUnit pop in province.allPopUnits)
                        //    if (pop.sentToMarket.getProduct() == sup.getProduct()) //sup.getProduct()
                        //        result += pop.sentToMarket.get();
                    }
                getSupplyBuffer.Set(new Storage(sup.getProduct(), result));
            }
            dateOfgetSupply = Game.date;
        }
        Storage tmp = getSupplyBuffer.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return getSupplyBuffer.findStorage(pro).get();
        //return result;
    }
    /// <summary>
    /// All produced supplies
    /// Based  on last turn data or on this turn data, correctly
    /// </summary>    
    internal float getProductionTotal(Product pro)
    {
        float result = 0f;
        if (dateOfgetProductionTotal != Game.date)
        {
            //recalculate Productionbuffer
            foreach (Storage sup in marketPrice)
            {
                result = 0;
                foreach (Country country in Country.allCountries)
                    foreach (Province province in country.ownedProvinces)
                    {
                        foreach (Producer factory in province)
                        {
                            if (factory.gainGoodsThisTurn.getProduct() == sup.getProduct()) //sup.getProduct()
                                result += factory.gainGoodsThisTurn.get();
                        }
                        //// todo isServiceProvider()
                        //foreach (PopUnit pop in province.allPopUnits)
                        //    if (pop.gainGoodsThisTurn.getProduct() == sup.getProduct()) //sup.getProduct()
                        //        result += pop.gainGoodsThisTurn.get();
                    }
                getProductionTotalBuffer.Set(new Storage(sup.getProduct(), result));
            }
            dateOfgetProductionTotal = Game.date;
        }
        Storage tmp = getProductionTotalBuffer.findStorage(pro);
        if (tmp == null)
            return 0;
        else
            return getProductionTotalBuffer.findStorage(pro).get();
        return result;
    }
    internal void ForceDSBRecalculation()
    {
        dateOfDSB--;
        getDemandSupplyBalance(null);
    }


    /// <summary>
    /// per 1 unit
    /// </summary>
    //Value defaultPrice = new Value(2f);
    public void SetDefaultPrice(Product pro, float inprice)
    {
        marketPrice.Set(new Storage(pro, inprice));
    }/// <summary>
     /// returns how much was sold de-facto
     /// contains pre-seller code & pre Market storage code with long circle on all sellers
     /// </summary>   
    internal Storage BuyOld(Producer buyer, Storage buying)
    {
        float producedThisTurn = 0;
        float stillHaveOnStorage = 0f;
        float DSB = getDemandSupplyBalance(buying.getProduct());
        float BuyingAmountAvailable = 0;
        float consumedDeFacto = 0;
        Value actualBuyingAmountOnThisIteration;
        Storage price = findPrice(buying.getProduct());
        Value trade;
        if (DSB < 1f) DSB = 1f;
        BuyingAmountAvailable = buying.get() / DSB;
        foreach (Country country in Country.allCountries)//todo add RicherOrder Iterator/ own country
                                                         //if (country.isInvented(InventionType.capitalism))
            foreach (Province province in country.ownedProvinces)
            {
                ///////////////////
                //refactor//
                foreach (Producer seller in province)
                {
                    // don't buy from your self?
                    if (seller.storageNow.getProduct() == buying.getProduct())// && seller != buyer)
                    {
                        producedThisTurn = seller.gainGoodsThisTurn.get();
                        stillHaveOnStorage = seller.storageNow.get();
                        if (producedThisTurn > 0f || stillHaveOnStorage > 0f)
                        {
                            float supply = getSupply(buying.getProduct());
                            //if (supply ==0f)

                            actualBuyingAmountOnThisIteration = new Value(BuyingAmountAvailable * producedThisTurn / supply);
                            if (actualBuyingAmountOnThisIteration.get() > stillHaveOnStorage)
                                actualBuyingAmountOnThisIteration.set(stillHaveOnStorage);
                            if (actualBuyingAmountOnThisIteration.get() > 0f)
                            {
                                trade = actualBuyingAmountOnThisIteration.multiple(price);
                                if (buyer.wallet.canPay(trade))
                                    buyer.wallet.pay(seller.wallet, trade);
                                else
                                {
                                    //just fighting roundation errors
                                    //Debug.Log("Buying failed in payment");
                                    buyer.wallet.pay(seller.wallet, buyer.wallet.haveMoney);
                                }
                                if (seller.storageNow.canPay(actualBuyingAmountOnThisIteration))
                                {
                                    seller.storageNow.subtract(actualBuyingAmountOnThisIteration);
                                    consumedDeFacto += actualBuyingAmountOnThisIteration.get();
                                }
                                else
                                {
                                    //just fighting roundation errors
                                    //Debug.Log("Buying failed in storage transaction");
                                    // Should never be fired in new DSB
                                    seller.storageNow.set(0);
                                }
                            }

                        }
                    }
                }
                ///////////////////
                //foreach (Factory seller in province.allFactories)
                //    {
                //        // don't buy from your self?
                //        if (seller.storageNow.getProduct() == buying.getProduct())// && seller != buyer)
                //        {
                //            producedThisTurn = seller.gainGoodsThisTurn.get();
                //            stillHaveOnStorage = seller.storageNow.get();
                //            if (producedThisTurn > 0f && stillHaveOnStorage > 0f)
                //            {
                //                float supply = getSupply(buying.getProduct());
                //                //if (supply ==0f)

                //                actualBuyingAmountOnThisIteration = new Value(BuyingAmountAvailable * producedThisTurn / supply);
                //                if (actualBuyingAmountOnThisIteration.get() > stillHaveOnStorage)
                //                    actualBuyingAmountOnThisIteration.set(stillHaveOnStorage);
                //                if (actualBuyingAmountOnThisIteration.get() > 0f)
                //                {
                //                    trade = actualBuyingAmountOnThisIteration.multiple(price);
                //                    if (buyer.wallet.canPay(trade))
                //                        buyer.wallet.pay(seller.wallet, trade);
                //                    else
                //                    {
                //                        //just fighting roundation errors
                //                        //Debug.Log("Buying failed in payment");
                //                        buyer.wallet.pay(seller.wallet, buyer.wallet.haveMoney);
                //                    }
                //                    if (seller.storageNow.canPay(actualBuyingAmountOnThisIteration))
                //                    {
                //                        seller.storageNow.subtract(actualBuyingAmountOnThisIteration);
                //                        consumedDeFacto += actualBuyingAmountOnThisIteration.get();
                //                    }
                //                    else
                //                    {
                //                        //just fighting roundation errors
                //                        //Debug.Log("Buying failed in storage transaction");
                //                        // Should never be fired in new DSB
                //                        seller.storageNow.set(0);
                //                    }
                //                }

                //            }
                //        }
                //    }

                //    // todo isServiceProvider() and as iterator
                //    foreach (PopUnit seller in province.allPopUnits)
                //    {
                //        // don't buy from your self?
                //        if (seller.storageNow.getProduct() == buying.getProduct())// && seller != buyer)
                //        {
                //            producedThisTurn = seller.gainGoodsThisTurn.get();
                //            stillHaveOnStorage = seller.storageNow.get();
                //            //seller.ge
                //            if (producedThisTurn > 0f && stillHaveOnStorage > 0f)
                //            {
                //                //todo add here checks for zero
                //                actualBuyingAmountOnThisIteration = new Value(BuyingAmountAvailable * producedThisTurn / getSupply(buying.getProduct()));
                //                if (actualBuyingAmountOnThisIteration.get() > stillHaveOnStorage)
                //                    actualBuyingAmountOnThisIteration.set(stillHaveOnStorage);
                //                if (actualBuyingAmountOnThisIteration.get() > 0f)
                //                {
                //                    trade = actualBuyingAmountOnThisIteration.multiple(price);
                //                    if (buyer.wallet.canPay(trade))
                //                        buyer.wallet.pay(seller.wallet, trade);
                //                    else
                //                    {
                //                        //just fighting roundation errors
                //                        //Debug.Log("Buying failed in payment");
                //                        buyer.wallet.pay(seller.wallet, buyer.wallet.haveMoney);
                //                    }
                //                    if (seller.storageNow.canPay(actualBuyingAmountOnThisIteration))
                //                    {
                //                        seller.storageNow.subtract(actualBuyingAmountOnThisIteration);
                //                        consumedDeFacto += actualBuyingAmountOnThisIteration.get();
                //                    }
                //                    else
                //                    {
                //                        //just fighting roundation errors
                //                        //Debug.Log("Buying failed in storage transaction");
                //                        // Should never be fired in new DSB
                //                        seller.storageNow.set(0);
                //                    }
                //                }
                //            }
                //        }
                //    }
                // todo add same for country and any seller
            }

        //return new Storage(buying.getProduct(), BuyingAmountAvailable);
        return new Storage(buying.getProduct(), consumedDeFacto);
    }
    /// <summary>
    /// returns how much was sold de-facto
    /// new version of byy-old
    /// </summary>   
    internal Storage Buy(Producer buyer, Storage buying)
    {
        //TODO store markert goods in here or in producers??
        // IN Producers? Orrr, I can collect all production, sell it, and return money basing on sold / un sold relation
        //Storage storage = findStorage(what.getProduct());
        // float producedThisTurn = 0;
        //float stillHaveOnStorage = 0f;
        //float DSB = getDemandSupplyBalance(buying.getProduct());
        //Storage BuyingAmountAvailable;
        Storage howMuchCanConsume;
        //Value actualBuyingAmountOnThisIteration;
        Storage price = findPrice(buying.getProduct());
        Value cost;
        // not using DSB anyway
        //if (DSB < 1f) DSB = 1f;
        // BuyingAmountAvailable = buying.get() / DSB;
        // todo reduce can afford barnhes

        //cost = Game.market.getCost(buying);
        if (Game.market.tmpMarketStorage.has(buying))
        {
            cost = buying.multiple(price);
            if (buyer.wallet.canPay(cost))
            {
                buyer.wallet.pay(Game.market.wallet, cost);
                Game.market.tmpMarketStorage.subtract(buying);
                howMuchCanConsume = buying;
            }
            else
            {
                float val = buyer.wallet.haveMoney.get() / price.get();
                val = Mathf.Floor(val * Value.precision) / Value.precision;
                howMuchCanConsume = new Storage(price.getProduct(), val);
                buyer.wallet.pay(Game.market.wallet, howMuchCanConsume.multiple(price));
                Game.market.tmpMarketStorage.subtract(howMuchCanConsume);
            }
        }
        else
        {
            // assuming available < buying
            
            Storage available = Game.market.HowMuchAvailable(buying);
            if (available.get() > 0f)
            {
                cost = available.multiple(price);
                
                if (buyer.wallet.canPay(cost))
                {
                    buyer.wallet.pay(Game.market.wallet, cost);
                    Game.market.tmpMarketStorage.subtract(available);
                    howMuchCanConsume = available;
                }
                else
                {                   
                    howMuchCanConsume = new Storage(price.getProduct(), buyer.wallet.haveMoney.get() / price.get());
                    if (howMuchCanConsume.get() > available.get())
                        howMuchCanConsume.set(available.get()); // you don't buy more than there is
                    buyer.wallet.pay(Game.market.wallet, buyer.wallet.haveMoney); //pay all money couse you don't have more
                    Game.market.tmpMarketStorage.subtract(howMuchCanConsume);
                }               
            }
            else
                  howMuchCanConsume = new Storage(buying.getProduct(), 0f);
        }
        return howMuchCanConsume;


    }
    /// <summary>
    /// return procent of actual buying
    /// </summary>    
    public Storage Consume(Producer forWhom, Storage need)
    {
        float actuallyNeedsFullfilled = 0f;
        Storage actualConsumption;
        if (forWhom.wallet.CanAfford(need))
        {
            actualConsumption = Game.market.Buy(forWhom, need);
            // todo connect to Buy,                     
            forWhom.consumedTotal.add(actualConsumption);
            forWhom.consumedInMarket.add(actualConsumption);
            actuallyNeedsFullfilled = actualConsumption.get() / need.get();
            //NeedsFullfilled.set(actualConsumption.get() / need.get() / 3f);
        }
        else
        {
            Storage howMuchCanAfford = forWhom.wallet.HowMuchCanAfford(need);
            if (howMuchCanAfford.get() > 0f)
            {
                howMuchCanAfford = Game.market.Buy(forWhom, howMuchCanAfford);
                if (howMuchCanAfford.get() > 0f)
                {
                    forWhom.consumedTotal.add(howMuchCanAfford);
                    forWhom.consumedInMarket.add(howMuchCanAfford);
                    actuallyNeedsFullfilled = howMuchCanAfford.get() / need.get();
                }
            }
            //NeedsFullfilled.set(howMuchCanAfford.get() / need.get() / 3f);
        }
        return new Storage(need.getProduct(), actuallyNeedsFullfilled);
    }
    /// <summary>
    /// return true if buying is zero
    /// </summary>    
    internal bool Buy(Producer buyer, PrimitiveStorageSet buying, Procent buyInTime, PrimitiveStorageSet ofWhat)
    {
        bool buyingIsEmpty = true;
        foreach (Storage what in ofWhat)
        {
            Storage consumeOnThisEteration = new Storage(what.getProduct(), what.get() * buyInTime.get());
            if (consumeOnThisEteration.get() == 0)
                return true;
            // check if buying still have enoth to subtract consumeOnThisEteration
            if (!buying.has(consumeOnThisEteration))
                consumeOnThisEteration = buying.findStorage(what.getProduct());
            consumeOnThisEteration.multipleInside(Consume(buyer, consumeOnThisEteration));

            buying.subtract(consumeOnThisEteration);

            if (buying.findStorage(what.getProduct()).get() > 0)
                buyingIsEmpty = false;
        }
        return buyingIsEmpty;
        //    foreach (Storage input in buying)
        //{
        //    Storage stor = new Storage(input.getProduct(), input.get() * buyInTime.get());

        //    stor.multipleInside(Consume(buyer, stor));
        //    buying.subtract(stor);

        //}
    }
    internal void Buy(Producer buyer, PrimitiveStorageSet buying)
    {
        // Storage actualConsumption;
        foreach (Storage input in buying)
        {
            Consume(buyer, input);

            //if (Game.market.getSupply(input.getProduct()) > 0f)
            //    if (buyer.wallet.CanAfford(input))
            //    {
            //        actualConsumption = Game.market.Buy(buyer, input);
            //        // todo connect to Buy,                     
            //        buyer.consumedTotal.Add(actualConsumption);
            //        //input.subtract(actualConsumption);
            //        //actuallyNeedsFullfilled = actualConsumption.get() / input.get();                       
            //    }
            //    else
            //    {
            //        //Debug.Log("This brancge in POp.consime was not awaited...");
            //        //No, actually, its okey
            //        Storage howMuchCanAfford = buyer.wallet.HowMuchCanAfford(input);
            //        howMuchCanAfford = Game.market.Buy(buyer, howMuchCanAfford);
            //        buyer.consumedTotal.Add(howMuchCanAfford);
            //        //input.subtract(howMuchCanAfford);
            //        //actuallyNeedsFullfilled = howMuchCanAfford.get() / input.get();                       
            //    }
            // return actuallyNeedsFullfilled;
        }
    }
    /// <summary>
    /// Date actual for how much produced on turn start, not how much left
    /// </summary>
    /// <param name="need"></param>
    /// <returns></returns>
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
        //todo fucker - here DSB is based not on last turn data, but on this turn. Fix it!
        return new Storage(need, getSupply(need));
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
        Storage result = tmpMarketStorage.findStorage(need.getProduct());
        if (result == null)
            return new Storage(need.getProduct(), 0f);
        else
            return new Storage (result);

        //BuyingAmountAvailable = need.get() / DSB;
        ////todo PUT in BUY?? for anti-mirrorring
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
    /// Result > 1 mean demand is hier, price should go up   Result fiwer 1 mean supply is hier, price should go down
    /// based on last turn data    
    internal float getDemandSupplyBalance(Product pro)
    {
        float balance;
        if (dateOfDSB != Game.date)
        // recalculate DSBbuffer
        {

            foreach (Storage stor in marketPrice)
            {
                //todo zero division\
                getProductionTotal(pro); // for pre-turn initialization
                getTotalConsumption(pro);// for pre-turn initialization
                float supply = getSupply(stor.getProduct());
                float demand = getBouth(stor.getProduct());
                //if (demand == 0) getTotalConsumption(stor.getProduct());
                //else
                ////if (demand == 0) balance = 1f;
                ////else
                //if (supply == 0) balance = 2f;
                //else

                balance = demand / supply;

                //if (balance > 1f) balance = 1f;
                //&& supply == 0
                if (demand == 0) balance = 0f; // overwise - furniture bag
                                               // else
                if (supply == 0) balance = float.MaxValue;
                //if (float.IsInfinity(balance)) // if divided by zero, s = zero
                //    balance = float.NaN;
                DSBbuffer.Set(new Storage(stor.getProduct(), balance));
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
    /// Result > 1 mean demand is hier, price should go up   Result fiwer 1 mean supply is hier, price should go down
    /// based on last turn data    
    internal float getDemandSupplyBalanceOldNOLder(Product pro)
    {
        float balance;
        if (dateOfDSB != Game.date)
        // recalculate DSBbuffer
        {

            foreach (Storage stor in marketPrice)
            {
                //todo zero division
                float supply = getSupply(stor.getProduct());
                float demand = getBouth(stor.getProduct());
                //if (demand == 0) getTotalConsumption(stor.getProduct());
                //else
                ////if (demand == 0) balance = 1f;
                ////else
                //if (supply == 0) balance = 2f;
                //else

                balance = demand / supply;

                //if (balance > 1f) balance = 1f;
                //&& supply == 0
                if (demand == 0) balance = 0f; // overwise - furniture bag
                                               // else
                if (supply == 0) balance = float.MaxValue;
                //if (float.IsInfinity(balance)) // if divided by zero, s = zero
                //    balance = float.NaN;
                DSBbuffer.Set(new Storage(stor.getProduct(), balance));
            }
            dateOfDSB = Game.date;
        }
        Storage tmp = DSBbuffer.findStorage(pro);

        if (tmp == null)
            return float.NaN;
        else
            return tmp.get();
    }
    //todo initialize product list?? - its done, actually
    /// <summary>
    /// Changes price for every product in market
    /// That's firts call for DSB in tick
    /// </summary>
    public void simulatePriceChangeBasingOnLastTurnDate()
    {
        float balance;
        float priceChangeSpeed = 0;
        float highestChangingSpeed = 0.2f; //%
        float highChangingSpeed = 0.04f;//%
        float antiBalance;
        foreach (Storage price in this.marketPrice)
            if (price.getProduct() != Product.findByName("Gold"))
            {
                balance = getDemandSupplyBalance(price.getProduct());
                /// Result > 1 mean demand is hier, price should go up  
                /// Result fiwer 1 mean supply is hier, price should go down               
                //if (getSupply(price.getProduct()) == 0)
                //    balance = 1;
                //if (balance < 1f) antiBalance = 1 / balance;
                //else antiBalance = balance;
                priceChangeSpeed = 0;
                if (balance == 1f) priceChangeSpeed = 0.001f + price.get() * 0.1f;
                else
                    //if (balance > 1f && getSupply(price.getProduct()) == 0f) priceChangeSpeed = 0;
                    // else
                    //(0.0001f <= balance &&
                    if (balance <= 0.5f)
                    priceChangeSpeed = -0.001f + price.get() * -0.02f;
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
        if (newValue <= 0) newValue = Game.minPrice;
        if (newValue >= Game.maxPrice)
        {
            newValue = Game.maxPrice;
            if (getBouth(price.getProduct()) != 0) newValue = Game.maxPrice / 20f;
        }
        price.set(newValue);
        priceHistory.addData(price.getProduct(), price);
    }

}