using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// <summary>
///     Clears the contents of the string builder.
/// </summary>
/// <param name="value">
///     The <see cref="StringBuilder"/> to clear.
/// </param>

abstract public class PopUnit : Producer
{

    public static Procent growthSpeed = new Procent(0.002f);
    public static Procent starvationSpeed = new Procent(0.01f);

    ///<summary> demotion  - when popUnit can't fullfill needs</summary>
    public static Procent demotionSpeed = new Procent(0.01f);

    ///<summary> promotion  - when popUnit has chance to get better place in ierarhy</summary>
    public static Procent promotionSpeed = new Procent(0.01f);

    ///<summary>buffer popList of demoter. To avoid iteration breaks</summary>
    public static List<PopUnit> tempPopList = new List<PopUnit>();

    public Procent loyalty;
    public uint population;
    int mobilized;
    public PopType type;
    public Culture culture;
    public Procent education;
    public Procent NeedsFullfilled;

    public ModifiersList modifiersLoyaltyChange;

    Modifier modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierEverydayNeedsFulfilled, modifierLifeNeedsFulfilled,
        modifierStarvation, modifierUpsetByForcedReform, modifierLifeNeedsNotFulfilled, modifierNotGivenUnemploymentSubsidies;
    private uint daysUpsetByForcedReform;
    private bool dintGetUnemloymentSubsidy;


    public PopUnit(uint iamount, PopType ipopType, Culture iculture, Province where)
    {
        population = iamount;
        type = ipopType;
        culture = iculture;

        storageNow = new Storage(Product.findByName("Food"), 0);
        gainGoodsThisTurn = new Storage(Product.findByName("Food"), 0);
        sentToMarket = new Storage(Product.findByName("Food"), 0);
        education = new Procent(0.00f);
        loyalty = new Procent(0.50f);
        NeedsFullfilled = new Procent(0.50f);
        province = where;
        modifierStarvation = new Modifier(delegate (Country forWhom) { return NeedsFullfilled.get() < 0.20f; }, "Starvation", false, -0.3f);
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
        modifierNotGivenUnemploymentSubsidies = new Modifier((Country x) => dintGetUnemloymentSubsidy, "Didn't got promised Unemployment Subsidies", false, -1.0f);
        modifiersLoyaltyChange = new ModifiersList(new List<Condition>()
        {
           modifierStarvation, modifierLifeNeedsNotFulfilled, modifierLifeNeedsFulfilled, modifierEverydayNeedsFulfilled, modifierLuxuryNeedsFulfilled,
            modifierCanVote, modifierCanNotVote, modifierUpsetByForcedReform, modifierNotGivenUnemploymentSubsidies
        });
    }
    
    internal int howMuchCanMobilize()
    {
        int howMuchCanMobilize = (int)(population * loyalty.get() * Game.mobilizationFactor);
        howMuchCanMobilize -= mobilized;
        mobilized += howMuchCanMobilize;
        return howMuchCanMobilize;
    }
    public Corps mobilize()
    {
        int amount = howMuchCanMobilize();
        if (amount > 1)
            return new Corps(this, amount);
        else
            return null;
    }

    internal void addDaysUpsetByForcedReform(uint popDaysUpsetByForcedReform)
    {
        daysUpsetByForcedReform += popDaysUpsetByForcedReform;
    }

    // todo refactor mirroring above
    public PopUnit(PopUnit ipopUnit)
    {
        population = ipopUnit.population;
        type = ipopUnit.type;
        culture = ipopUnit.culture;


        storageNow = new Storage(Product.findByName("Food"), 0);
        gainGoodsThisTurn = new Storage(Product.findByName("Food"), 0);
        sentToMarket = new Storage(Product.findByName("Food"), 0);
        education = new Procent(ipopUnit.education.get());

        loyalty = new Procent(ipopUnit.loyalty.get());
        NeedsFullfilled = new Procent(ipopUnit.NeedsFullfilled.get());

        //owner = ipopUnit.getOwner();
        province = ipopUnit.province;
        //province.allPopUnits.Add(this);

        tempPopList.Add(this);
    }

    //internal float getSayYesProcent(AbstractReformValue selectedReformValue)
    //{
    //    return (uint)Mathf.RoundToInt(getSayingYes(selectedReformValue) / (float)population);
    //}

