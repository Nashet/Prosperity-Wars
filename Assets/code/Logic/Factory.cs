using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
public class Factory : Producer
{
    //public enum PopTypes { Forestry, GoldMine, MetalMine };

    internal FactoryType type;
    protected static readonly int workForcePerLevel = 1000;
    protected int level = 0;
    /// <summary>shownFactory in a process of building - level 1 </summary>
    private bool building = true;
    private bool upgrading = false;
    private bool working = false;
    private bool toRemove = false;
    private bool dontHireOnSubsidies, subsidized;
    private byte priority = 0;
    protected Value salary = new Value(0);
    Agent factoryOwner;
    internal PrimitiveStorageSet needsToUpgrade;
    internal readonly PrimitiveStorageSet inputReservs = new PrimitiveStorageSet();

    protected List<PopLinkage> hiredWorkForce = new List<PopLinkage>();
    private static int xMoneyReservForResources = 10;
    private int daysInConstruction;
    private int daysUnprofitable;
    private int daysClosed;
    internal bool justHiredPeople;
    private int hiredLastTurn;
    internal readonly ConditionsList conditionsUpgrade, conditionsClose, conditionsReopen,
        conditionsDestroy, conditionsSell, conditionsBuy, conditionsNatinalize,
        conditionsSubsidize, conditionsDontHireOnSubsidies, conditionsChangePriority;

    internal ModifiersList modifierEfficiency;
    internal Modifier modifierHasResourceInProvince, modifierLevelBonus,
        modifierInventedMiningAndIsShaft, modifierBelongsToCountry, modifierIsSubsidised;
    internal Condition conNotBelongsToCountry;//, conIsBuilding;

