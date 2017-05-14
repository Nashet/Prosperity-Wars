using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignPattern.Objectpool;
/// <summary>
///     Clears the contents of the string builder.
/// </summary>
/// <param name="value">
///     The <see cref="StringBuilder"/> to clear.
/// </param>

abstract public class PopUnit : Producer
{


    ///<summary>buffer popList of demoted. To avoid iteration breaks</summary>
    public static List<PopUnit> PopListToAddToGeneralList = new List<PopUnit>();

    public Procent loyalty;
    int population;
    int mobilized;
    public PopType type;
    public Culture culture;
    public Procent education;
    public Procent needsFullfilled;

    private int daysUpsetByForcedReform;
    private bool didntGetPromisedUnemloymentSubsidy;

    public ModifiersList modifiersLoyaltyChange;

    Modifier modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierEverydayNeedsFulfilled, modifierLifeNeedsFulfilled,
        modifierStarvation, modifierUpsetByForcedReform, modifierLifeNeedsNotFulfilled, modifierNotGivenUnemploymentSubsidies;

    public Value incomeTaxPayed = new Value(0);

    private int born;
    //if add new fields make sure it's implemented in second constructor and in merge()   


    public PopUnit(int iamount, PopType ipopType, Culture iculture, Province where)
    {
        born = Game.date;
        population = iamount;
        type = ipopType;
        culture = iculture;

        storageNow = new Storage(Product.findByName("Food"), 0);
        gainGoodsThisTurn = new Storage(Product.findByName("Food"), 0);
        sentToMarket = new Storage(Product.findByName("Food"), 0);
        education = new Procent(0.00f);
        loyalty = new Procent(0.50f);
        needsFullfilled = new Procent(0.50f);
        province = where;
        makeModifiers();
    }
    /// <summary> Creates new PopUnit basing on part of other PopUnit.
    /// And transfers sizeOfNewPop population.
    /// </summary>    
    public PopUnit(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture)// : this(source.getPopulation(), source.type, source.culture, source.province)
    {
        born = Game.date;
        PopListToAddToGeneralList.Add(this);
        makeModifiers();

        // here should be careful copying of popUnit data
        //And careful editing of old unit
        Procent newPopShare = Procent.makeProcent(sizeOfNewPop, source.getPopulation());

        //Own PopUnit fields:
        loyalty = new Procent(source.loyalty.get());
        population = sizeOfNewPop;
        if (source.population - sizeOfNewPop <= 0 && this.type == PopType.aristocrats || this.type == PopType.capitalists)
            // if source pop is gonna be dead..
            //secede property... to new pop.. what is new pop in migration - OK; or changed type - fixed.
            //todo - can optimize it, double run on List
            source.getOwnedFactories().ForEach(x => x.factoryOwner = this);
        source.subtractPopulation(sizeOfNewPop);
        mobilized = 0; ;
        type = newPopType;
        this.culture = culture;
        education = new Procent(source.education.get());
        needsFullfilled = new Procent(source.needsFullfilled.get());
        daysUpsetByForcedReform = 0;
        didntGetPromisedUnemloymentSubsidy = false;

        //Owner's fields:
        wallet = new Wallet(0f);
        source.wallet.pay(wallet, source.wallet.haveMoney.multiple(newPopShare));
        //wallet = newPopShare.sendProcentToNew(source.wallet.haveMoney);

        //Producer's fields:
        storageNow = newPopShare.sendProcentToNew(source.storageNow);
        gainGoodsThisTurn = new Storage(source.gainGoodsThisTurn.getProduct());
        sentToMarket = new Storage(source.sentToMarket.getProduct());

        ////loans = new Value(0);
        ////source.loans.pay(loans, source.loans.multiple(newPopShare));
        loans = newPopShare.sendProcentToNew(source.loans);

        consumedTotal = new PrimitiveStorageSet();
        consumedLastTurn = new PrimitiveStorageSet();
        consumedInMarket = new PrimitiveStorageSet();

        incomeTaxPayed = newPopShare.sendProcentToNew(source.incomeTaxPayed);

        province = where;//source.province;
    }
    /// <summary>
    /// Merging source into this pop
    /// assuming that both pops are in same province, and has same type
    /// culture defaults to this.culture
    /// </summary>    
    internal void mergeIn(PopUnit source)
    {
        //addPopulation(pop.getPopulation());
        //carefully summing 2 pops..                

        //Own PopUnit fields:
        loyalty.addPoportionally(this.getPopulation(), source.getPopulation(), source.loyalty);
        addPopulation(source.getPopulation());


        mobilized += source.mobilized;
        //type = newPopType; don't change that
        //culture = source.culture; don't change that
        education.addPoportionally(this.getPopulation(), source.getPopulation(), source.education);
        needsFullfilled.addPoportionally(this.getPopulation(), source.getPopulation(), source.needsFullfilled);
        //daysUpsetByForcedReform = 0; don't change that
        //didntGetPromisedUnemloymentSubsidy = false; don't change that

        //Owner's fields:        
        source.wallet.sendAll(this.wallet);

        //Producer's fields:
        storageNow.add(source.storageNow);
        gainGoodsThisTurn.add(source.gainGoodsThisTurn);
        sentToMarket.add(source.sentToMarket);

        loans.add(source.loans);

        consumedTotal.add(source.consumedTotal);
        consumedLastTurn.add(source.consumedLastTurn);
        consumedLastTurn.add(source.consumedLastTurn);

        //province = source.province; don't change that

        //if (source.population - sizeOfNewPop <= 0)// if source pop is gonna be dead..It gonna be, for sure
        //secede property... to new pop.. 
        //todo - can optimize it, double run on List. Also have point only in Consolidation, not for PopUnit.PopListToAddToGeneralList
        //that check in not really needed as it this pop supposed to be same type as source
        //if (this.type == PopType.aristocrats || this.type == PopType.capitalists)
            source.getOwnedFactories().ForEach(x => x.factoryOwner = this);

        // basically, killing that unit. Currently that object is linked in PopUnit.PopListToAddInGeneralList only so don't worry
        source.deleteData();
    }
    /// <summary>
    /// Sets population to zero as a mark to delete this Pop
    /// </summary>
    private void deleteData()
    {
        population = 0;
        //province.allPopUnits.Remove(this); // gives exception        
        //Game.popsToShowInPopulationPanel.Remove(this);
        if (MainCamera.popUnitPanel.whomShowing() == this)
            MainCamera.popUnitPanel.Hide();
        //remove from population panel.. Would do it automatically        
        //secede property... to government
        getOwnedFactories().ForEach(x => x.factoryOwner = province.getOwner());
    }
    public List<Factory> getOwnedFactories()
    {
        List<Factory> result = new List<Factory>();
        if (type == PopType.aristocrats || type == PopType.capitalists)
        {
            foreach (var item in province.allFactories)
                if (item.factoryOwner == this)
                    result.Add(item);
            return result;
        }
        else //return empty list
            return result;
    }
    public int getAge()
    {
        return Game.date - born;
    }
    private void makeModifiers()
    {
        modifierStarvation = new Modifier(delegate (Country forWhom) { return needsFullfilled.get() < 0.20f; }, "Starvation", false, -0.3f);
        modifierLifeNeedsNotFulfilled = new Modifier(delegate (Country forWhom) { return getLifeNeedsFullfilling().get() < 0.99f; }, "Life needs are not satisfied", false, -0.2f);
        modifierLifeNeedsFulfilled = new Modifier(delegate (Country forWhom) { return getLifeNeedsFullfilling().get() > 0.99f; }, "Life needs are satisfied", false, 0.1f);
        modifierEverydayNeedsFulfilled = new Modifier(delegate (Country forWhom) { return getEveryDayNeedsFullfilling().get() > 0.99f; }, "Everyday needs are satisfied", false, 0.15f);
        modifierLuxuryNeedsFulfilled = new Modifier(delegate (Country forWhom) { return getLuxuryNeedsFullfilling().get() > 0.99f; }, "Luxury needs are satisfied", false, 0.2f);

        //Game.threadDangerSB.Clear();
        //Game.threadDangerSB.Append("Likes that government because can vote with ").Append(this.province.getOwner().government.ToString());
        modifierCanVote = new Modifier(delegate (Country forWhom) { return canVote(); }, "Can vote with that government ", false, 0.1f);
        //Game.threadDangerSB.Clear();
        //Game.threadDangerSB.Append("Dislikes that government because can't vote with ").Append(this.province.getOwner().government.ToString());
        modifierCanNotVote = new Modifier(delegate (Country forWhom) { return !canVote(); }, "Can't vote with that government ", false, -0.1f);
        //Game.threadDangerSB.Clear();
        //Game.threadDangerSB.Append("Upset by forced reform - ").Append(daysUpsetByForcedReform).Append(" days");
        modifierUpsetByForcedReform = new Modifier(delegate (Country forWhom) { return daysUpsetByForcedReform > 0; }, "Upset by forced reform", false, -0.3f);
        modifierNotGivenUnemploymentSubsidies = new Modifier((Country x) => didntGetPromisedUnemloymentSubsidy, "Didn't got promised Unemployment Subsidies", false, -1.0f);
        modifiersLoyaltyChange = new ModifiersList(new List<Condition>()
        {
           modifierStarvation, modifierLifeNeedsNotFulfilled, modifierLifeNeedsFulfilled, modifierEverydayNeedsFulfilled, modifierLuxuryNeedsFulfilled,
            modifierCanVote, modifierCanNotVote, modifierUpsetByForcedReform, modifierNotGivenUnemploymentSubsidies
        });
    }
    public int getPopulation()
    { return population; }
    internal int howMuchCanMobilize()
    {

        int howMuchCanMobilize = (int)(getPopulation() * loyalty.get() * Options.mobilizationFactor);
        howMuchCanMobilize -= mobilized;
        if (howMuchCanMobilize < 0) howMuchCanMobilize = 0;

        return howMuchCanMobilize;
    }
    public Corps mobilize()
    {
        int amount = howMuchCanMobilize();

        if (amount > 0)
        {
            mobilized += amount;
            return Pool.GetObject(this, amount);
        }
        else
            return null;
    }
    public void demobilize()
    {
        mobilized = 0;

    }
    internal void takeLoss(int loss)
    {
        //int newPopulation = getPopulation() - (int)(loss * Options.PopAttritionFactor);

        //if (newPopulation > 0)
        this.subtractPopulation((int)(loss * Options.PopAttritionFactor));
        //else
        //pop totally killed
        ;
        mobilized -= loss;
        if (mobilized < 0) mobilized = 0;
    }
    internal void addDaysUpsetByForcedReform(int popDaysUpsetByForcedReform)
    {
        daysUpsetByForcedReform += popDaysUpsetByForcedReform;
    }