    public static PopUnit Instantiate(PopType type, PopUnit pop)
    {
        if (type == PopType.tribeMen) return new Tribemen(pop);
        else
        if (type == PopType.farmers) return new Farmers(pop);
        else
        if (type == PopType.aristocrats) return new Aristocrats(pop);
        else
        if (type == PopType.workers) return new Workers(pop);
        else
            if (type == PopType.capitalists) return new Capitalists(pop);
        else
        {
            Debug.Log("Unknow pop type!");
            return null;
        }
    }
    public static PopUnit Instantiate(uint iamount, PopType ipopType, Culture iculture, Province where)
    {

        if (ipopType == PopType.tribeMen) return new Tribemen(iamount, ipopType, iculture, where);
        else
        if (ipopType == PopType.farmers) return new Farmers(iamount, ipopType, iculture, where);
        else
        if (ipopType == PopType.aristocrats) return new Aristocrats(iamount, ipopType, iculture, where);
        else
        if (ipopType == PopType.workers) return new Workers(iamount, ipopType, iculture, where);
        else
            if (ipopType == PopType.capitalists) return new Capitalists(iamount, ipopType, iculture, where);
        else
        {
            Debug.Log("Unknow pop type!");
            return null;
        }
    }
    abstract internal bool getSayingYes(AbstractReformValue reform);
    public static uint getRandomPopulationAmount(int minGeneratedPopulation, int maxGeneratedPopulation)
    {
        uint randomPopulation = (uint)(minGeneratedPopulation + Game.random.Next(maxGeneratedPopulation - minGeneratedPopulation));
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
        Value multiplier = new Value(this.population / 1000f);

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
            uint employed = 0;
            foreach (Factory factory in province.allFactories)
                employed += factory.HowManyEmployed(this);
            if ((int)population - (int)employed <= 0) //happening due population change by growth/demotion
                return new Procent(0);
            return new Procent((population - employed) / (float)population);
        }
        else
            if (type == PopType.farmers || type == PopType.tribeMen)
        {
            float overPopulation = province.getOverPopulation();
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
    ////                uint overPopulation = province.getMenPopulation() - province.maxTribeMenCapacity;
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

            //taxSize = wallet.moneyIncomethisTurn.multiple(province.getOwner().countryTax);
            if (this.type.isPoorStrata())
            {
                taxSize = wallet.moneyIncomethisTurn.multiple((province.getOwner().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
                if (wallet.canPay(taxSize))
                {
                    province.getOwner().getCountryWallet().poorTaxIncomeAdd(taxSize);
                    wallet.pay(province.getOwner().wallet, taxSize);
                }
                else
                {
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

    public Procent getLifeNeedsFullfilling()
    {
        float need = NeedsFullfilled.get();
        if (need < 1f / 3f)
            return new Procent(NeedsFullfilled.get() * 3f);
        else
            return new Procent(1f);
    }
    public Procent getEveryDayNeedsFullfilling()
    {
        float need = NeedsFullfilled.get();
        if (need <= 1f / 3f)
            return new Procent(0f);
        if (need < 2f / 3f)
            return new Procent((NeedsFullfilled.get() - (1f / 3f)) * 3f);
        else
            return new Procent(1f);
    }

    public Procent getLuxuryNeedsFullfilling()
    {
        float need = NeedsFullfilled.get();
        if (need <= 2f / 3f)
            return new Procent(0f);
        if (need == 0.999f)
            return new Procent(1f);
        else
            return new Procent((NeedsFullfilled.get() - 0.666f) * 3f);

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
                    NeedsFullfilled.set(2f / 3f);
                    if (howDeep != 0) consumeEveryDayAndLuxury(getRealLuxuryNeeds(), 0.99f, howDeep);
                }
                else
                {
                    float canConsume = storageNow.get();
                    consumedTotal.add(storageNow);
                    storageNow.set(0);
                    NeedsFullfilled.add(canConsume / need.get() / 3f);
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
                    consumedInMarket.Set(need);
                    NeedsFullfilled.set(1f / 3f);
                    //consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 0.66f, 2);
                }
                else
                    NeedsFullfilled.set(Game.market.Consume(this, need, null).get() / 3f);
            }

        //if (NeedsFullfilled.get() > 0.33f) NeedsFullfilled.set(0.33f);

        if (getLifeNeedsFullfilling().get() >= 0.95f)
        {
            Wallet reserv = new Wallet(0);
            wallet.payWithoutRecord(reserv, wallet.haveMoney.multiple(Game.savePopMoneyReserv));
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
                NeedsFullfilled.add(spentMoney.get() / needsCost.get() / 3f);
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
                    NeedsFullfilled.add(spentMoney.get() / needsCost.get() / 3f);
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
                    NeedsFullfilled.set(1f / 3f);
                    consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 2f / 3f, 2);
                }
                else
                {
                    float canConsume = storageNow.get();
                    consumedTotal.Set(storageNow);
                    storageNow.set(0);
                    NeedsFullfilled.set(canConsume / need.get() / 3f);
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
        Game.market.tmpMarketStorage.SetZero();
        foreach (Country country in Country.allCountries)
            if (country != Country.NullCountry)
            {
                country.wallet.moneyIncomethisTurn.set(0);
                country.getCountryWallet().setSatisticToZero();
                country.aristocrstTax = country.serfdom.status.getTax();
                foreach (Province province in country.ownedProvinces)
                {
                    province.BalanceEmployableWorkForce();
                    {
                        foreach (PopUnit pop in province.allPopUnits)
                        {
                            pop.gainGoodsThisTurn.set(0f);
                            // pop.storageNow.set(0f);
                            pop.wallet.moneyIncomethisTurn.set(0f);

                            pop.consumedLastTurn.copyDataFrom(pop.consumedTotal); // temp
                            pop.NeedsFullfilled.set(0f);
                            pop.sentToMarket.set(0f);
                            pop.consumedTotal.SetZero();
                            pop.consumedInMarket.SetZero();

                            pop.dintGetUnemloymentSubsidy = false;
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
    //public void Growth(uint size)
    //{

    //}
    public void calcGrowth()
    {
        //uint growthSize = getGrowthSize();
        population = (uint)(population + getGrowthSize());
    }
    public void calcDemotions()
    {
        uint demotionSize = getDemotionSize();
        //&& CanDemote()
        if (WantsDemotion() && demotionSize > 0 && this.population > demotionSize)
            Demote(getRichestDemotionTarget(), demotionSize);
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
        List<PopLinkageValue> list = new List<PopLinkageValue>();
        foreach (PopType nextType in PopType.allPopTypes)
            if (CanThisDemoteInto(nextType))
                list.Add(new PopLinkageValue(nextType,
                    province.getMiddleNeedFullfilling(nextType)
                    ));
        list = list.OrderByDescending(o => o.amount.get()).ToList();
        if (list.Count == 0)
            return null;
        else
            if (list[0].amount.get() > this.NeedsFullfilled.get())
            return list[0].type;
        else return null;
    }
    abstract public bool CanThisDemoteInto(PopType popType);

    private void Demote(PopType type, uint amount)
    {
        //PopUnit newPop = new PopUnit(this);
        if (type != null)
        {
            PopUnit newPop = PopUnit.Instantiate(type, this);
            newPop.population = amount;
            newPop.type = type;
            this.population -= amount;
        }
    }

    internal void takeUnemploymentSubsidies()
    {
        var reform = province.getOwner().unemploymentSubsidies.getValue();
        if (getUnemployedProcent().get() > 0 && reform != UnemploymentSubsidies.None)
        {
            Value subsidy = getUnemployedProcent();
            subsidy.multipleInside(population / 1000f * (reform as UnemploymentSubsidies.LocalReformValue).getSubsidiesRate());
            //float subsidy = population / 1000f * getUnemployedProcent().get() * (reform as UnemploymentSubsidies.LocalReformValue).getSubsidiesRate();
            if (province.getOwner().wallet.canPay(subsidy))
            {
                province.getOwner().wallet.pay(this.wallet, subsidy);
                province.getOwner().getCountryWallet().unemploymentSubsidiesExpenseAdd(subsidy);
            }
            else
                this.dintGetUnemloymentSubsidy = true;

        }

    }

    public uint getDemotionSize()
    {
        return (uint)Mathf.RoundToInt(this.population * PopUnit.demotionSpeed.get());
    }
    public int getGrowthSize()
    {
        int result = 0;
        if (this.NeedsFullfilled.get() >= 0.33f) // positive grotwh
            result = Mathf.RoundToInt(PopUnit.growthSpeed.get() * population);
        else
            if (this.NeedsFullfilled.get() >= 0.20f) // zero grotwh
            result = 0;
        else if (type != PopType.farmers) //starvation  
        {
            result = Mathf.RoundToInt(PopUnit.starvationSpeed.get() * population * -1);
            if (result * -1 >= population) // total starvation
                result = 0;
        }

        return result;
        //return (uint)Mathf.RoundToInt(this.population * PopUnit.growthSpeed.get());
    }
    public bool WantsDemotion()
    {
        float demotionLimit = 0.333f;
        if (this.NeedsFullfilled.get() < demotionLimit)
            return true;
        else return false;
    }

    internal void Merge(PopUnit pop)
    {
        population = population + pop.population;
        //storage.value.add(pop.storage.value);
        //produced = new Storage(Product.findByName("Food"), 0);
        //education = new Procent(0.01f);
        //loyalty = new Procent(0.50f);
        //NeedsFullfilled = new Procent(1f);                
    }
    internal void Invest()
    {
        if (type == PopType.aristocrats)
        {
            if (!province.isThereMoreThanFactoriesInUpgrade(Game.maximumFactoriesInUpgradeToBuildNew))
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
                            && factory.getWorkForceFullFilling() > Game.minWorkforceFullfillingToUpgradeFactory
                            && factory.getMargin().get() >= Game.minMarginToUpgrade)
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
                !province.isThereMoreThanFactoriesInUpgrade(Game.maximumFactoriesInUpgradeToBuildNew))
            {
                FactoryType proposition = FactoryType.getMostTeoreticalProfitable(province);
                if (proposition != null)
                    if (province.CanBuildNewFactory(proposition) &&
                        (province.getUnemployed() > Game.minUnemploymentToBuldFactory || province.getMiddleFactoryWorkforceFullfilling() > Game.minFactoryWorkforceFullfillingToBuildNew))
                    {
                        PrimitiveStorageSet resourceToBuild = proposition.getBuildNeeds();
                        Value cost = Game.market.getCost(resourceToBuild);
                        cost.add(Game.factoryMoneyReservPerLevel);
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
                        && factory.getMargin().get() >= Game.minMarginToUpgrade
                        && factory.getWorkForceFullFilling() > Game.minWorkforceFullfillingToUpgradeFactory)
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
    public Tribemen(PopUnit pop) : base(pop)
    {
    }
    public Tribemen(uint iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
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
        float overpopulation = province.getOverPopulation();
        if (overpopulation <= 1) // all is ok
            producedAmount = new Value(population * type.basicProduction.get() / 1000f);
        else
            producedAmount = new Value(population * type.basicProduction.get() / 1000f / overpopulation);
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
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.8f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.2f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
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
    public Farmers(PopUnit pop) : base(pop)
    { }
    public Farmers(uint iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
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
        float overpopulation = province.getOverPopulation();
        if (overpopulation <= 1) // all is ok
            producedAmount = new Value(population * type.basicProduction.get() / 1000 + population * type.basicProduction.get() / 1000 * education.get());
        else
            producedAmount = new Value(population * type.basicProduction.get() / 1000 / overpopulation + population * type.basicProduction.get() / 1000 / overpopulation * education.get());
        gainGoodsThisTurn.set(producedAmount);

        if (Economy.isMarket.checkIftrue(province.getOwner()))
        {
            sentToMarket.set(gainGoodsThisTurn);
            Game.market.tmpMarketStorage.add(gainGoodsThisTurn);
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
                return baseOpinion.get() > Game.votingPassBillLimit;
            }
            else if (reform == Government.Aristocracy)
            {
                var baseOpinion = new Procent(0.2f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Game.votingPassBillLimit;
            }
            else if (reform == Government.Democracy)
            {
                var baseOpinion = new Procent(1f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Game.votingPassBillLimit;
            }
            else if (reform == Government.Despotism)
            {
                var baseOpinion = new Procent(0.2f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Game.votingPassBillLimit;
            }
            else if (reform == Government.ProletarianDictatorship)
            {
                var baseOpinion = new Procent(0.3f);
                baseOpinion.add(this.loyalty);
                return baseOpinion.get() > Game.votingPassBillLimit;
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
            return baseOpinion.get() > Game.votingPassBillLimit;
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
    public Aristocrats(PopUnit pop) : base(pop)
    { }
    public Aristocrats(uint iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
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
        if (storageNow.get() > Game.aristocratsFoodReserv)
        {
            Storage howMuchSend = new Storage(storageNow.getProduct(), storageNow.get() - Game.aristocratsFoodReserv);
            storageNow.pay(sentToMarket, howMuchSend);
            //sentToMarket.set(howMuchSend);
            Game.market.tmpMarketStorage.add(howMuchSend);
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
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.6f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
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
    public Capitalists(PopUnit pop) : base(pop)
    { }
    public Capitalists(uint iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
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
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.8f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.3f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.1f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
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
    public Workers(PopUnit pop) : base(pop)
    { }
    public Workers(uint iamount, PopType ipopType, Culture iculture, Province where) : base(iamount, ipopType, iculture, where)
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
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Aristocracy)
        {
            var baseOpinion = new Procent(0f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Democracy)
        {
            var baseOpinion = new Procent(0.6f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.Despotism)
        {
            var baseOpinion = new Procent(0.3f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
        }
        else if (reform == Government.ProletarianDictatorship)
        {
            var baseOpinion = new Procent(0.8f);
            baseOpinion.add(this.loyalty);
            return baseOpinion.get() > Game.votingPassBillLimit;
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
public class PopLinkageValue
{
    public PopType type;
    public Value amount;
    internal PopLinkageValue(PopType p, Value a)
    {
        type = p;
        amount = a;
    }
}