    internal Factory(Province province, Agent factoryOwner, FactoryType type) : base(province.getCountry().bank)
    { //assuming this is level 0 building
        this.type = type;
        needsToUpgrade = this.type.getBuildNeeds();
        province.allFactories.Add(this);
        this.factoryOwner = factoryOwner;
        base.province = province;

        gainGoodsThisTurn = new Storage(this.type.basicProduction.getProduct());
        storageNow = new Storage(this.type.basicProduction.getProduct());
        sentToMarket = new Storage(this.type.basicProduction.getProduct());

        salary.set(base.province.getLocalMinSalary());


        modifierHasResourceInProvince = new Modifier(x => !this.type.isResourceGathering() && base.province.isProducingOnFactories(this.type.resourceInput),
           "Has input resource in this province", 20f, false);
        modifierLevelBonus = new Modifier(delegate () { return this.getLevel() - 1; }, "High production concentration bonus", 1f, false);
        modifierInventedMiningAndIsShaft = new Modifier(
             forWhom => (forWhom as Country).isInvented(Invention.Mining) && this.type.isShaft(),
           new StringBuilder("Invented ").Append(Invention.Mining.ToString()).ToString(), 50f, false);
        modifierBelongsToCountry = new Modifier(x => this.factoryOwner is Country, "Belongs to government", -20f, false);

        conNotBelongsToCountry = new Condition(
           x => !(this.factoryOwner is Country),
          "Doesn't belongs to government", false);
        modifierIsSubsidised = new Modifier((x) => isSubsidized(), "Is subsidized", -10f, false);
        modifierEfficiency = new ModifiersList(new List<Condition>
        {
            //x=>(x as Country).isInvented(InventionType.steamPower)
            new Modifier(Invention.SteamPowerInvented , 25f, false),
            new Modifier(Invention.CombustionEngine , 25f, false),
            modifierInventedMiningAndIsShaft,
            new Modifier(Economy.StateCapitalism,  10f, false),
            new Modifier(Economy.Interventionism,  30f, false),
            new Modifier(Economy.LaissezFaire,  50f, false),
            new Modifier(Economy.PlannedEconomy,  -10f, false),
            modifierHasResourceInProvince, modifierLevelBonus,
            modifierBelongsToCountry, modifierIsSubsidised
        });
        Condition factoryPlacedInOurCountry = new Condition((forWhom) => base.province.getCountry() == forWhom, "Enterprise placed in our country", false);
        conditionsUpgrade = new ConditionsList(new List<Condition>
        {
            new Condition(x=>  base.province.getCountry().economy.status != Economy.LaissezFaire || x is PopUnit, "Economy policy is not Laissez Faire", true),
             //Economy.isNotLF,
            new Condition(x=> !isUpgrading() , "Not upgrading", false),
            new Condition(x=>  !isBuilding(), "Not building", false),
            new Condition(x=> isWorking(), "Open", false),
            new Condition(x=> level != Options.maxFactoryLevel, "Max level not achieved", false),
            new Condition(delegate (System.Object forWhom)
            {
                Value cost = this.getUpgradeCost();
                return (forWhom as Agent).canPay(cost);
            }, delegate
            {
                Game.threadDangerSB.Clear();
                Game.threadDangerSB.Append("Have ").Append(getUpgradeCost()).Append(" coins");
                return Game.threadDangerSB.ToString();
            }, true)
                ,factoryPlacedInOurCountry
        });

        conditionsClose = new ConditionsList(new List<Condition>
        {
            new Condition(x=>  base.province.getCountry().economy.status != Economy.LaissezFaire || x is PopUnit, "Economy policy is not Laissez Faire", true),
            //Economy.isNotLF,
            new Condition(x=>  !isBuilding(),  "Not building", false),
            new Condition(x=>   isWorking(),  "Open", false),
            factoryPlacedInOurCountry
        });
        conditionsReopen = new ConditionsList(new List<Condition>
        {
            new Condition(x=>  base.province.getCountry().economy.status != Economy.LaissezFaire || x is PopUnit, "Economy policy is not Laissez Faire", true),
            //Economy.isNotLF,
            new Condition(x=>  !isBuilding(), "Not building", false),
            new Condition(x=> !isWorking(), "Close", false),
            new Condition(x=>  (x as Agent).canPay(getReopenCost()),  delegate () {
                    Game.threadDangerSB.Clear();
                    Game.threadDangerSB.Append("Have ").Append(getReopenCost()).Append(" coins");
                    return Game.threadDangerSB.ToString();
                }, true),
                factoryPlacedInOurCountry
        });
        conditionsDestroy = new ConditionsList(new List<Condition> { Economy.isNotLF, factoryPlacedInOurCountry });
        //status == Economy.LaissezFaire || status == Economy.Interventionism || status == Economy.NaturalEconomy
        conditionsSell = ConditionsList.IsNotImplemented; // !Planned and ! State fabricIsOur

        //(status == Economy.StateCapitalism || status == Economy.Interventionism || status == Economy.NaturalEconomy)
        conditionsBuy = ConditionsList.IsNotImplemented; // ! LF and !Planned fabricIsOur

        // (status == Economy.PlannedEconomy || status == Economy.NaturalEconomy || status == Economy.StateCapitalism)
        conditionsNatinalize = new ConditionsList(new List<Condition>
        {
            Economy.isNotLF, Economy.isNotInterventionism, conNotBelongsToCountry, factoryPlacedInOurCountry
        }); //!LF and ! Inter


        conditionsSubsidize = new ConditionsList(new List<Condition>
        {
            Economy.isNotLF, Economy.isNotNatural, factoryPlacedInOurCountry
        });

        conditionsDontHireOnSubsidies = new ConditionsList(new List<Condition>
        {
            Economy.isNotLF, Economy.isNotNatural, Condition.IsNotImplemented,factoryPlacedInOurCountry
        });

        conditionsChangePriority = new ConditionsList(new List<Condition> { Economy.isNotLF, Condition.IsNotImplemented, factoryPlacedInOurCountry });


    }
    internal PrimitiveStorageSet getUpgradeNeeds()
    {
        if (getLevel() < Options.FactoryMediumTierLevels)
            return type.upgradeResourceLowTier;
        else
            if (getLevel() < Options.FactoryMediumHighLevels)
            return type.upgradeResourceMediumTier;
        else
            return type.upgradeResourceHighTier;
    }
    internal float getPriority()
    {
        return priority;
    }

    internal bool isDontHireOnSubsidies()
    {
        return dontHireOnSubsidies;
    }

    internal bool isSubsidized()
    {
        return subsidized;
    }