    //internal float getSayYesProcent(AbstractReformValue selectedReformValue)
    //{
    //    return (int)Mathf.RoundToInt(getSayingYes(selectedReformValue) / (float)population);
    //}
    /// <summary>
    /// Creates Pop in PopListToAddToGeneralList, later in will go to proper List
    /// </summary>    
    //public static PopUnit Instantiate(PopType type, PopUnit source, int sizeOfNewPop)
    //{
    //    if (type == PopType.tribeMen) return new Tribemen(source, sizeOfNewPop);
    //    else
    //    if (type == PopType.farmers) return new Farmers(source, sizeOfNewPop);
    //    else
    //    if (type == PopType.aristocrats) return new Aristocrats(source, sizeOfNewPop);
    //    else
    //    if (type == PopType.workers) return new Workers(source, sizeOfNewPop);
    //    else
    //        if (type == PopType.capitalists) return new Capitalists(source, sizeOfNewPop);
    //    else
    //    {
    //        Debug.Log("Unknown pop type!");
    //        return null;
    //    }
    //}
    /// <summary>
    /// Creates Pop in PopListToAddToGeneralList, later in will go to proper List
    /// </summary>    
    public static PopUnit Instantiate(PopType type, PopUnit source, int sizeOfNewPop, Province where, Culture culture)
    {
        if (type == PopType.tribeMen) return new Tribemen(source, sizeOfNewPop, where, culture);
        else
        if (type == PopType.farmers) return new Farmers(source, sizeOfNewPop, where, culture);
        else
        if (type == PopType.aristocrats) return new Aristocrats(source, sizeOfNewPop, where, culture);
        else
        if (type == PopType.workers) return new Workers(source, sizeOfNewPop, where, culture);
        else
            if (type == PopType.capitalists) return new Capitalists(source, sizeOfNewPop, where, culture);
        else
        {
            Debug.Log("Unknown pop type!");
            return null;
        }
    }
    //todo delete it
    //public static PopUnit Instantiate(int iamount, PopType ipopType, Culture iculture, Province where)
    //{

