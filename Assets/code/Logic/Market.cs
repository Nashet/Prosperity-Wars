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
    DateTime dateOfDSB = new DateTime(int.MaxValue);
    PrimitiveStorageSet DSBbuffer = new PrimitiveStorageSet();
    DateTime dateOfgetSupply = new DateTime(int.MaxValue);
    DateTime dateOfgetProductionTotal = new DateTime(int.MaxValue);
    DateTime dateOfgetTotalConsumption = new DateTime(int.MaxValue);
    DateTime dateOfgetBouth = new DateTime(int.MaxValue);
    PrimitiveStorageSet getSupplyBuffer = new PrimitiveStorageSet();
    PrimitiveStorageSet getProductionTotalBuffer = new PrimitiveStorageSet();
    PrimitiveStorageSet getTotalConsumptionBuffer = new PrimitiveStorageSet();
    PrimitiveStorageSet getBouthBuffer = new PrimitiveStorageSet();
    internal PricePool priceHistory;
    internal PrimitiveStorageSet sentToMarket = new PrimitiveStorageSet();
    public Market() : base(0f, null)
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
        float cost = 0;
        float price;

        price = Game.market.findPrice(need.getProduct()).get();
        cost = need.get() * price;

        return new Value(cost);
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
        if (dateOfgetBouth != Game.date)
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
                getBouthBuffer.set(new Storage(sup.getProduct(), result));
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
                getTotalConsumptionBuffer.set(new Storage(sup.getProduct(), result));
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
        foreach (Country country in Country.getExisting())
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
        if (dateOfgetSupply != Game.date)
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

                getSupplyBuffer.set(new Storage(sup.getProduct(), result));
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
        if (dateOfgetProductionTotal != Game.date)
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
                getProductionTotalBuffer.set(new Storage(sup.getProduct(), result));
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
    }/// <summary>
     /// returns how much was sold de-facto
     /// contains pre-seller code & pre Market storage code with long circle on all sellers
     /// </summary>   
    //internal Storage BuyOld(Producer buyer, Storage buying)
    //{
    //    float producedThisTurn = 0;
    //    float stillHaveOnStorage = 0f;
    //    float DSB = getDemandSupplyBalance(buying.getProduct());
    //    float BuyingAmountAvailable = 0;
    //    float consumedDeFacto = 0;
    //    Value actualBuyingAmountOnThisIteration;
    //    Storage price = findPrice(buying.getProduct());
    //    Value trade;
    //    if (DSB < 1f) DSB = 1f;
    //    BuyingAmountAvailable = buying.get() / DSB;
    //    foreach (Country country in Country.allCountries)//todo add RicherOrder Iterator/ own country
    //                                                     //if (country.isInvented(InventionType.capitalism))
    //        foreach (Province province in country.ownedProvinces)
    //        {
    //            ///////////////////
    //            //refactor//
    //            foreach (Producer seller in province)
    //            {
    //                // don't buy from your self?
    //                if (seller.storageNow.getProduct() == buying.getProduct())// && seller != buyer)
    //                {
    //                    producedThisTurn = seller.gainGoodsThisTurn.get();
    //                    stillHaveOnStorage = seller.storageNow.get();
    //                    if (producedThisTurn > 0f || stillHaveOnStorage > 0f)
    //                    {
    //                        float supply = getSupply(buying.getProduct());
    //                        //if (supply ==0f)

    //                        actualBuyingAmountOnThisIteration = new Value(BuyingAmountAvailable * producedThisTurn / supply);
    //                        if (actualBuyingAmountOnThisIteration.get() > stillHaveOnStorage)
    //                            actualBuyingAmountOnThisIteration.set(stillHaveOnStorage);
    //                        if (actualBuyingAmountOnThisIteration.get() > 0f)
    //                        {
    //                            trade = actualBuyingAmountOnThisIteration.multiple(price);
    //                            if (buyer.wallet.canPay(trade))
    //                                buyer.wallet.pay(seller.wallet, trade);
    //                            else
    //                            {
    //                                //just fighting roundation errors
    //                                //Debug.Log("Buying failed in payment");
    //                                buyer.wallet.pay(seller.wallet, buyer.wallet.haveMoney);
    //                            }
    //                            if (seller.storageNow.canPay(actualBuyingAmountOnThisIteration))
    //                            {
    //                                seller.storageNow.subtract(actualBuyingAmountOnThisIteration);
    //                                consumedDeFacto += actualBuyingAmountOnThisIteration.get();
    //                            }
    //                            else
    //                            {
    //                                //just fighting roundation errors
    //                                //Debug.Log("Buying failed in storage transaction");
    //                                // Should never be fired in new DSB
    //                                seller.storageNow.set(0);
    //                            }
    //                        }

    //                    }
    //                }
    //            }
    //            ///////////////////
    //            //foreach (Factory seller in province.allFactories)
    //            //    {
    //            //        // don't buy from your self?
    //            //        if (seller.storageNow.getProduct() == buying.getProduct())// && seller != buyer)
    //            //        {
    //            //            producedThisTurn = seller.gainGoodsThisTurn.get();
    //            //            stillHaveOnStorage = seller.storageNow.get();
    //            //            if (producedThisTurn > 0f && stillHaveOnStorage > 0f)
    //            //            {
    //            //                float supply = getSupply(buying.getProduct());
    //            //                //if (supply ==0f)

    //            //                actualBuyingAmountOnThisIteration = new Value(BuyingAmountAvailable * producedThisTurn / supply);
    //            //                if (actualBuyingAmountOnThisIteration.get() > stillHaveOnStorage)
    //            //                    actualBuyingAmountOnThisIteration.set(stillHaveOnStorage);
    //            //                if (actualBuyingAmountOnThisIteration.get() > 0f)
    //            //                {
    //            //                    trade = actualBuyingAmountOnThisIteration.multiple(price);
    //            //                    if (buyer.wallet.canPay(trade))
    //            //                        buyer.wallet.pay(seller.wallet, trade);
    //            //                    else
    //            //                    {
    //            //                        //just fighting roundation errors
    //            //                        //Debug.Log("Buying failed in payment");
    //            //                        buyer.wallet.pay(seller.wallet, buyer.wallet.haveMoney);
    //            //                    }
    //            //                    if (seller.storageNow.canPay(actualBuyingAmountOnThisIteration))
    //            //                    {
    //            //                        seller.storageNow.subtract(actualBuyingAmountOnThisIteration);
    //            //                        consumedDeFacto += actualBuyingAmountOnThisIteration.get();
    //            //                    }
    //            //                    else
    //            //                    {
    //            //                        //just fighting roundation errors
    //            //                        //Debug.Log("Buying failed in storage transaction");
    //            //                        // Should never be fired in new DSB
    //            //                        seller.storageNow.set(0);
    //            //                    }
    //            //                }

    //            //            }
    //            //        }
    //            //    }

    //            //    // todo isServiceProvider() and as iterator
    //            //    foreach (PopUnit seller in province.allPopUnits)
    //            //    {
    //            //        // don't buy from your self?
    //            //        if (seller.storageNow.getProduct() == buying.getProduct())// && seller != buyer)
    //            //        {
    //            //            producedThisTurn = seller.gainGoodsThisTurn.get();
    //            //            stillHaveOnStorage = seller.storageNow.get();
    //            //            //seller.ge
    //            //            if (producedThisTurn > 0f && stillHaveOnStorage > 0f)
    //            //            {
    //            //                //todo add here checks for zero
    //            //                actualBuyingAmountOnThisIteration = new Value(BuyingAmountAvailable * producedThisTurn / getSupply(buying.getProduct()));
    //            //                if (actualBuyingAmountOnThisIteration.get() > stillHaveOnStorage)
    //            //                    actualBuyingAmountOnThisIteration.set(stillHaveOnStorage);
    //            //                if (actualBuyingAmountOnThisIteration.get() > 0f)
    //            //                {
    //            //                    trade = actualBuyingAmountOnThisIteration.multiple(price);
    //            //                    if (buyer.wallet.canPay(trade))
    //            //                        buyer.wallet.pay(seller.wallet, trade);
    //            //                    else
    //            //                    {
    //            //                        //just fighting roundation errors
    //            //                        //Debug.Log("Buying failed in payment");
    //            //                        buyer.wallet.pay(seller.wallet, buyer.wallet.haveMoney);
    //            //                    }
    //            //                    if (seller.storageNow.canPay(actualBuyingAmountOnThisIteration))
    //            //                    {
    //            //                        seller.storageNow.subtract(actualBuyingAmountOnThisIteration);
    //            //                        consumedDeFacto += actualBuyingAmountOnThisIteration.get();
    //            //                    }
    //            //                    else
    //            //                    {
    //            //                        //just fighting roundation errors
    //            //                        //Debug.Log("Buying failed in storage transaction");
    //            //                        // Should never be fired in new DSB
    //            //                        seller.storageNow.set(0);
    //            //                    }
    //            //                }
    //            //            }
    //            //        }
    //            //    }
    //            // todo add same for country and any seller
    //        }

    //    //return new Storage(buying.getProduct(), BuyingAmountAvailable);
    //    return new Storage(buying.getProduct(), consumedDeFacto);
    //}
    /// <summary>
    /// returns how much was sold de-facto
    /// new version of byy-old
    /// </summary>   
    internal Storage buy(Consumer buyer, Storage buying)
    {

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
        // todo reduce can afford circles

        //cost = Game.market.getCost(buying);
        if (Game.market.sentToMarket.has(buying))
        {
            cost = buying.multipleOutside(price);
            if (buyer.canPay(cost))
            {
                buyer.pay(Game.market, cost);
                Game.market.sentToMarket.subtract(buying);
                if (buyer is Factory)
                    (buyer as Factory).inputReservs.add(buying);
                howMuchCanConsume = buying;
            }
            else
            {
                float val = buyer.cash.get() / price.get();
                val = Mathf.Floor(val * Value.precision) / Value.precision;
                howMuchCanConsume = new Storage(price.getProduct(), val);
                buyer.pay(Game.market, howMuchCanConsume.multipleOutside(price));
                Game.market.sentToMarket.subtract(howMuchCanConsume);
                if (buyer is Factory)
                    (buyer as Factory).inputReservs.add(howMuchCanConsume);
            }
        }
        else
        {
            // assuming available < buying

            Storage available = Game.market.HowMuchAvailable(buying);
            if (available.get() > 0f)
            {
                cost = available.multipleOutside(price);

                if (buyer.canPay(cost))
                {
                    buyer.pay(Game.market, cost);
                    Game.market.sentToMarket.subtract(available);
                    if (buyer is Factory)
                        (buyer as Factory).inputReservs.add(available);
                    howMuchCanConsume = available;
                }
                else
                {
                    howMuchCanConsume = new Storage(price.getProduct(), buyer.cash.get() / price.get());
                    if (howMuchCanConsume.get() > available.get())
                        howMuchCanConsume.set(available.get()); // you don't buy more than there is
                    buyer.sendAllAvailableMoney(Game.market); //pay all money cause you don't have more
                    Game.market.sentToMarket.subtract(howMuchCanConsume);
                    if (buyer is Factory)
                        (buyer as Factory).inputReservs.add(howMuchCanConsume);
                }
            }
            else
                howMuchCanConsume = new Storage(buying.getProduct(), 0f);
        }
        return howMuchCanConsume;


    }

    float DoPartialBuying(Consumer forWhom, Storage need)
    {
        if (need.get() > 0f)
        {
            Storage howMuchCanAfford = forWhom.HowMuchCanAfford(need);
            if (howMuchCanAfford.get() > 0f)
            {
                howMuchCanAfford = Game.market.buy(forWhom, howMuchCanAfford);
                if (howMuchCanAfford.get() > 0f)
                {
                    forWhom.consumedTotal.add(howMuchCanAfford);
                    forWhom.consumedInMarket.add(howMuchCanAfford);
                    //actuallyNeedsFullfilled = howMuchCanAfford.get() / need.get();
                    return howMuchCanAfford.get() / need.get();
                }
            }
            return 0f;
        }
        else return 0f;
    }
    float DoFullBuying(Consumer forWhom, Storage need)
    {
        if (need.get() > 0f)
        {
            var actualConsumption = Game.market.buy(forWhom, need);
            // todo connect to Buy,                     
            forWhom.consumedTotal.add(actualConsumption);
            forWhom.consumedInMarket.add(actualConsumption);
            //actuallyNeedsFullfilled = actualConsumption.get() / need.get();
            return actualConsumption.get() / need.get();
        }
        else return 0f;
    }
    /// <summary>
    /// returns PROCENT actual buying
    /// </summary>    
    public Storage buy(Consumer forWhom, Storage need, Country subsidizer)
    {
        float actuallyNeedsFullfilled = 0f;
        //Storage actualConsumption;

        if (forWhom.CanAfford(need))
            actuallyNeedsFullfilled = DoFullBuying(forWhom, need);
        else
            if (subsidizer == null)
            actuallyNeedsFullfilled = DoPartialBuying(forWhom, need);
        else
        {
            subsidizer.takeFactorySubsidies(forWhom, forWhom.HowMuchMoneyCanNotPay(need));
            //repeat attempt
            if (forWhom.CanAfford(need))
                actuallyNeedsFullfilled = DoFullBuying(forWhom, need);
            else
                actuallyNeedsFullfilled = DoPartialBuying(forWhom, need);
        }

        return new Storage(need.getProduct(), actuallyNeedsFullfilled);
    }
    /// <summary>
    /// return true if buying is zero (bought all what it wanted)
    /// </summary>    
    internal bool buy(Producer buyer, PrimitiveStorageSet buying, Procent buyInTime, PrimitiveStorageSet ofWhat)
    {
        bool buyingIsEmpty = true;
        foreach (Storage what in ofWhat)
        {
            Storage consumeOnThisEteration = new Storage(what.getProduct(), what.get() * buyInTime.get());
            if (consumeOnThisEteration.get() == 0)
                return true;
            // check if buying still have enoth to subtract consumeOnThisEteration
            if (!buying.has(consumeOnThisEteration))
                consumeOnThisEteration = buying.getStorage(what.getProduct());
            consumeOnThisEteration.multiple(buy(buyer, consumeOnThisEteration, null));

            buying.subtract(consumeOnThisEteration);

            if (buying.getStorage(what.getProduct()).get() > 0)
                buyingIsEmpty = false;
        }
        return buyingIsEmpty;

    }
    internal void buy(Factory buyer, PrimitiveStorageSet buying, Country subsidizer)
    {
        // Storage actualConsumption;
        foreach (Storage input in buying)
        {
            buy(buyer, input, subsidizer);
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
                if (balance == 1f) priceChangeSpeed = 0.001f + price.get() * 0.1f;
                else
                {
                    //if (balance > 1f && getSupply(price.getProduct()) == 0f) priceChangeSpeed = 0;
                    // else
                    //(0.0001f <= balance &&
                    if (balance <= 0.75f)
                        priceChangeSpeed = -0.001f + price.get() * -0.02f;
                    else
                    {
                        if (balance > 1f)
                            ChangePrice(price, price.getProduct().getDefaultPrice().get() - price.get());
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