    internal Procent getResouceFullfillig()
    {
        return new Procent(getInputFactor());
    }
    internal int getLevel()
    {
        return level;
    }
    internal bool isUpgrading()
    {
        return upgrading;//building ||
    }
    internal bool isBuilding()
    {
        return building;
    }
    override public string ToString()
    {
        return type.name + " L" + getLevel();
    }
    internal Agent getOwner()
    {
        return factoryOwner;
    }
    public void setOwner(Agent agent)
    {
        factoryOwner = agent;
    }
    //abstract internal string getName();
    public override void simulate()
    {
        //hireWorkForce();
        //produce();
        //payTaxes();
        //paySalary();
        //consume();

    }
    /// <summary>  Return in pieces basing on current prices and needs  /// </summary>        
    override internal float getLocalEffectiveDemand(Product product)
    {

        // need to know how much i Consumed inside my needs
        Storage need = type.resourceInput.findStorage(product);
        if (need != null)
        {
            Storage realNeed = new Storage(need.getProduct(), need.get() * getWorkForceFullFilling());
            //Storage realNeed = new Storage(need.getProduct(), need.get() * getInputFactor());
            Storage canAfford = HowMuchCanAfford(realNeed);
            return canAfford.get();
        }
        else return 0f;
    }



    /// <summary>
    /// Should optimize? Seek for changes..
    /// </summary>
    /// <returns></returns>
    public int getWorkForce()
    {
        int result = 0;
        foreach (PopLinkage pop in hiredWorkForce)
            result += pop.amount;
        return result;
    }
    /// <summary>
    /// returns how much factory hired in reality
    /// </summary>    
    public int hireWorkforce(int amount, List<PopUnit> popList)
    {
        //check on no too much workers?
        //if (amount > HowMuchWorkForceWants())
        //    amount = HowMuchWorkForceWants();
        int wasWorkforce = getWorkForce();
        hiredWorkForce.Clear();
        if (amount > 0)
        {
            int leftToHire = amount;
            foreach (PopUnit pop in popList)
            {
                if (pop.getPopulation() >= leftToHire) // satisfied demand
                {
                    hiredWorkForce.Add(new PopLinkage(pop, leftToHire));
                    hiredLastTurn = getWorkForce() - wasWorkforce;
                    return hiredLastTurn;
                    //break;
                }
                else
                {
                    hiredWorkForce.Add(new PopLinkage(pop, pop.getPopulation())); // hire as we can
                    leftToHire -= pop.getPopulation();
                }
            }
            hiredLastTurn = getWorkForce() - wasWorkforce;
            return hiredLastTurn;
        }
        else return 0;
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        storageNow.set(0f);
    }
    internal void setDontHireOnSubsidies(bool isOn)
    {
        dontHireOnSubsidies = isOn;
    }

    internal void setSubsidized(bool isOn)
    {
        subsidized = isOn;
    }

    internal void setPriority(byte value)
    {
        priority = value;
    }

    internal int HowManyEmployed(PopUnit pop)
    {
        int result = 0;
        foreach (PopLinkage link in hiredWorkForce)
            if (link.pop == pop)
                result += link.amount;
        return result;
    }

    internal bool IsThereMoreWorkersToHire()
    {
        int totalAmountWorkers = province.getPopulationAmountByType(PopType.Workers);
        int result = totalAmountWorkers - getWorkForce();
        return (result > 0);
    }
    internal int getFreeJobSpace()
    {
        return getMaxWorkforceCapacity() - getWorkForce();
    }
    internal bool ThereIsPossibilityToHireMore()
    {
        //if there is other pops && there is space on factory     

        if (getFreeJobSpace() > 0 && IsThereMoreWorkersToHire())
            return true;
        else
            return false;

    }