    //    if (ipopType == PopType.tribeMen) return new Tribemen(iamount, ipopType, iculture, where);
    //    else
    //    if (ipopType == PopType.farmers) return new Farmers(iamount, ipopType, iculture, where);
    //    else
    //    if (ipopType == PopType.aristocrats) return new Aristocrats(iamount, ipopType, iculture, where);
    //    else
    //    if (ipopType == PopType.workers) return new Workers(iamount, ipopType, iculture, where);
    //    else
    //        if (ipopType == PopType.capitalists) return new Capitalists(iamount, ipopType, iculture, where);
    //    else
    //    {
    //        Debug.Log("Unknown pop type!");
    //        return null;
    //    }
    //}
    abstract internal bool getSayingYes(AbstractReformValue reform);
    public static int getRandomPopulationAmount(int minGeneratedPopulation, int maxGeneratedPopulation)
    {
        int randomPopulation = minGeneratedPopulation + Game.random.Next(maxGeneratedPopulation - minGeneratedPopulation);
        return randomPopulation;
    }



    /// <summary> /// Return in pieces  /// </summary>    
    override internal float getLocalEffectiveDemand(Product product)
    {
        float result = 0;
        // need to know huw much i Consumed inside my needs
        PrimitiveStorageSet needs = new PrimitiveStorageSet(getRealLifeNeeds());
        Storage need = needs.findStorage(product);
        if (need != null)
        {
            Storage canAfford = wallet.HowMuchCanAfford(need);
            result += canAfford.get();
        }
        needs = new PrimitiveStorageSet(getRealEveryDayNeeds());
        need = needs.findStorage(product);
        if (need != null)
        {
            Storage canAfford = wallet.HowMuchCanAfford(need);
            result += canAfford.get();
        }
        needs = new PrimitiveStorageSet(getRealLuxuryNeeds());
        need = needs.findStorage(product);
        if (need != null)
        {
            Storage canAfford = wallet.HowMuchCanAfford(need);
            result += canAfford.get();
        }
        return result;
    }
    private List<Storage> getNeedsInCommon(List<Storage> needs)
    {
        Value multiplier = new Value(this.getPopulation() / 1000f);

        List<Storage> result = new List<Storage>();
        foreach (Storage next in needs)
            if (next.getProduct().isInventedByAnyOne())
            {
                Storage nStor = new Storage(next.getProduct(), next.get());
                nStor.multipleInside(multiplier);
                result.Add(nStor);
            }
        result.Sort(delegate (Storage x, Storage y)
        {
            float sumX = x.get() * Game.market.findPrice(x.getProduct()).get();
            float sumY = y.get() * Game.market.findPrice(y.getProduct()).get();
            return sumX.CompareTo(sumY);
        });
        return result;
    }

    public List<Storage> getRealLifeNeeds()
    {
        return getNeedsInCommon(this.type.getLifeNeedsPer1000());
    }

    public List<Storage> getRealEveryDayNeeds()
    {
        return getNeedsInCommon(this.type.getEveryDayNeedsPer1000());
    }

    public List<Storage> getRealLuxuryNeeds()
    {
        return getNeedsInCommon(this.type.getLuxuryNeedsPer1000());
    }

    internal Procent getUnemployedProcent()
    {
        if (type == PopType.workers)
        //return new Procent(0);
        {
            int employed = 0;
            foreach (Factory factory in province.allFactories)
                employed += factory.HowManyEmployed(this);
            if (getPopulation() - employed <= 0) //happening due population change by growth/demotion
                return new Procent(0);
            return new Procent((getPopulation() - employed) / (float)getPopulation());
        }
        else
            if (type == PopType.farmers || type == PopType.tribeMen)
        {
            float overPopulation = province.getOverpopulation();
            if (overPopulation <= 1f)
                return new Procent(0);
            else
                return new Procent(1f - (1f / overPopulation));
        }
        else return new Procent(0);
    }

    ////abstract public override void produce();
    ////{
    ////    float tribeMenOverPopulationFactor = 1f; //goes to zero with 20

    ////    switch (type.type)
    ////    {
    ////        case PopType.PopTypes.TribeMen:
    ////            Value producedAmount;
    ////            if (population <= province.maxTribeMenCapacity)
    ////                producedAmount = new Value(population * type.basicProduction.value.get() / 1000f);
    ////            else
    ////            {
    ////                int overPopulation = province.getMenPopulation() - province.maxTribeMenCapacity;
    ////                float over = (float)(overPopulation / (float)province.maxTribeMenCapacity);
    ////                producedAmount = new Value(population * type.basicProduction.value.get() / 1000f); //TODO fix shit

    ////                Value negation = new Value(producedAmount.get() * over / tribeMenOverPopulationFactor);
    ////                if (negation.get() > producedAmount.get()) producedAmount.set(0);
    ////                else
    ////                    producedAmount.subtract(negation);

    ////            }
    ////            storage.value.add(producedAmount);
    ////            produced.set(producedAmount);

    ////            break;
    ////        case PopType.PopTypes.Aristocrats:

    ////            break;
    ////        case PopType.PopTypes.Farmers:
    ////            producedAmount = new Value(population * type.basicProduction.value.get() / 1000);
    ////            storage.value.add(producedAmount);
    ////            produced.set(producedAmount);
    ////            break;
    ////        case PopType.PopTypes.Artisans:

