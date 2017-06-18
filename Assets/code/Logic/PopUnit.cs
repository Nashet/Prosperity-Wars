using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;


abstract public class PopUnit : Producer
{
    ///<summary>buffer popList of demoted. To avoid iteration breaks</summary>
    public readonly static List<PopUnit> PopListToAddToGeneralList = new List<PopUnit>();

    public Procent loyalty;
    int population;
    int mobilized;

    public PopType popType;

    public Culture culture;

    public Procent education;
    public Procent needsFullfilled;

    private int daysUpsetByForcedReform;
    private bool didntGetPromisedUnemloymentSubsidy;

    public readonly static ModifiersList modifiersLoyaltyChange;

    static Modifier modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierEverydayNeedsFulfilled, modifierLifeNeedsFulfilled,
        modifierStarvation, modifierUpsetByForcedReform, modifierLifeNeedsNotFulfilled, modifierNotGivenUnemploymentSubsidies,
        modifierMinorityPolicy;

    public Value incomeTaxPayed = new Value(0);

    private readonly DateTime born;
    //if add new fields make sure it's implemented in second constructor and in merge()   

    static PopUnit()
    {
        //makeModifiers();
        modifierStarvation = new Modifier(x => (x as PopUnit).needsFullfilled.get() < 0.20f, "Starvation", -0.3f, false);
        modifierLifeNeedsNotFulfilled = new Modifier(x => (x as PopUnit).getLifeNeedsFullfilling().get() < 0.99f, "Life needs are not satisfied", -0.2f, false);
        modifierLifeNeedsFulfilled = new Modifier(x => (x as PopUnit).getLifeNeedsFullfilling().get() > 0.99f, "Life needs are satisfied", 0.1f, false);
        modifierEverydayNeedsFulfilled = new Modifier(x => (x as PopUnit).getEveryDayNeedsFullfilling().get() > 0.99f, "Everyday needs are satisfied", 0.15f, false);
        modifierLuxuryNeedsFulfilled = new Modifier(x => (x as PopUnit).getLuxuryNeedsFullfilling().get() > 0.99f, "Luxury needs are satisfied", 0.2f, false);

        //Game.threadDangerSB.Clear();
        //Game.threadDangerSB.Append("Likes that government because can vote with ").Append(this.province.getOwner().government.ToString());
        modifierCanVote = new Modifier(x => (x as PopUnit).canVote(), "Can vote with that government ", 0.1f, false);
        //Game.threadDangerSB.Clear();
        //Game.threadDangerSB.Append("Dislikes that government because can't vote with ").Append(this.province.getOwner().government.ToString());
        modifierCanNotVote = new Modifier(x => !(x as PopUnit).canVote(), "Can't vote with that government ", -0.1f, false);
        //Game.threadDangerSB.Clear();
        //Game.threadDangerSB.Append("Upset by forced reform - ").Append(daysUpsetByForcedReform).Append(" days");
        modifierUpsetByForcedReform = new Modifier(x => (x as PopUnit).daysUpsetByForcedReform > 0, "Upset by forced reform", -0.3f, false);
        modifierNotGivenUnemploymentSubsidies = new Modifier(x => (x as PopUnit).didntGetPromisedUnemloymentSubsidy, "Didn't got promised Unemployment Subsidies", -1.0f, false);
        modifierMinorityPolicy = //new Modifier(MinorityPolicy.IsResidencyPop, 0.02f);
        new Modifier(x => !(x as PopUnit).isStateCulture()
        && ((x as PopUnit).province.getCountry().minorityPolicy.status == MinorityPolicy.Residency
        || (x as PopUnit).province.getCountry().minorityPolicy.status == MinorityPolicy.NoRights), "Is minority", -0.1f, false);


        //MinorityPolicy.IsResidency
        modifiersLoyaltyChange = new ModifiersList(new List<Condition>
        {
           modifierStarvation, modifierLifeNeedsNotFulfilled, modifierLifeNeedsFulfilled, modifierEverydayNeedsFulfilled,
        modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierUpsetByForcedReform, modifierNotGivenUnemploymentSubsidies,
            modifierMinorityPolicy
});
    }
    protected PopUnit(int iamount, PopType ipopType, Culture iculture, Province where) : base(where.getCountry().bank)
    {
        born = Game.date;
        population = iamount;
        popType = ipopType;
        culture = iculture;

        storageNow = new Storage(Product.findByName("Food"), 0);
        gainGoodsThisTurn = new Storage(Product.findByName("Food"), 0);
        sentToMarket = new Storage(Product.findByName("Food"), 0);
        education = new Procent(0.00f);
        loyalty = new Procent(0.50f);
        needsFullfilled = new Procent(0.50f);
        province = where;

    }
    /// <summary> Creates new PopUnit basing on part of other PopUnit.
    /// And transfers sizeOfNewPop population.
    /// </summary>    
    protected PopUnit(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(where.getCountry().bank)
    {
        born = Game.date;
        PopListToAddToGeneralList.Add(this);
        // makeModifiers();

        // here should be careful copying of popUnit data
        //And careful editing of old unit
        Procent newPopShare = Procent.makeProcent(sizeOfNewPop, source.getPopulation());

        //Own PopUnit fields:
        loyalty = new Procent(source.loyalty.get());
        population = sizeOfNewPop;
        if (source.population - sizeOfNewPop <= 0 && this.popType == PopType.aristocrats || this.popType == PopType.capitalists)
            // if source pop is gonna be dead..
            //secede property... to new pop.. 
            //todo - can optimize it, double run on List
            source.getOwnedFactories().ForEach(x => x.setOwner(this));

        mobilized = 0;
        popType = newPopType;
        this.culture = culture;
        education = new Procent(source.education.get());
        needsFullfilled = new Procent(source.needsFullfilled.get());
        daysUpsetByForcedReform = 0;
        didntGetPromisedUnemloymentSubsidy = false;
        //incomeTaxPayed = newPopShare.sendProcentToNew(source.incomeTaxPayed);

        //Agent's fields:
        //wallet = new Wallet(0f, where.getCountry().bank); it's already set in constructor
        //bank - could be different, set in constructor
        //loans - keep it in old unit        
        //take deposit share?
        if (source.deposits.isExist())
        {
            Value takeDeposit = source.deposits.multipleOutside(newPopShare);
            if (source.getCountry().bank.canGiveMoney(this, takeDeposit))
            {
                source.getCountry().bank.giveMoney(source, takeDeposit);
                source.payWithoutRecord(this, takeDeposit);
            }
        }
        source.payWithoutRecord(this, source.cash.multipleOutside(newPopShare));

        //Producer's fields:
        storageNow = newPopShare.sendProcentToNew(source.storageNow);
        gainGoodsThisTurn = new Storage(source.gainGoodsThisTurn.getProduct());
        sentToMarket = new Storage(source.sentToMarket.getProduct());

        province = where;//source.province;

        //Consumer's fields:
        consumedTotal = new PrimitiveStorageSet();
        consumedLastTurn = new PrimitiveStorageSet();
        consumedInMarket = new PrimitiveStorageSet();

        //kill in the end
        source.subtractPopulation(sizeOfNewPop);
    }

    internal abstract int getVotingPower(Government.ReformValue reformValue);
    internal int getVotingPower()
    {
        return getVotingPower(getCountry().government.getTypedValue());
    }

    /// <summary>
    /// Merging source into this pop
    /// assuming that both pops are in same province, and has same type
    /// culture defaults to this.culture
    /// </summary>    
    internal void mergeIn(PopUnit source)
    {
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

        //Agent's fields:        
        source.sendAllAvailableMoneyWithoutRecord(this); // includes deposits
        loans.add(source.loans);
        // Bank - stays same

        //Producer's fields:
        storageNow.add(source.storageNow);
        gainGoodsThisTurn.add(source.gainGoodsThisTurn);
        sentToMarket.add(source.sentToMarket);
        //province - read header

        //consumer's fields
        consumedTotal.add(source.consumedTotal);
        consumedLastTurn.add(source.consumedLastTurn);
        consumedLastTurn.add(source.consumedLastTurn);

        //province = source.province; don't change that

        //if (source.population - sizeOfNewPop <= 0)// if source pop is gonna be dead..It gonna be, for sure
        //secede property... to new pop.. 
        //todo - can optimize it, double run on List. Also have point only in Consolidation, not for PopUnit.PopListToAddToGeneralList
        //that check in not really needed as it this pop supposed to be same type as source
        //if (this.type == PopType.aristocrats || this.type == PopType.capitalists)
        source.getOwnedFactories().ForEach(x => x.setOwner(this));

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
            MainCamera.popUnitPanel.hide();
        //remove from population panel.. Would do it automatically        
        //secede property... to government
        getOwnedFactories().ForEach(x => x.setOwner(province.getCountry()));
        sendAllAvailableMoney(getCountry().bank); // just in case if there is something
        getCountry().bank.defaultLoaner(this);
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        incomeTaxPayed.set(0); // need it because pop could stop paying taxes due to reforms for example
        needsFullfilled.set(0f);
        didntGetPromisedUnemloymentSubsidy = false;
        // pop.storageNow.set(0f);
    }
    //abstract public Procent howIsItGoodForMe(AbstractReformValue reform);
    public List<Factory> getOwnedFactories()
    {
        List<Factory> result = new List<Factory>();
        if (popType == PopType.aristocrats || popType == PopType.capitalists)
        {
            foreach (var item in province.allFactories)
                if (item.getOwner() == this)
                    result.Add(item);
            return result;
        }
        else //return empty list
            return result;
    }
    public int getAge()
    {
        //return Game.date - born;
        return born.getYearsSince();
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

        this.subtractPopulation((int)(loss * Options.PopAttritionFactor));
        mobilized -= loss;
        if (mobilized < 0) mobilized = 0;
    }
    internal void addDaysUpsetByForcedReform(int popDaysUpsetByForcedReform)
    {
        daysUpsetByForcedReform += popDaysUpsetByForcedReform;
    }

    /// <summary>
    /// Creates Pop in PopListToAddToGeneralList, later in will go to proper List
    /// </summary>    
    public static PopUnit makeVirtualPop(PopType type, PopUnit source, int sizeOfNewPop, Province where, Culture culture)
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

    internal bool getSayingYes(AbstractReformValue reform)
    {
        return reform.modVoting.getModifier(this) > Options.votingPassBillLimit;
    }
    public static int getRandomPopulationAmount(int minGeneratedPopulation, int maxGeneratedPopulation)
    {
        int randomPopulation = minGeneratedPopulation + Game.Random.Next(maxGeneratedPopulation - minGeneratedPopulation);
        return randomPopulation;
    }



    /// <summary> /// Return in pieces  /// </summary>    
    override internal float getLocalEffectiveDemand(Product product)
    {
        float result = 0;
        // need to know how much i Consumed inside my needs
        PrimitiveStorageSet needs = new PrimitiveStorageSet(getRealLifeNeeds());
        Storage need = needs.findStorage(product);
        if (need != null)
        {
            Storage canAfford = HowMuchCanAfford(need);
            result += canAfford.get();
        }
        needs = new PrimitiveStorageSet(getRealEveryDayNeeds());
        need = needs.findStorage(product);
        if (need != null)
        {
            Storage canAfford = HowMuchCanAfford(need);
            result += canAfford.get();
        }
        needs = new PrimitiveStorageSet(getRealLuxuryNeeds());
        need = needs.findStorage(product);
        if (need != null)
        {
            Storage canAfford = HowMuchCanAfford(need);
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
                nStor.multiple(multiplier);
                result.Add(nStor);
            }
        if (Game.Random.Next(20) == 1)
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
        return getNeedsInCommon(popType.getLifeNeedsPer1000());
    }

    public List<Storage> getRealEveryDayNeeds()
    {
        return getNeedsInCommon(popType.getEveryDayNeedsPer1000());
    }

    public List<Storage> getRealLuxuryNeeds()
    {
        return getNeedsInCommon(this.popType.getLuxuryNeedsPer1000());
    }
    public List<Storage> getRealAllNeeds()
    {
        return getNeedsInCommon(this.popType.getAllNeedsPer1000());
    }

    internal Procent getUnemployedProcent()
    {
        if (popType == PopType.workers)
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
            if (popType == PopType.farmers || popType == PopType.tribeMen)
        {
            float overPopulation = province.getOverpopulation();
            if (overPopulation <= 1f)
                return new Procent(0);
            else
                return new Procent(1f - (1f / overPopulation));
        }
        else return new Procent(0);
    }

    internal Country getCountry()
    {
        return province.getCountry();
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
        if (this.popType == PopType.aristocrats && Serfdom.IsNotAbolishedInAnyWay.checkIftrue((province.getCountry())))
            return false;
        else return true;
    }
    public override void payTaxes() // should be abstract 
    {
        Value taxSize = new Value(0);
        if (Economy.isMarket.checkIftrue(province.getCountry()) && popType != PopType.tribeMen)
        {
            if (this.popType.isPoorStrata())
            {
                taxSize = moneyIncomethisTurn.multipleOutside((province.getCountry().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
                if (canPay(taxSize))
                {
                    incomeTaxPayed = taxSize;
                    province.getCountry().poorTaxIncomeAdd(taxSize);
                    pay(province.getCountry(), taxSize);
                }
                else
                {
                    incomeTaxPayed.set(cash);
                    province.getCountry().poorTaxIncomeAdd(cash);
                    sendAllAvailableMoney(province.getCountry());

                }
            }
            else
            if (this.popType.isRichStrata())
            {
                taxSize = moneyIncomethisTurn.multipleOutside((province.getCountry().taxationForRich.getValue() as TaxationForRich.ReformValue).tax);
                if (canPay(taxSize))
                {
                    incomeTaxPayed.set(taxSize);
                    province.getCountry().richTaxIncomeAdd(taxSize);
                    pay(province.getCountry(), taxSize);
                }
                else
                {
                    incomeTaxPayed.set(cash);
                    province.getCountry().richTaxIncomeAdd(cash);
                    sendAllAvailableMoney(province.getCountry());
                }
            }

        }
        else// non market
        if (this.popType != PopType.aristocrats)
        {
            // taxSize = gainGoodsThisTurn.multiple(province.getOwner().countryTax);

            if (this.popType.isPoorStrata())
                taxSize = gainGoodsThisTurn.multipleOutside((province.getCountry().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
            else
            if (this.popType.isRichStrata())
                taxSize = gainGoodsThisTurn.multipleOutside((province.getCountry().taxationForRich.getValue() as TaxationForPoor.ReformValue).tax);

            if (storageNow.has(taxSize))
                storageNow.send(province.getCountry().storageSet, taxSize);
            else
                storageNow.sendAll(province.getCountry().storageSet);
        }
    }

    internal bool isStateCulture()
    {
        return this.culture == this.province.getCountry().getCulture();
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
    private void consumeEveryDayAndLuxury(List<Storage> needs, byte howDeep)
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
                    if (howDeep != 0) consumeEveryDayAndLuxury(getRealLuxuryNeeds(), howDeep);
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
                if (storageNow.has(need))// dont need to buy on market
                {
                    storageNow.subtract(need);
                    consumedTotal.set(need);
                    //consumedInMarket.Set(need); are you crazy?
                    needsFullfilled.set(1f / 3f);
                    //consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 0.66f, 2);
                }
                else
                    needsFullfilled.set(Game.market.buy(this, need, null).get() / 3f);
            }

        //if (NeedsFullfilled.get() > 0.33f) NeedsFullfilled.set(0.33f);

        if (getLifeNeedsFullfilling().get() >= 0.95f)
        {
            Agent reserv = new Agent(0f, null);
            payWithoutRecord(reserv, cash.multipleOutside(Options.savePopMoneyReserv));
            var everyDayNeeds = (getRealEveryDayNeeds());
            Value needsCost = Game.market.getCost(everyDayNeeds);
            float moneyWas = cash.get();
            Value spentMoney;

            foreach (Storage need in everyDayNeeds)
            {
                //NeedsFullfilled.set(0.33f + Game.market.Consume(this, need).get() / 3f);
                Game.market.buy(this, need, null);
            }
            spentMoney = new Value(moneyWas - cash.get());
            if (spentMoney.get() != 0f)
                needsFullfilled.add(spentMoney.get() / needsCost.get() / 3f);
            if (getEveryDayNeedsFullfilling().get() >= 0.95f)
            {
                var luxuryNeeds = (getRealLuxuryNeeds());
                needsCost = Game.market.getCost(luxuryNeeds);
                moneyWas = cash.get();
                foreach (Storage need in luxuryNeeds)
                {
                    Game.market.buy(this, need, null);
                    //NeedsFullfilled.set(0.66f + Game.market.Consume(this, need).get() / 3f);

                }
                spentMoney = new Value(moneyWas - cash.get());
                if (spentMoney.get() != 0f)
                    needsFullfilled.add(spentMoney.get() / needsCost.get() / 3f);
            }
            reserv.payWithoutRecord(this, reserv.cash);
        }
    }
    /// <summary> </summary>
    public override void buyNeeds()
    {
        //life needs First
        List<Storage> needs = getRealLifeNeeds();

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
                    consumedTotal.set(need);
                    needsFullfilled.set(1f / 3f);
                    consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 2);
                }
                else
                {
                    float canConsume = storageNow.get();
                    consumedTotal.set(storageNow);
                    storageNow.set(0);
                    needsFullfilled.set(canConsume / need.get() / 3f);
                }
            if (popType == PopType.aristocrats) // to allow trade without capitalism
                subConsumeOnMarket(needs, true);
        }
    }
    abstract internal bool canTrade();
    internal bool canVote()
    {
        return canVote(getCountry().government.getTypedValue());
    }
    abstract internal bool canVote(Government.ReformValue reform);
    public void calcLoyalty()
    {
        float newRes = loyalty.get() + modifiersLoyaltyChange.getModifier(this) / 100f;
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
            Value taxSize = gainGoodsThisTurn.multipleOutside(province.getCountry().serfdom.status.getTax());
            province.shareWithAllAristocrats(storageNow, taxSize);
        }
    }