    internal void paySalary()
    {
        //if (getLevel() > 0)
        if (isWorking())
        {
            // per 1000 men            
            if (Economy.isMarket.checkIftrue(province.getCountry()))
            {
                foreach (PopLinkage link in hiredWorkForce)
                {
                    Value howMuchPay = new Value(0);
                    howMuchPay.set(salary.get() * link.amount / 1000f);
                    if (canPay(howMuchPay))
                        pay(link.pop, howMuchPay);
                    else
                        if (isSubsidized()) //take money and try again
                    {
                        province.getCountry().takeFactorySubsidies(this, HowMuchMoneyCanNotPay(howMuchPay));
                        if (canPay(howMuchPay))
                            pay(link.pop, howMuchPay);
                        else
                            salary.set(province.getCountry().getMinSalary());
                    }
                    else
                        salary.set(province.getCountry().getMinSalary());
                    //todo else dont pay if there is nothing to pay
                }
            }
            else
            {
                // non market!!
                Storage foodSalary = new Storage(Product.Food, 1f);
                foreach (PopLinkage link in hiredWorkForce)
                {
                    Storage howMuchPay = new Storage(foodSalary.getProduct(), foodSalary.get() * link.amount / 1000f);
                    if (factoryOwner is Country)
                    {
                        Country payer = factoryOwner as Country;

                        if (payer.storageSet.has(howMuchPay))
                        {
                            payer.storageSet.send(link.pop, howMuchPay);
                            link.pop.gainGoodsThisTurn.add(howMuchPay);
                            salary.set(foodSalary);
                        }
                        //todo no salary cuts yet
                        //else salary.set(0);
                    }
                    else // assuming - PopUnit
                    {
                        PopUnit payer = factoryOwner as PopUnit;

                        if (payer.storageNow.has(link.pop.storageNow, howMuchPay))
                        {
                            payer.storageNow.send(link.pop.storageNow, howMuchPay);
                            link.pop.gainGoodsThisTurn.add(howMuchPay);
                            salary.set(foodSalary);
                        }
                        //todo no resiuces tio pay salary
                        //else salary.set(0);
                    }
                    //else dont pay if there is nothing to pay
                }
            }
        }
    }
    internal float getProfit()
    {
        float z = moneyIncomethisTurn.get() - getConsumedCost() - getSalaryCost();
        return z;
    }
    internal Procent getMargin()
    {
        float x = getProfit() / (getUpgradeCost().get() * level);
        return new Procent(x);
    }
    internal Value getReopenCost()
    {
        return new Value(Options.factoryMoneyReservPerLevel);

    }
    internal Value getUpgradeCost()
    {
        Value result = Game.market.getCost(getUpgradeNeeds());
        result.add(Options.factoryMoneyReservPerLevel);
        return result;
        //return Game.market.getCost(type.getUpgradeNeeds());
    }
    internal float getConsumedCost()
    {
        float result = 0f;
        foreach (Storage stor in consumedTotal)
            result += stor.get() * Game.market.findPrice(stor.getProduct()).get();
        return result;
    }
    /// <summary>
    /// Feels storageNow and gainGoodsThisTurn
    /// </summary>
    public override void produce()
    {
        if (isWorking())
        {
            int workers = getWorkForce();
            if (workers > 0)
            {
                Value producedAmount;
                producedAmount = new Value(type.basicProduction.get() * getEfficiency(true).get() * getLevel()); // * getLevel());

                storageNow.add(producedAmount);
                gainGoodsThisTurn.set(producedAmount);
                consumeInputResources(getRealNeeds());

                if (type == FactoryType.GoldMine)
                {
                    this.ConvertFromGoldAndAdd(storageNow);
                    //send 50% to government
                    Value sentToGovernment = new Value(moneyIncomethisTurn.get() * Options.GovernmentTakesShareOfGoldOutput);
                    pay(province.getCountry(), sentToGovernment);
                    province.getCountry().goldMinesIncomeAdd(sentToGovernment);
                }
                else
                {
                    sentToMarket.set(gainGoodsThisTurn);
                    storageNow.set(0f);
                    Game.market.sentToMarket.add(gainGoodsThisTurn);
                }

                //if (province.getOwner().isInvented(InventionType.capitalism))
                if (Economy.isMarket.checkIftrue(province.getCountry()))
                {
                    // Buyers should come and buy something...
                    // its in other files.
                }
                else // send all production to owner
                    ; // todo write ! capitalism
                      //storageNow.sendAll(owner.storageSet);
            }
        }
    }

    private void consumeInputResources(List<Storage> list)
    {
        foreach (Storage next in list)
        {
            inputReservs.subtract(next, false);
            //var storage = inputReservs.findStorage(next.getProduct());
            //if (storage != null)
            //    if (storage.isBiggerOrEqual(next))
            //        storage.subtract(next);
            //    else
            //        storage.setZero();
        }
    }