    ////            break;
    ////        case PopType.PopTypes.Soldiers:

    ////            break;
    ////        default:
    ////            Debug.Log("Unnown PopType in Game.cs");
    ////            break;

    ////    }
    ////}
    internal bool hasToPayGovernmentTaxes()
    {
        if (this.type == PopType.aristocrats && Serfdom.IsNotAbolishedInAnyWay.checkIftrue((province.getOwner())))
            return false;
        else return true;
    }
    public override void payTaxes() // should be abstract 
    {
        Value taxSize = new Value(0);
        if (Economy.isMarket.checkIftrue(province.getOwner()) && type != PopType.tribeMen)
        {
            if (this.type.isPoorStrata())
            {
                taxSize = wallet.moneyIncomethisTurn.multiple((province.getOwner().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
                if (wallet.canPay(taxSize))
                {
                    incomeTaxPayed = taxSize;
                    province.getOwner().getCountryWallet().poorTaxIncomeAdd(taxSize);
                    wallet.pay(province.getOwner().wallet, taxSize);
                }
                else
                {
                    incomeTaxPayed.set(wallet.haveMoney);
                    province.getOwner().getCountryWallet().poorTaxIncomeAdd(wallet.haveMoney);
                    wallet.sendAll(province.getOwner().wallet);

                }
            }
            else
            if (this.type.isRichStrata())
            {
                taxSize = wallet.moneyIncomethisTurn.multiple((province.getOwner().taxationForRich.getValue() as TaxationForRich.ReformValue).tax);
                if (wallet.canPay(taxSize))
                {
                    province.getOwner().getCountryWallet().richTaxIncomeAdd(taxSize);
                    wallet.pay(province.getOwner().wallet, taxSize);
                }
                else
                {
                    province.getOwner().getCountryWallet().richTaxIncomeAdd(wallet.haveMoney);
                    wallet.sendAll(province.getOwner().wallet);
                }
            }

        }
        else// non market
        if (this.type != PopType.aristocrats)
        {
            // taxSize = gainGoodsThisTurn.multiple(province.getOwner().countryTax);

            if (this.type.isPoorStrata())
                taxSize = gainGoodsThisTurn.multiple((province.getOwner().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
            else
            if (this.type.isRichStrata())
                taxSize = gainGoodsThisTurn.multiple((province.getOwner().taxationForRich.getValue() as TaxationForPoor.ReformValue).tax);

            if (storageNow.canPay(taxSize))
                storageNow.pay(province.getOwner().storageSet, taxSize);
            else
                storageNow.sendAll(province.getOwner().storageSet);
        }
    }

    internal bool isStateCulture()
    {
        return this.culture == this.province.getOwner().culture;
    }

    public Procent getLifeNeedsFullfilling()
    {
        float need = needsFullfilled.get();
        if (need < 1f / 3f)
            return new Procent(needsFullfilled.get() * 3f);
        else
            return new Procent(1f);
    }
    public Procent getEveryDayNeedsFullfilling()
    {
        float need = needsFullfilled.get();
        if (need <= 1f / 3f)
            return new Procent(0f);
        if (need < 2f / 3f)
            return new Procent((needsFullfilled.get() - (1f / 3f)) * 3f);
        else
            return new Procent(1f);
    }

    public Procent getLuxuryNeedsFullfilling()
    {
        float need = needsFullfilled.get();
        if (need <= 2f / 3f)
            return new Procent(0f);
        if (need == 0.999f)
            return new Procent(1f);
        else
            return new Procent((needsFullfilled.get() - 0.666f) * 3f);

    }
    /// <summary>
    /// !!Recursion is here!!
    /// </summary>
    /// <param name="needs"></param>
    /// <param name="maxLevel"></param>
    /// <param name="howDeep"></param>
    private void consumeEveryDayAndLuxury(List<Storage> needs, float maxLevel, byte howDeep)
    {
        howDeep--;
        //List<Storage> needs = getEveryDayNeeds();
        foreach (Storage need in needs)
            if (storageNow.getProduct() == need.getProduct())
                if (storageNow.get() > need.get())
                {
                    storageNow.subtract(need);
                    consumedTotal.add(need);
                    needsFullfilled.set(2f / 3f);
                    if (howDeep != 0) consumeEveryDayAndLuxury(getRealLuxuryNeeds(), 0.99f, howDeep);
                }
                else
                {
                    float canConsume = storageNow.get();
                    consumedTotal.add(storageNow);
                    storageNow.set(0);
                    needsFullfilled.add(canConsume / need.get() / 3f);
                }
    }
    /// <summary> </summary>
    void subConsumeOnMarket(List<Storage> lifeNeeds, bool skipKifeneeds)
    {
        if (!skipKifeneeds)
            foreach (Storage need in lifeNeeds)
            {
                if (storageNow.canPay(need))// dont need to buy on market
                {
                    storageNow.subtract(need);
                    consumedTotal.Set(need);
                    //consumedInMarket.Set(need); are you crazy?
                    needsFullfilled.set(1f / 3f);
                    //consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 0.66f, 2);
                }
                else
                    needsFullfilled.set(Game.market.Consume(this, need, null).get() / 3f);
            }

        //if (NeedsFullfilled.get() > 0.33f) NeedsFullfilled.set(0.33f);

        if (getLifeNeedsFullfilling().get() >= 0.95f)
        {
            Wallet reserv = new Wallet(0);
            wallet.payWithoutRecord(reserv, wallet.haveMoney.multiple(Options.savePopMoneyReserv));
            lifeNeeds = (getRealEveryDayNeeds());
            Value needsCost = Game.market.getCost(lifeNeeds);
            float moneyWas = wallet.haveMoney.get();
            Value spentMoney;

            foreach (Storage need in lifeNeeds)
            {
                //NeedsFullfilled.set(0.33f + Game.market.Consume(this, need).get() / 3f);
                Game.market.Consume(this, need, null);
            }
            spentMoney = new Value(moneyWas - wallet.haveMoney.get());
            if (spentMoney.get() != 0f)
                needsFullfilled.add(spentMoney.get() / needsCost.get() / 3f);
            if (getEveryDayNeedsFullfilling().get() >= 0.95f)
            {
                lifeNeeds = (getRealLuxuryNeeds());
                needsCost = Game.market.getCost(lifeNeeds);
                moneyWas = wallet.haveMoney.get();
                foreach (Storage need in lifeNeeds)
                {
                    Game.market.Consume(this, need, null);
                    //NeedsFullfilled.set(0.66f + Game.market.Consume(this, need).get() / 3f);

                }
                spentMoney = new Value(moneyWas - wallet.haveMoney.get());
                if (spentMoney.get() != 0f)
                    needsFullfilled.add(spentMoney.get() / needsCost.get() / 3f);
            }
            reserv.payWithoutRecord(wallet, reserv.haveMoney);
        }
    }
    /// <summary> </summary>
    public override void consume()
    {
        //lifeneeds First
        List<Storage> needs = (getRealLifeNeeds());

        //if (province.getOwner().isInvented(InventionType.capitalism) && type != PopType.tribeMen)
        if (canTrade())
        {
            subConsumeOnMarket(needs, false);
        }
        else
        {//non - market consumption
            payTaxes(); // pops who can't trade always should pay taxes -  hasToPayGovernmentTaxes() is  excessive
            foreach (Storage need in needs)
                if (storageNow.get() > need.get())
                {
                    storageNow.subtract(need);
                    consumedTotal.Set(need);
                    needsFullfilled.set(1f / 3f);
                    consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 2f / 3f, 2);
                }
                else
                {
                    float canConsume = storageNow.get();
                    consumedTotal.Set(storageNow);
                    storageNow.set(0);
                    needsFullfilled.set(canConsume / need.get() / 3f);
                }
            if (type == PopType.aristocrats) // to allow trade without capitalism
                subConsumeOnMarket(needs, true);
        }
    }
    abstract internal bool canTrade();
    abstract internal bool canVote();
    public void calcLoyalty()
    {
        float newRes = loyalty.get() + modifiersLoyaltyChange.getModifier(this.province.getOwner()) / 100f;
        loyalty.set(Mathf.Clamp01(newRes));
        if (daysUpsetByForcedReform > 0)
            daysUpsetByForcedReform--;
    }

    public override void simulate()
    {

    }

    // Not called in capitalism
    public void PayTaxToAllAristocrats()
    {
        {
            Value taxSize = new Value(0);
            taxSize = gainGoodsThisTurn.multiple(province.getOwner().aristocrstTax);
            province.shareWithAllAristocrats(storageNow, taxSize);
        }
    }

    abstract public bool ShouldPayAristocratTax();


    public static void PrepareForNewTick()
    {
        Game.market.sentToMarket.SetZero();
        foreach (Country country in Country.allExisting)
        // if (country != Country.NullCountry)
        {
            //country.wallet.moneyIncomethisTurn.set(0);
            country.getCountryWallet().setSatisticToZero();
            country.aristocrstTax = country.serfdom.status.getTax();
            foreach (Province province in country.ownedProvinces)
            {
                province.BalanceEmployableWorkForce();
                {
                    foreach (PopUnit pop in province.allPopUnits)
                    {
                        pop.incomeTaxPayed.set(0); // need it because pop could stop paying taxes due to reforms for example
                        pop.gainGoodsThisTurn.set(0f);
                        // pop.storageNow.set(0f);
                        pop.wallet.moneyIncomethisTurn.set(0f);

                        pop.consumedLastTurn.copyDataFrom(pop.consumedTotal); // temp
                        pop.needsFullfilled.set(0f);
                        pop.sentToMarket.set(0f);
                        pop.consumedTotal.SetZero();
                        pop.consumedInMarket.SetZero();

                        pop.didntGetPromisedUnemloymentSubsidy = false;
                    }
                    foreach (Factory factory in province.allFactories)
                    {
                        factory.gainGoodsThisTurn.set(0f);
                        factory.storageNow.set(0f);
                        factory.wallet.moneyIncomethisTurn.set(0f);

                        factory.consumedLastTurn.copyDataFrom(factory.consumedTotal);
                        factory.sentToMarket.set(0f);
                        factory.consumedTotal.SetZero();
                        factory.consumedInMarket.SetZero();
                    }
                }
            }
        }
    }
    public void calcPromotions()
    {

    }

    //private bool CanDemote()
    //{
    //    if (popType == PopType.aristocrats)
    //        return true;
    //    else
    //        if (popType == PopType.tribeMen && countryOwner.farming.Invented())
    //        return true;
    //    return false;
    //}
    //public void Growth(int size)
    //{

    //}


    private void setPopulation(int newPopulation)
    {
        if (newPopulation > 0)
            population = newPopulation;
        else
            this.deleteData();
        //throw new NotImplementedException();
        //because pool aren't implemented yet
        //Pool.ReleaseObject(this);
    }
    private void subtractPopulation(int subtract)
    {
        setPopulation(getPopulation() - subtract);
        //population -= subtract; ;
    }
    private void addPopulation(int adding)
    {
        population += adding;
    }
    internal void takeUnemploymentSubsidies()
    {
        var reform = province.getOwner().unemploymentSubsidies.getValue();
        if (getUnemployedProcent().get() > 0 && reform != UnemploymentSubsidies.None)
        {
            Value subsidy = getUnemployedProcent();
            subsidy.multipleInside(getPopulation() / 1000f * (reform as UnemploymentSubsidies.ReformValue).getSubsidiesRate());
            //float subsidy = population / 1000f * getUnemployedProcent().get() * (reform as UnemploymentSubsidies.LocalReformValue).getSubsidiesRate();
            if (province.getOwner().wallet.canPay(subsidy))
            {
                province.getOwner().wallet.pay(this.wallet, subsidy);
                province.getOwner().getCountryWallet().unemploymentSubsidiesExpenseAdd(subsidy);
            }
            else
                this.didntGetPromisedUnemloymentSubsidy = true;

        }

    }
    public void calcGrowth()
    {
        addPopulation(getGrowthSize());
    }
    public int getGrowthSize()
    {
        int result = 0;
        if (this.needsFullfilled.get() >= 0.33f) // positive growth
            result = Mathf.RoundToInt(Options.PopGrowthSpeed.get() * getPopulation());
        else
            if (this.needsFullfilled.get() >= 0.20f) // zero growth
            result = 0;
        else if (type != PopType.farmers) //starvation  
        {
            result = Mathf.RoundToInt(Options.PopStarvationSpeed.get() * getPopulation() * -1);
            if (result * -1 >= getPopulation()) // total starvation
                result = 0;
        }

        return result;
        //return (int)Mathf.RoundToInt(this.population * PopUnit.growthSpeed.get());
    }
    public void calcDemotions()
    {
        int demotionSize = getDemotionSize();
        if (wantsToDemote() && demotionSize > 0 && this.getPopulation() >= demotionSize)
            demote(getRichestDemotionTarget(), demotionSize);
    }
    public int getDemotionSize()
    {
        int result = (int)(this.getPopulation() * Options.PopDemotionSpeed.get());
        if (result > 0)
            return result;
        else
        if (province.hasAnotherPop(this.type) && getAge() > Options.PopAgeLimitToWipeOut)
            return this.getPopulation();// wipe-out
        else
            return 0;
    }

    public bool wantsToDemote()
    {
        //float demotionLimit = 0.50f;
        if (this.needsFullfilled.get() < Options.PopNeedsDemotionLimit.get())
            return true;
        else return false;
    }
    public List<PopType> getPossibeDemotionsList()
    {
        List<PopType> result = new List<PopType>();
        foreach (PopType type in PopType.allPopTypes)
            if (CanThisDemoteInto(this.type))
                result.Add(type);
        return result;
    }

    //abstract public PopType getRichestDemotionTarget();
    public PopType getRichestDemotionTarget()
    {
        Dictionary<PopType, Value> list = new Dictionary<PopType, Value>();

        foreach (PopType nextType in PopType.allPopTypes)
            if (CanThisDemoteInto(nextType))
                list.Add(nextType, province.getMiddleNeedsFulfilling(nextType));
        var result = list.MaxBy(x => x.Value.get());
        if (result.Value.get() > this.needsFullfilled.get())
            return result.Key;
        else
            return null;


        //List<PopLinkageValue> list = new List<PopLinkageValue>();
        //foreach (PopType nextType in PopType.allPopTypes)
        //    if (CanThisDemoteInto(nextType))
        //        list.Add(new PopLinkageValue(nextType,
        //            province.getMiddleNeedsFulfilling(nextType)
        //            ));
        //list = list.OrderByDescending(o => o.amount.get()).ToList();
        //if (list.Count == 0)
        //    return null;
        //else
        //    if (list[0].amount.get() > this.needsFullfilled.get())
        //    return list[0].type;
        //else return null;
    }
    abstract public bool CanThisDemoteInto(PopType popType);

    private void demote(PopType targetType, int amount)
    {
        if (targetType != null)
        {
            PopUnit newPop = PopUnit.Instantiate(targetType, this, amount, this.province, this.culture);
        }
    }
    internal void calcMigrations()
    {
        int migrationSize = getMigrationSize();
        if (wantsToMigrate() && migrationSize > 0 && this.getPopulation() >= migrationSize)
            migrate(getRichestMigrationTarget(), migrationSize);
    }
    /// <summary>
    /// return null if there is no better place to live
    /// </summary>    
    public Province getRichestMigrationTarget()
    {
        Dictionary<Province, Value> provinces = new Dictionary<Province, Value>();
        foreach (var pro in province.getOwner().ownedProvinces)
            //if (provinces.ContainsKey(pro))
            //    provinces[pro] = size;
            //else
            if (pro != this.province)
            {
                var needsInProvince = pro.getMiddleNeedsFulfilling(this.type);
                if (needsInProvince.get() > needsFullfilled.get())
                    provinces.Add(pro, needsInProvince);
            }
        return provinces.MaxBy(x => x.Value.get()).Key;
    }

    private void migrate(Province where, int migrationSize)
    {
        if (where != null)
        {
            PopUnit newPop = PopUnit.Instantiate(type, this, migrationSize, where, this.culture);
        }
    }

    public bool wantsToMigrate()
    {

        if (this.needsFullfilled.get() < Options.PopNeedsMigrationLimit.get())
            return true;
        else return false;
    }
    public int getMigrationSize()
    {
        int result = (int)(this.getPopulation() * Options.PopMigrationSpeed.get());
        if (result > 0)
            return result;
        else
        if (province.hasAnotherPop(this.type) && getAge() > Options.PopAgeLimitToWipeOut)
            return this.getPopulation();// wipe-out
        else
            return 0;
    }
    internal void calcAssimilations()
    {
        if (this.culture != province.getOwner().culture)
        {
            int assimilationSize = getAssimilationSize();
            if (assimilationSize > 0 && this.getPopulation() >= assimilationSize)
                assimilate(province.getOwner().culture, assimilationSize);
        }
    }

    private void assimilate(Culture toWhom, int assimilationSize)
    {
        //if (toWhom != null)
        //{
        PopUnit newPop = PopUnit.Instantiate(type, this, assimilationSize, this.province, toWhom);
        //}
    }

    public int getAssimilationSize()
    {
        int result = (int)(this.getPopulation() * Options.PopAssimilationSpeed.get());
        if (result > 0)
            return result;
        else if (getAge() > Options.PopAgeLimitToWipeOut)
            return this.getPopulation(); // wipe-out
        else
            return 0;

    }

    internal void Invest()
    {
        if (type == PopType.aristocrats)
        {
            if (!province.isThereMoreThanFactoriesInUpgrade(Options.maximumFactoriesInUpgradeToBuildNew))
            {
                if (province.getResource() != null)
                {
                    FactoryType ftype = FactoryType.whoCanProduce(province.getResource());
                    PrimitiveStorageSet resourceToBuild;
                    Factory factory = province.getResourceFactory();
                    if (factory == null)
                        resourceToBuild = ftype.getBuildNeeds();
                    else
                        resourceToBuild = ftype.getUpgradeNeeds();
                    //build new shownFactory
                    if (factory == null)
                    //Has money/ resources?
                    {
                        Storage needFood = resourceToBuild.findStorage(Product.Food);
                        if (storageNow.get() >= needFood.get())
                        {
                            Factory fact = new Factory(province, this, ftype);
                            //wallet.pay(fact.wallet, new Value(100f));
                            storageNow.subtract(needFood);
                        }
                        //if (wallet.CanAfford(resourceToBuild))
                        //{// build new one
                        //    Factory fact = new Factory(province, this, ftype);
                        //    wallet.pay(fact.wallet, new Value(100f));
                        //}
                        //else;
                    }
                    else//upgrade shownFactory
                    {
                        Value cost = Game.market.getCost(resourceToBuild);

                        if (factory != null
                            //&& wallet.canPay(cost)
                            //&& factory.canUpgrade()
                            //&& !factory.isUpgrading()
                            //&& !factory.isBuilding()
                            && factory.conditionsUpgrade.isAllTrue(this)
                            && factory.getWorkForceFullFilling() > Options.minWorkforceFullfillingToUpgradeFactory
                            && factory.getMargin().get() >= Options.minMarginToUpgrade)
                        {
                            factory.upgrade(this);
                            //wallet.pay(factory.wallet, cost); // upgrade
                        }
                    }
                }
            }
        }
        //if ()
        //if (province.getOwner().isInvented(InventionType.capitalism) && type == PopType.capitalists && Game.random.Next(10) == 1)
        if (Economy.isMarket.checkIftrue(province.getOwner()) && type == PopType.capitalists && Game.random.Next(10) == 1)
        {
            //should I buld?
            if (//province.getUnemployed() > Game.minUnemploymentToBuldFactory && 
                !province.isThereMoreThanFactoriesInUpgrade(Options.maximumFactoriesInUpgradeToBuildNew))
            {
                FactoryType proposition = FactoryType.getMostTeoreticalProfitable(province);
                if (proposition != null)
                    if (province.CanBuildNewFactory(proposition) &&
                        (province.getUnemployed() > Options.minUnemploymentToBuldFactory || province.getMiddleFactoryWorkforceFullfilling() > Options.minFactoryWorkforceFullfillingToBuildNew))
                    {
                        PrimitiveStorageSet resourceToBuild = proposition.getBuildNeeds();
                        Value cost = Game.market.getCost(resourceToBuild);
                        cost.add(Options.factoryMoneyReservPerLevel);
                        if (wallet.canPay(cost))
                        {
                            Factory found = new Factory(province, this, proposition);
                            wallet.payWithoutRecord(found.wallet, cost);
                        }
                        else // find money in bank?
                        if (province.getOwner().isInvented(InventionType.banking))
                        {
                            Value needLoan = new Value(cost.get() - wallet.haveMoney.get());
                            if (province.getOwner().bank.CanITakeThisLoan(needLoan))
                            {
                                province.getOwner().bank.TakeLoan(this, needLoan);
                                Factory found = new Factory(province, this, proposition);
                                wallet.payWithoutRecord(found.wallet, cost);
                            }
                        }
                    }
                //upgrade section

                // if (Game.random.Next(10) == 1) // is there factories to upgrde?
                {
                    Factory factory = FactoryType.getMostPracticlyProfitable(province);
                    //Factory f = province.findFactory(proposition);
                    if (factory != null
                        && factory.canUpgrade()
                        && factory.getMargin().get() >= Options.minMarginToUpgrade
                        && factory.getWorkForceFullFilling() > Options.minWorkforceFullfillingToUpgradeFactory)
                    {
                        //PrimitiveStorageSet resourceToBuild = proposition.getUpgradeNeeds();
                        //Value cost = Game.market.getCost(resourceToBuild);
                        Value cost = factory.getUpgradeCost();
                        if (wallet.canPay(cost))
                            factory.upgrade(this);
                        else // find money in bank?
                        if (province.getOwner().isInvented(InventionType.banking))
                        {
                            Value needLoan = new Value(cost.get() - wallet.haveMoney.get());
                            if (province.getOwner().bank.CanITakeThisLoan(needLoan))
                            {
                                province.getOwner().bank.TakeLoan(this, needLoan);
                                factory.upgrade(this);
                            }
                        }
                    }
                }
            }
        }
    }



    override public string ToString()
    {
        return type + " from " + province;
    }
}
public class Tribemen : PopUnit
{
    public Tribemen(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.tribeMen, where, culture)
    {
    }
    public Tribemen(int iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
    {
    }
    public override bool CanThisDemoteInto(PopType targetType)
    {
        if (targetType == this.type
            || targetType == PopType.farmers && !province.getOwner().isInvented(InventionType.farming)
            || targetType == PopType.capitalists
            || targetType == PopType.aristocrats)
            return false;
        else
            return true;
    }
    public override void produce()
    {
        Value producedAmount;
        float overpopulation = province.getOverpopulation();
        if (overpopulation <= 1) // all is ok
            producedAmount = new Value(getPopulation() * type.basicProduction.get() / 1000f);
        else
            producedAmount = new Value(getPopulation() * type.basicProduction.get() / 1000f / overpopulation);
        storageNow.add(producedAmount);
        gainGoodsThisTurn.set(producedAmount);
    }
    internal override bool canTrade()
    {
        return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    internal override bool getSayingYes(AbstractReformValue reform)
    {
        if (reform == Government.Tribal)
        {
            var baseOpinion = new Procent(1f);
            baseOpinion.add(this.loyalty);
            //return baseOpinion.getProcent(this.population);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.8f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.2f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else
            return false;

    }

    internal override bool canVote()
    {
        Country count = province.getOwner();
        var government = count.government.status;
        if (government == Government.Tribal || government == Government.Democracy)
            return true;
        else
            return false;
    }
}
public class Farmers : PopUnit
{
    public Farmers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.farmers, where, culture)
    { }
    public Farmers(int iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
    { }

    public override bool CanThisDemoteInto(PopType targetType)
    {
        if (targetType == this.type
            || targetType == PopType.farmers && !province.getOwner().isInvented(InventionType.farming)
            || targetType == PopType.capitalists
            || targetType == PopType.aristocrats)
            return false;
        else
            return true;
    }

    public override void produce()
    {
        Value producedAmount;
        float overpopulation = province.getOverpopulation();
        if (overpopulation <= 1) // all is ok
            producedAmount = new Value(getPopulation() * type.basicProduction.get() / 1000 + getPopulation() * type.basicProduction.get() / 1000 * education.get());
        else
            producedAmount = new Value(getPopulation() * type.basicProduction.get() / 1000 / overpopulation + getPopulation() * type.basicProduction.get() / 1000 / overpopulation * education.get());
        gainGoodsThisTurn.set(producedAmount);

        if (Economy.isMarket.checkIftrue(province.getOwner()))
        {
            sentToMarket.set(gainGoodsThisTurn);
            Game.market.sentToMarket.add(gainGoodsThisTurn);
        }
        else
            storageNow.add(gainGoodsThisTurn);
    }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getOwner()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    internal override bool getSayingYes(AbstractReformValue reform)
    {
        if (reform is Government.ReformValue)
        {
            if (reform == Government.Tribal)
            {
                var baseOpinion = new Procent(0f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Options.votingPassBillLimit;
            }
            else if (reform == Government.Aristocracy)
            {
                var baseOpinion = new Procent(0.2f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Options.votingPassBillLimit;
            }
            else if (reform == Government.Democracy)
            {
                var baseOpinion = new Procent(1f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Options.votingPassBillLimit;
            }
            else if (reform == Government.Despotism)
            {
                var baseOpinion = new Procent(0.2f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Options.votingPassBillLimit;
            }
            else if (reform == Government.ProletarianDictatorship)
            {
                var baseOpinion = new Procent(0.3f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Options.votingPassBillLimit;
            }
            else
                return false;
        }
        else if (reform is TaxationForPoor.ReformValue)
        {
            TaxationForPoor.ReformValue taxReform = reform as TaxationForPoor.ReformValue;
            var baseOpinion = new Procent(1f);
            baseOpinion.set(baseOpinion.get() - taxReform.tax.get() * 2);
            baseOpinion.set(baseOpinion.get() + loyalty.get() - 0.5f);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else
            return false;
    }
    internal override bool canVote()
    {
        Country count = province.getOwner();
        var government = count.government.status;
        if (government == Government.Democracy || government == Government.AnticRespublic || government == Government.WealthDemocracy)
            return true;
        else
            return false;
    }
}
public class Aristocrats : PopUnit
{
    public Aristocrats(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.aristocrats, where, culture)
    { }
    public Aristocrats(int iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
    { }
    public override bool CanThisDemoteInto(PopType targetType)
    {
        if (targetType == this.type
            || targetType == PopType.farmers && !province.getOwner().isInvented(InventionType.farming)
            || targetType == PopType.capitalists && Economy.isNotMarket.checkIftrue(province.getOwner()))
            return false;
        else
            return true;
    }
    internal void dealWithMarket()
    {
        if (storageNow.get() > Options.aristocratsFoodReserv)
        {
            Storage howMuchSend = new Storage(storageNow.getProduct(), storageNow.get() - Options.aristocratsFoodReserv);
            storageNow.pay(sentToMarket, howMuchSend);
            //sentToMarket.set(howMuchSend);
            Game.market.sentToMarket.add(howMuchSend);
        }
    }
    public override void produce()
    { }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getOwner()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }
    internal override bool getSayingYes(AbstractReformValue reform)
    {
        if (reform == Government.Tribal)
        {
            var baseOpinion = new Procent(0.4f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.6f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else
            return false;
    }
    internal override bool canVote()
    {
        Country count = province.getOwner();
        var government = count.government.status;
        if (government == Government.Democracy || government == Government.AnticRespublic || government == Government.WealthDemocracy || government == Government.Aristocracy || government == Government.Tribal)
            return true;
        else
            return false;
    }
}
public class Capitalists : PopUnit
{
    public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.capitalists, where, culture)
    { }
    public Capitalists(int iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
    { }
    public override bool CanThisDemoteInto(PopType targetType)
    {
        if (targetType == this.type
            || targetType == PopType.farmers && !province.getOwner().isInvented(InventionType.farming))
            return false;
        else
            return true;
    }
    public override void produce()
    { }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getOwner()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }
    internal override bool getSayingYes(AbstractReformValue reform)
    {
        if (reform == Government.Tribal)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.8f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.3f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else
            return false;
    }
    internal override bool canVote()
    {
        Country count = province.getOwner();
        var government = count.government.status;
        if (government == Government.Democracy || government == Government.AnticRespublic || government == Government.WealthDemocracy || government == Government.BourgeoisDictatorship)
            return true;
        else
            return false;
    }
}
public class Workers : PopUnit
{
    public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.workers, where, culture)
    { }
    public Workers(int iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
    { }

    public override bool CanThisDemoteInto(PopType targetType)
    {
        if (targetType == this.type
            || targetType == PopType.farmers && !province.getOwner().isInvented(InventionType.farming)
            || targetType == PopType.capitalists
            || targetType == PopType.aristocrats)
            return false;
        else
            return true;
    }
    public override void produce()
    { }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getOwner()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    internal override bool getSayingYes(AbstractReformValue reform)
    {
        if (reform == Government.Tribal)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.6f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.3f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.8f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Options.votingPassBillLimit;
        }
        else
            return false;
    }
    internal override bool canVote()
    {
        Country count = province.getOwner();
        var government = count.government.status;
        if (government == Government.Democracy)
            return true;
        else
            return false;
    }
}
//public class PopLinkageValue
//{
//    public PopType type;
//    public Value amount;
//    internal PopLinkageValue(PopType p, Value a)
//    {
//        type = p;
//        amount = a;
//    }
//}