    abstract public bool ShouldPayAristocratTax();




    public void calcPromotions()
    {
        int promotionSize = getPromotionSize();
        if (wantsToPromote() && promotionSize > 0 && this.getPopulation() >= promotionSize)
            promote(getRichestPromotionTarget(), promotionSize);
    }
    public int getPromotionSize()
    {
        int result = (int)(this.getPopulation() * Options.PopPromotionSpeed.get());
        if (result > 0)
            return result;
        else
        if (province.hasAnotherPop(this.popType) && getAge() > Options.PopAgeLimitToWipeOut)
            return this.getPopulation();// wipe-out
        else
            return 0;
    }

    public bool wantsToPromote()
    {
        if (this.needsFullfilled.get() > Options.PopNeedsPromotionLimit.get())
            return true;
        else return false;
    }

    //abstract public PopType getRichestDemotionTarget();
    public PopType getRichestPromotionTarget()
    {
        Dictionary<PopType, Value> list = new Dictionary<PopType, Value>();
        foreach (PopType nextType in PopType.allPopTypes)
            if (canThisPromoteInto(nextType))
                list.Add(nextType, province.getMiddleNeedsFulfilling(nextType));
        var result = list.MaxBy(x => x.Value.get());
        if (result.Value != null && result.Value.get() > this.needsFullfilled.get())
            return result.Key;
        else
            return null;
    }
    abstract public bool canThisPromoteInto(PopType popType);