    /// <summary> only make sense if called before HireWorkforce()
    ///  PEr 1000 men!!!
    /// !!! Mirroring PaySalary
    /// </summary>    
    public float getSalary()
    {
        return salary.get();
    }
    public int getMaxWorkforceCapacity()
    {
        int cantakeMax = level * workForcePerLevel;
        return cantakeMax;
    }
    internal void changeSalary()
    {
        //if (getLevel() > 0)
        if (isWorking() && Economy.isMarket.checkIftrue(province.getCountry()))

        {
            // rise salary to attract  workforce, including workforce from other factories
            if (ThereIsPossibilityToHireMore() && getMargin().get() > Options.minMarginToRiseSalary)// && getInputFactor() == 1)
                salary.add(0.03f);

            //too allocate workers form other popTypes
            //if (getFreeJobSpace() > 100 && province.getPopulationAmountByType(PopType.Workers) < 600
            //    && getMargin().get() > Options.minMarginToRiseSalary && getInputFactor() == 1)// in that case float can store 1 exactly
            //    salary.add(0.01f);

            // to help factories catch up other factories salaries
            //if (getWorkForce() <= 100 && province.getUnemployed() == 0 && this.wallet.haveMoney.get() > 10f)
            //    salary.set(province.getLocalMinSalary());
            // freshly built factories should rise salary to concurrency with old ones
            if (getWorkForce() < 100 && province.getUnemployedWorkers() == 0 && this.cash.get() > 10f)// && getInputFactor() == 1)
                salary.add(0.09f);

            float minSalary = province.getCountry().getMinSalary();
            // reduce salary on non-profit
            if (getProfit() < 0f && daysUnprofitable >= Options.minDaysBeforeSalaryCut && !justHiredPeople && !isSubsidized())
                if (salary.get() - 0.3f >= minSalary)
                    salary.subtract(0.3f);
                else
                    salary.set(minSalary);

            // check if country's min wage changed
            if (salary.get() < minSalary)
                salary.set(minSalary);

        }
    }