    private void promote(PopType targetType, int amount)
    {
        if (targetType != null)
        {
            PopUnit.makeVirtualPop(targetType, this, amount, this.province, this.culture);
        }
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
        var reform = province.getCountry().unemploymentSubsidies.getValue();
        if (getUnemployedProcent().get() > 0 && reform != UnemploymentSubsidies.None)
        {
            Value subsidy = getUnemployedProcent();
            subsidy.multiple(getPopulation() / 1000f * (reform as UnemploymentSubsidies.ReformValue).getSubsidiesRate());
            //float subsidy = population / 1000f * getUnemployedProcent().get() * (reform as UnemploymentSubsidies.LocalReformValue).getSubsidiesRate();
            if (province.getCountry().canPay(subsidy))
            {
                province.getCountry().pay(this, subsidy);
                province.getCountry().unemploymentSubsidiesExpenseAdd(subsidy);
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
        else if (popType != PopType.farmers) //starvation  
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
    private void demote(PopType targetType, int amount)
    {
        if (targetType != null)
        {
            PopUnit.makeVirtualPop(targetType, this, amount, this.province, this.culture);
        }
    }
    public int getDemotionSize()
    {
        int result = (int)(this.getPopulation() * Options.PopDemotionSpeed.get());
        if (result > 0)
            return result;
        else
        if (province.hasAnotherPop(this.popType) && getAge() > Options.PopAgeLimitToWipeOut)
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
        foreach (PopType nextType in PopType.allPopTypes)
            if (canThisDemoteInto(this.popType))
                result.Add(nextType);
        return result;
    }

    //abstract public PopType getRichestDemotionTarget();
    public PopType getRichestDemotionTarget()
    {
        Dictionary<PopType, Value> list = new Dictionary<PopType, Value>();

        foreach (PopType nextType in PopType.allPopTypes)
            if (canThisDemoteInto(nextType))
                list.Add(nextType, province.getMiddleNeedsFulfilling(nextType));
        var result = list.MaxBy(x => x.Value.get());
        if (result.Value != null && result.Value.get() > this.needsFullfilled.get())
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

    abstract public bool canThisDemoteInto(PopType popType);


    //**********************************************
    internal void calcImmigrations()
    {
        int immigrationSize = getImmigrationSize();
        if (wantsToImmigrate() && immigrationSize > 0 && this.getPopulation() >= immigrationSize)
            immigrate(getRichestImmigrationTarget(), immigrationSize);
    }
    private void immigrate(Province where, int immigrationSize)
    {
        if (where != null)
        {
            PopUnit.makeVirtualPop(popType, this, immigrationSize, where, this.culture);
        }
    }
    /// <summary>
    /// return null if there is no better place to live
    /// </summary>    
    public Province getRichestImmigrationTarget()
    {
        Dictionary<Province, Value> provinces = new Dictionary<Province, Value>();
        //where to g0?
        // where life is rich and I where I have some rights
        foreach (var country in Country.getExisting())
            if (country.getCulture() == this.culture || country.minorityPolicy.getValue() == MinorityPolicy.Equality)
                if (country != this.getCountry())
                    foreach (var pro in country.ownedProvinces)
                    {
                        var needsInTargetProvince = pro.getMiddleNeedsFulfilling(this.popType);
                        if (needsInTargetProvince.get() >= this.needsFullfilled.get())
                            provinces.Add(pro, needsInTargetProvince);
                    }
        return provinces.MaxBy(x => x.Value.get()).Key;
    }

    internal void putExtraMoneyInBank()
    {
        if (getCountry().isInvented(Invention.banking))
        {
            Value extraMoney = new Value(cash.get() - Game.market.getCost(this.getRealAllNeeds()).get() * 10f);
            if (extraMoney.get() > 5f)
                getCountry().bank.takeMoney(this, extraMoney);
        }
    }

    public bool wantsToImmigrate()
    {
        if (this.needsFullfilled.get() < Options.PopNeedsImmigrationLimit.get()
            || (getCountry().minorityPolicy.getValue() != MinorityPolicy.Equality && !isStateCulture()))
            return true;
        else return false;
    }
    public int getImmigrationSize()
    {
        int result = (int)(this.getPopulation() * Options.PopImmigrationSpeed.get());
        if (result > 0)
            return result;
        else
        if (province.hasAnotherPop(this.popType) && getAge() > Options.PopAgeLimitToWipeOut)
            return this.getPopulation();// wipe-out
        else
            return 0;
    }
    //**********************************************
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
        //foreach (var pro in province.getCountry().ownedProvinces)            
        foreach (var pro in province.getNeigbors(x => x.getCountry() == province.getCountry()))
        //if (pro != this.province)
        {
            var needsInProvince = pro.getMiddleNeedsFulfilling(this.popType);
            if (needsInProvince.get() > needsFullfilled.get())
                provinces.Add(pro, needsInProvince);
        }
        return provinces.MaxBy(x => x.Value.get()).Key;
    }

    private void migrate(Province where, int migrationSize)
    {
        if (where != null)
        {
            PopUnit.makeVirtualPop(popType, this, migrationSize, where, this.culture);
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
        if (province.hasAnotherPop(this.popType) && getAge() > Options.PopAgeLimitToWipeOut)
            return this.getPopulation();// wipe-out
        else
            return 0;
    }
    internal void calcAssimilations()
    {
        if (this.culture != province.getCountry().getCulture())
        {
            int assimilationSize = getAssimilationSize();
            if (assimilationSize > 0 && this.getPopulation() >= assimilationSize)
                assimilate(province.getCountry().getCulture(), assimilationSize);
        }
    }

    private void assimilate(Culture toWhom, int assimilationSize)
    {
        //if (toWhom != null)
        //{
        PopUnit.makeVirtualPop(popType, this, assimilationSize, this.province, toWhom);
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

    internal void invest()
    {
        if (popType == PopType.aristocrats)
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
                        resourceToBuild = factory.getUpgradeNeeds();
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
        if (Economy.isMarket.checkIftrue(province.getCountry()) && popType == PopType.capitalists && Game.Random.Next(10) == 1)
        {
            //should I build?
            //province.getUnemployed() > Game.minUnemploymentToBuldFactory && 
            if (!province.isThereMoreThanFactoriesInUpgrade(Options.maximumFactoriesInUpgradeToBuildNew))
            {
                FactoryType proposition = FactoryType.getMostTeoreticalProfitable(province);
                if (proposition != null && province.CanBuildNewFactory(proposition) &&
                    (province.getUnemployedWorkers() > Options.minUnemploymentToBuldFactory || province.getMiddleFactoryWorkforceFullfilling() > Options.minFactoryWorkforceFullfillingToBuildNew))
                {
                    PrimitiveStorageSet resourceToBuild = proposition.getBuildNeeds();
                    Value cost = Game.market.getCost(resourceToBuild);
                    cost.add(Options.factoryMoneyReservPerLevel);
                    if (canPay(cost))
                    {
                        Factory found = new Factory(province, this, proposition);
                        payWithoutRecord(found, cost);
                    }
                    else // find money in bank?
                    if (province.getCountry().isInvented(Invention.banking))
                    {
                        Value needLoan = new Value(cost.get() - cash.get());
                        if (province.getCountry().bank.canGiveMoney(this, needLoan))
                        {
                            province.getCountry().bank.giveMoney(this, needLoan);
                            Factory found = new Factory(province, this, proposition);
                            payWithoutRecord(found, cost);
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
                        if (canPay(cost))
                            factory.upgrade(this);
                        else // find money in bank?
                        if (province.getCountry().isInvented(Invention.banking))
                        {
                            Value needLoan = new Value(cost.get() - cash.get());
                            if (province.getCountry().bank.canGiveMoney(this, needLoan))
                            {
                                province.getCountry().bank.giveMoney(this, needLoan);
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
        return popType + " from " + province;
    }
}