    int getMaxHiringSpeed()
    {
        return Options.maxFactoryFireHireSpeed * getLevel();
    }
    /// <summary>
    /// max - max capacity
    /// </summary>    
    public int HowMuchWorkForceWants()
    {
        //if (getLevel() == 0) return 0;
        if (!isWorking()) return 0;
        int wants = Mathf.RoundToInt(getMaxWorkforceCapacity());// * getInputFactor());

        int difference = wants - getWorkForce();

        int maxHiringSpeed = getMaxHiringSpeed();
        // clamp difference in Options.maxFactoryFireHireSpeed []
        if (difference > maxHiringSpeed)
            difference = maxHiringSpeed;
        else
            if (difference < -1 * maxHiringSpeed) difference = -1 * maxHiringSpeed;

        //fire people if no enough input. getHowMuchHiredLastTurn() - to avoid last turn input error
        if (difference > 0 && !justHiredPeople && getInputFactor() < 0.95f && !(getHowMuchHiredLastTurn() > 0) && !isSubsidized())// && getWorkForce() >= Options.maxFactoryFireHireSpeed)
            difference = -1 * maxHiringSpeed;

        //fire people if unprofitable. 
        if (difference > 0 && (getProfit() < 0f) && !justHiredPeople && daysUnprofitable >= Options.minDaysBeforeSalaryCut && !isSubsidized())// && getWorkForce() >= Options.maxFactoryFireHireSpeed)
            difference = -1 * maxHiringSpeed;

        // just don't hire more..
        if (difference > 0 && (getProfit() < 0f || getInputFactor() < 0.95f) && !isSubsidized())
            difference = 0;

        //todo optimize getWorkforce()
        int result = getWorkForce() + difference;
        if (result < 0) return 0;
        return result;
    }
    internal int getHowMuchHiredLastTurn()
    {
        return hiredLastTurn;
    }
    public override void payTaxes() // currently no taxes for factories
    {

    }
    internal float getInputFactor()
    {
        float inputFactor = 1;
        List<Storage> realInput = new List<Storage>();
        //Storage available;

        // how much we really want
        foreach (Storage input in type.resourceInput)
        {
            realInput.Add(new Storage(input.getProduct(), input.get() * getWorkForceFullFilling()));
        }

        // checking if there is enough in market
        //old DSB
        //foreach (Storage input in realInput)
        //{
        //    available = Game.market.HowMuchAvailable(input);
        //    if (available.get() < input.get())
        //        input.set(available);
        //}
        foreach (Storage input in realInput)
        {
            if (!inputReservs.has(input))
            {
                Storage found = inputReservs.findStorage(input.getProduct());
                if (found == null)
                    input.set(0f);
                else
                    input.set(found);

            }
        }
        //old last turn consumption checking thing
        //foreach (Storage input in realInput)
        //{

        //    //if (Game.market.getDemandSupplyBalance(input.getProduct()) >= 1f)
        //    //available = input

        //    available = consumedLastTurn.findStorage(input.getProduct());
        //    if (available == null)
        //        ;// do nothing - pretend there is 100%, it fires only on shownFactory start
        //    else
        //    if (!justHiredPeople && available.get() < input.get())
        //        input.set(available);
        //}
        // checking if there is enough money to pay for
        // doesn't have sense with inputReserv
        //foreach (Storage input in realInput)
        //{
        //    Storage howMuchCan = wallet.HowMuchCanAfford(input);
        //    input.set(howMuchCan.get());
        //}
        // searching lowest factor
        foreach (Storage rInput in realInput)//todo optimize - convert into for i
        {
            float newFactor = rInput.get() / (type.resourceInput.findStorage(rInput.getProduct()).get() * getWorkForceFullFilling());
            if (newFactor < inputFactor)
                inputFactor = newFactor;
        }

        return inputFactor;
    }
    /// <summary>
    /// per 1000 men    
    /// </summary>
    /// <returns></returns>
    internal float getWorkForceFullFilling()
    {
        return getWorkForce() / (1000f * level);
    }
    /// <summary>
    /// per level
    /// </summary>    
    internal Procent getEfficiency(bool useBonuses)
    {
        //limit production by smalest factor
        float efficencyFactor = 0;
        float workforceProcent = getWorkForceFullFilling();
        float inputFactor = getInputFactor();
        if (inputFactor < workforceProcent) efficencyFactor = inputFactor;
        else efficencyFactor = workforceProcent;
        //float basicEff = efficencyFactor * getLevel();
        //Procent result = new Procent(basicEff);
        Procent result = new Procent(efficencyFactor);
        if (useBonuses)
            result.set(result.get() * (1f + modifierEfficiency.getModifier(province.getCountry()) / 100f));
        return result;
    }

    public List<Storage> getHowMuchReservesWants()
    {
        Value multiplier = new Value(getWorkForceFullFilling() * getLevel() * Options.factoryInputReservInDays);



        List<Storage> result = new List<Storage>();

        foreach (Storage next in type.resourceInput)
        {
            Storage howMuchWantBuy = new Storage(next.getProduct(), next.get());
            howMuchWantBuy.multiple(multiplier);
            Storage reserv = inputReservs.findStorage(next.getProduct());
            if (reserv == null)
                result.Add(howMuchWantBuy);
            else
                if (howMuchWantBuy.has(reserv))
            {
                howMuchWantBuy.subtract(reserv);
                result.Add(howMuchWantBuy);
            }//else  - there is enough reservs, don't buy that



        }
        return result;
    }
    // Should remove market availability assamption since its goes to double- calculation?
    public List<Storage> getRealNeeds()
    {
        Value multiplier = new Value(getEfficiency(false).get() * getLevel());

        List<Storage> result = new List<Storage>();

        foreach (Storage next in type.resourceInput)
        {
            Storage nStor = new Storage(next.getProduct(), next.get());
            nStor.multiple(multiplier);
            result.Add(nStor);
        }
        return result;
    }
    /// <summary>
    /// Now includes workforce/efficineneece. Here also happening buying dor upgrading\building
    /// </summary>
    override public void buyNeeds()
    {
        //if (getLevel() > 0)
        if (isWorking())
        {
            List<Storage> shoppingList = getHowMuchReservesWants();

            //todo !CAPITALISM part
            if (isSubsidized())
                Game.market.buy(this, new PrimitiveStorageSet(shoppingList), province.getCountry());
            else
                Game.market.buy(this, new PrimitiveStorageSet(shoppingList), null);
        }
        if (isUpgrading() || isBuilding())
        {
            bool isBuyingComplete = false;
            daysInConstruction++;
            bool isMarket = Economy.isMarket.checkIftrue(province.getCountry());// province.getOwner().isInvented(InventionType.capitalism);
            if (isMarket)
            {
                if (isBuilding())
                    isBuyingComplete = Game.market.buy(this, needsToUpgrade, Options.BuyInTimeFactoryUpgradeNeeds, type.getBuildNeeds());
                else
                    if (isUpgrading())
                    isBuyingComplete = Game.market.buy(this, needsToUpgrade, Options.BuyInTimeFactoryUpgradeNeeds, getUpgradeNeeds());
                // what if there is no enough money to complete buildinG?
                float minimalFond = cash.get() - 50f;

                if (minimalFond < 0 && getOwner().canPay(new Value(minimalFond * -1f)))
                    getOwner().payWithoutRecord(this, new Value(minimalFond * -1f));
            }
            if (isBuyingComplete || (!isMarket && daysInConstruction == Options.fabricConstructionTimeWithoutCapitalism))
            {
                level++;
                building = false;
                upgrading = false;
                needsToUpgrade.setZero();
                daysInConstruction = 0;
                inputReservs.subtract(type.getBuildNeeds(), false);
                inputReservs.subtract(getUpgradeNeeds(), false);

                reopen(this);
            }
            else if (daysInConstruction == Options.maxDaysBuildingBeforeRemoving)
                if (isBuilding())
                    markToDestroy();
                else // upgrading
                    stopUpgrading();

        }
    }
    private void stopUpgrading()
    {
        building = false;
        upgrading = false;
        needsToUpgrade.setZero();
        daysInConstruction = 0;
    }
    internal void markToDestroy()
    {
        toRemove = true;

        //return loans only if banking invented
        if (province.getCountry().isInvented(Invention.Banking))
        {
            if (loans.get() > 0f)
            {
                Value howMuchToReturn = new Value(loans);
                if (howMuchToReturn.get() <= cash.get())
                    howMuchToReturn.set(cash);
                province.getCountry().bank.takeMoney(this, howMuchToReturn);
                if (loans.get() > 0f)
                    province.getCountry().bank.defaultLoaner(this);
            }
        }
        sendAllAvailableMoney(getOwner());
        MainCamera.factoryPanel.removeFactory(this);

    }
    internal void destroyImmediately()
    {
        markToDestroy();
        province.allFactories.Remove(this);
        //province.allFactories.Remove(this);        
        // + interface 2 places
        MainCamera.factoryPanel.removeFactory(this);
        MainCamera.productionWindow.removeFactory(this);
    }
    internal bool isToRemove()
    {
        return toRemove;
    }

    float wantsMinMoneyReserv()
    {
        return getExpences() * Factory.xMoneyReservForResources + Options.factoryMoneyReservPerLevel * level;
    }
    internal void PayDividend()
    {
        //if (getLevel() > 0)
        if (isWorking())
        {
            float saveForYourSelf = wantsMinMoneyReserv();
            float divident = cash.get() - saveForYourSelf;

            if (divident > 0)
            {
                Value sentToOwner = new Value(divident);
                pay(getOwner(), sentToOwner);
                var owner = factoryOwner as Country;
                if (owner != null)
                    owner.ownedFactoriesIncomeAdd(sentToOwner);
            }

            if (getProfit() <= 0) // to avoid internal zero profit factories
            {
                daysUnprofitable++;
                if (daysUnprofitable == Options.maxDaysUnprofitableBeforeFactoryClosing && !isSubsidized())
                    this.close();
            }
            else
                daysUnprofitable = 0;
        }
        else //closed
        if (!isBuilding())
        {
            daysClosed++;
            if (daysClosed == Options.maxDaysClosedBeforeRemovingFactory)
                markToDestroy();
            else if (Game.Random.Next(Options.howOftenCheckForFactoryReopenning) == 1)
            {//take loan for reopen
                if (province.getCountry().isInvented(Invention.Banking) && this.type.getPossibleProfit(province).get() > 10f)
                {
                    float leftOver = cash.get() - wantsMinMoneyReserv();
                    if (leftOver < 0)
                    {
                        Value loanSize = new Value(leftOver * -1f);
                        if (province.getCountry().bank.canGiveMoney(this, loanSize))
                            province.getCountry().bank.giveMoney(this, loanSize);
                    }
                    leftOver = cash.get() - wantsMinMoneyReserv();
                    if (leftOver >= 0f)
                        reopen(this);
                }
            }
        }
    }

    //public bool isClosed()
    //{
    //    return !working;
    //}

    internal void close()
    {
        working = false;
        upgrading = false;
        needsToUpgrade.setZero();
        daysInConstruction = 0;
    }
    internal void reopen(Agent byWhom)
    {
        working = true;
        if (daysUnprofitable > 20)
            salary.set(province.getLocalMinSalary());
        daysUnprofitable = 0;
        daysClosed = 0;
        if (byWhom != this)
            byWhom.payWithoutRecord(this, getReopenCost());

    }

    internal bool isWorking()
    {
        return working && !building;
    }

    internal float getExpences()
    {
        return getConsumedCost() + getSalaryCost();
    }

    internal float getSalaryCost()
    {
        return getWorkForce() * getSalary() / 1000f;
    }

    internal bool canUpgrade()
    {
        return !isUpgrading() && !isBuilding() && level < Options.maxFactoryLevel && isWorking();
    }
    internal void upgrade(Agent byWhom)
    {
        upgrading = true;
        needsToUpgrade = getUpgradeNeeds().getCopy();
        byWhom.payWithoutRecord(this, getUpgradeCost());
    }

    internal int getDaysInConstruction()
    {
        return daysInConstruction;
    }

    internal int getDaysUnprofitable()
    {
        return daysUnprofitable;
    }
    internal int getDaysClosed()
    {
        return daysClosed;



    }


}
/// <summary>
/// ///////////////////////////////////////////////////////////////////****************
/// </summary>
//public class GoldMine : Factory
//{
//    public static string name = "Gold mine";
//    public GoldMine(Province iprovince, Owner inowner) : base(iprovince, inowner, FactoryType.GoldMine)
//    {

//    }
//    public override void produce()
//    {
//       
//        int workers = getWorkForce();
//        if (workers > 0)
//        {
//            Value producedAmount;
//            producedAmount = new Value(getWorkForce() * type.basicProduction.get() / 1000f);
//            storageNow.add(producedAmount);
//            gainGoodsThisTurn.set(producedAmount);

//            if (province.getOwner().capitalism.Invented())
//            {

//                this.wallet.ConvertFromGoldAndAdd(storageNow);
//                //send 50% to government

//                wallet.pay(province.getOwner().wallet, new Value(wallet.moneyIncomethisTurn.get() / 2f));
//            }
//            else // send all production to owner
//                storageNow.sendAll(province.getOwner().storageSet);
//        }
//    }

//}
//public class Forestry : Factory
//{
//    public static string name = "Forestry";
//    public Forestry(Province iprovince, Owner inowner) : base(iprovince, inowner)
//    {
//        type.basicProduction = new Storage(Product.findByName("Wood"), 2f);
//        gainGoodsThisTurn = new Storage(Product.findByName("Wood"));
//        //type = PopTypes.Forestry;
//        storageNow = new Storage(Product.findByName("Wood"));
//    }
//    internal override string getName()
//    {
//        return "Forestry";
//    }
//}
//public class Furniture : Factory
//{
//    public static string name = "Furniture";
//    public Furniture(Province iprovince, Owner inowner) : base(iprovince, inowner)
//    {
//        type.basicProduction = new Storage(Product.Furniture, 4f);
//        gainGoodsThisTurn = new Storage(Product.Furniture);
//        storageNow = new Storage(Product.Furniture);
//        type.resourceInput.Set(new Storage(Product.Lumber, 1f));
//    }
//    internal override string getName()
//    {
//        return "Furniture shownFactory";
//    }
//}
//public class Sawmill : Factory
//{
//    public static string name = "Sawmill";
//    public Sawmill(Province iprovince, Owner inowner) : base(iprovince, inowner)
//    {
//        type.basicProduction = new Storage(Product.Lumber, 2f);
//        storageNow = new Storage(Product.Lumber);
//        gainGoodsThisTurn = new Storage(Product.Lumber);
//        type.resourceInput.Set(new Storage(Product.Wood, 1f));
//    }
//    internal override string getName()
//    {
//        return "Sawmill";
//    }
//}

