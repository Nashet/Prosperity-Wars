using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
public class Factory : SimpleProduction, ICanBeCellInTable
{
    public enum Priority { none, low, medium, high }
    private static readonly int workForcePerLevel = 1000;
    private static int xMoneyReservForResources = 10;

    private int level = 0;
    private bool building = true;
    private bool upgrading = false;
    private bool working = false;
    private bool toRemove = false;

    private bool dontHireOnSubsidies, subsidized;
    private int priority = 0;
    private readonly Value salary = new Value(0);

    /// <summary>
    /// How much need to finish building/upgrading
    /// </summary>
    internal readonly StorageSet constructionNeeds;


    private readonly Dictionary<PopUnit, int> hiredWorkForce = new Dictionary<PopUnit, int>();

    private int daysInConstruction;
    private int daysUnprofitable;
    private int daysClosed;
    private bool justHiredPeople = true;
    private int hiredLastTurn;

    internal static readonly Modifier
        modifierHasResourceInProvince = new Modifier(x => !(x as Factory).getType().isResourceGathering() &&
        ((x as Factory).getProvince().isProducingOnEnterprises((x as Factory).getType().resourceInput)
        || ((x as Factory).getProvince().getResource() == Product.Grain && (x as Factory).getType() == FactoryType.Barnyard)
        ),
              "Has input resource in this province", 0.20f, false),

        modifierLevelBonus = new Modifier(x => ((x as Factory).getLevel() - 1) / 100f, "High production concentration bonus", 1f, false),

        modifierInventedMiningAndIsShaft = new Modifier(x => (x as Factory).getCountry().isInvented(Invention.Mining) && (x as Factory).getType().isShaft(),
           new StringBuilder("Invented ").Append(Invention.Mining.ToString()).ToString(), 0.50f, false),

        modifierBelongsToCountry = new Modifier(x => (x as Factory).getOwner() is Country, "Belongs to government", -0.20f, false),
        modifierIsSubsidised = new Modifier((x) => (x as Factory).isSubsidized(), "Is subsidized", -0.10f, false);

    internal static readonly Condition
        conNotBelongsToCountry = new Condition(x => !((x as Factory).getOwner() is Country), "Doesn't belongs to government", false),
        conNotUpgrading = new Condition(x => !(x as Factory).isUpgrading(), "Not upgrading", false),
        conNotBuilding = new Condition(x => !(x as Factory).isBuilding(), "Not building", false),
        conOpen = new Condition(x => (x as Factory).isWorking(), "Open", false),
        conClosed = new Condition(x => !(x as Factory).isWorking(), "Closed", false),
        conMaxLevelAchieved = new Condition(x => (x as Factory).getLevel() != Options.maxFactoryLevel, "Max level not achieved", false),

        conPlayerHaveMoneyToReopen = new Condition(x => Game.Player.canPay((x as Factory).getReopenCost()), delegate (object x)
        {
            var sb = new StringBuilder();
            sb.Append("Have ").Append((x as Factory).getReopenCost()).Append(" coins");
            return sb.ToString();
        }, true);
    internal static readonly ConditionForDoubleObjects
        conHaveMoneyOrResourcesToUpgrade = new ConditionForDoubleObjects(
            //(factory, agent) => (agent as Agent).canPay((factory as Factory).getUpgradeCost()),

            delegate (object factory, object upgrader)
            {
                var agent = upgrader as Agent;
                var typedfactory = factory as Factory;
                if (agent.getCountry().economy.getValue() == Economy.PlannedEconomy)
                {
                    return agent.getCountry().countryStorageSet.has(typedfactory.getUpgradeNeeds());
                }
                else
                {
                    Value cost = Game.market.getCost(typedfactory.getUpgradeNeeds());
                    return agent.canPay(cost);
                }
            },


            delegate (object x)
            {
                var sb = new StringBuilder();
                var factory = x as Factory;
                Value cost = Game.market.getCost(factory.getUpgradeNeeds());
                sb.Append("Have ").Append(cost).Append(" coins");
                sb.Append(" or (with ").Append(Economy.PlannedEconomy).Append(") have ").Append(factory.getUpgradeNeeds());
                return sb.ToString();
            }
            , true),
        conPlacedInOurCountry = new ConditionForDoubleObjects((factory, agent) => (factory as Factory).getCountry() == (agent as Consumer).getCountry(),
        (factory) => "Enterprise placed in our country", true),
        conNotLForNotCountry = new ConditionForDoubleObjects((factory, agent) => (factory as Factory).getCountry().economy.getValue() != Economy.LaissezFaire || !(agent is Country), (factory) => "Economy policy is not Laissez Faire", true)
        ;

    internal static readonly ConditionsListForDoubleObjects
        conditionsUpgrade = new ConditionsListForDoubleObjects(new List<Condition>
        {
            conNotUpgrading, conNotBuilding, conOpen, conMaxLevelAchieved, conNotLForNotCountry,
            conHaveMoneyOrResourcesToUpgrade, conPlacedInOurCountry
        }),
        conditionsClose = new ConditionsListForDoubleObjects(new List<Condition> { conNotBuilding, conOpen, conPlacedInOurCountry, conNotLForNotCountry }),
        conditionsReopen = new ConditionsListForDoubleObjects(new List<Condition> { conNotBuilding, conClosed, conPlayerHaveMoneyToReopen, conPlacedInOurCountry, conNotLForNotCountry }),
        conditionsDestroy = new ConditionsListForDoubleObjects(new List<Condition> {
            //new Condition(Economy.isNotLF, x=>(x as Producer).getCountry()),
             conPlacedInOurCountry }).addForSecondObject(new List<Condition> { Economy.isNotLF }),

    // (status == Economy.PlannedEconomy || status == Economy.NaturalEconomy || status == Economy.StateCapitalism)
        conditionsNatinalize = new ConditionsListForDoubleObjects(new List<Condition> { conNotBelongsToCountry, conPlacedInOurCountry })
        .addForSecondObject(new List<Condition> { Economy.isNotLF, Economy.isNotInterventionism }),

        conditionsSubsidize = new ConditionsListForDoubleObjects(new List<Condition> { conPlacedInOurCountry })
        .addForSecondObject(new List<Condition> { Economy.isNotLF, Economy.isNotNatural, Economy.isNotPlanned }),

        conditionsDontHireOnSubsidies = new ConditionsListForDoubleObjects(new List<Condition> { conPlacedInOurCountry })
        .addForSecondObject(new List<Condition> { Economy.isNotLF, Economy.isNotNatural, Condition.IsNotImplemented }),

        conditionsChangePriority = new ConditionsListForDoubleObjects(new List<Condition> { conPlacedInOurCountry })
        .addForSecondObject(new List<Condition> { Economy.isPlanned });//.isNotLF
    internal static readonly ConditionsList

        //status == Economy.LaissezFaire || status == Economy.Interventionism || status == Economy.NaturalEconomy
        conditionsSell = new ConditionsList(Condition.IsNotImplemented),

        // !Planned and ! State fabricIsOur
        //(status == Economy.StateCapitalism || status == Economy.Interventionism || status == Economy.NaturalEconomy)
        conditionsBuy = new ConditionsList(Condition.IsNotImplemented) // ! LF and !Planned fabricIsOur
        ;

    internal static readonly ModifiersList
        modifierEfficiency = new ModifiersList(new List<Condition>
        {
           Modifier.modifierDefault1,
            new Modifier(Invention.SteamPowerInvented, x => (x as Factory).getCountry(), 0.25f, false),
            new Modifier(Invention.CombustionEngineInvented, x => (x as Factory).getCountry(), 0.25f, false),

            new Modifier(Economy.isStateCapitlism, x => (x as Factory).getCountry(),  0.10f, false),
            new Modifier(Economy.isInterventionism, x => (x as Factory).getCountry(),  0.30f, false),
            new Modifier(Economy.isLF, x => (x as Factory).getCountry(), 0.50f, false),
            new Modifier(Economy.isPlanned, x => (x as Factory).getCountry(), -0.10f, false),

            modifierInventedMiningAndIsShaft,
            modifierHasResourceInProvince,
            modifierLevelBonus, modifierBelongsToCountry, modifierIsSubsidised,
            // copied in popUnit
             new Modifier(x => Government.isPolis.checkIftrue((x as Factory).getCountry())
             && (x as Factory).getProvince().isCapital(), "Capital of Polis", 0.50f, false),
             new Modifier(x=>(x as Factory).getProvince().hasModifier(Mod.recentlyConquered), Mod.recentlyConquered.ToString(), -0.20f, false),
             new Modifier(Government.isTribal, x=>(x as Factory).getCountry(), -1.0f, false),
             new Modifier(Government.isDespotism, x=>(x as Factory).getCountry(), -0.30f, false), // remove this?
             new Modifier(x=>!(x as Factory).getCountry().isInvented(Invention.Manufactures) 
             && !(x as Factory).getType().isResourceGathering(), Invention.ManufacturesUnInvented.getName(), -1f, false)
        });

    internal Factory(Province province, Agent factoryOwner, FactoryType type) : base(type, province)
    {
        //assuming this is level 0 building        
        constructionNeeds = getType().getBuildNeeds();
        province.allFactories.Add(this);
        setOwner(factoryOwner);
        salary.set(province.getLocalMinSalary());
        if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            setPriorityAutoWithPlannedEconomy();
    }
    //internal Value getMinimalMoneyForUpgrade()
    //{
    //    return Game.market.getCost(getUpgradeNeeds());
    //}
    internal StorageSet getUpgradeNeeds()
    {
        if (getLevel() < Options.FactoryMediumTierLevels)
            return getType().upgradeResourceLowTier;
        else
            if (getLevel() < Options.FactoryMediumHighLevels)
            return getType().upgradeResourceMediumTier;
        else
            return getType().upgradeResourceHighTier;
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
    public bool isJustHiredPeople()
    {
        return justHiredPeople;
    }
    override public string ToString()
    {
        return getType().name + " L" + getLevel();
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


    public void clearWorkforce()
    {
        hiredWorkForce.Clear();
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
        if (amount > 0 && wasWorkforce == 0)
            justHiredPeople = true;
        else
            justHiredPeople = false;

        clearWorkforce();
        if (amount > 0)
        {
            int leftToHire = amount;
            hiredLastTurn = 0;
            //foreach (PopUnit pop in popList)
            for (int i = popList.Count - 1; i >= 0; i--)
            {
                var pop = popList[i];
                if (pop.getPopulation() >= leftToHire) // satisfied demand
                {
                    hiredWorkForce.Add(pop, leftToHire);
                    //hiredLastTurn = getWorkForce() - wasWorkforce;
                    hiredLastTurn += leftToHire;
                    return hiredLastTurn;
                    //break;
                }
                else
                {
                    hiredWorkForce.Add(pop, pop.getPopulation()); // hire as we can
                    hiredLastTurn += pop.getPopulation();
                    leftToHire -= pop.getPopulation();
                }
            }
            //hiredLastTurn = getWorkForce() - wasWorkforce;
            return hiredLastTurn;
        }
        else
            return 0;
    }

    internal void setDontHireOnSubsidies(bool isOn)
    {
        dontHireOnSubsidies = isOn;
    }

    internal void setSubsidized(bool isOn)
    {
        subsidized = isOn;
    }
    internal void setPriorityAutoWithPlannedEconomy()
    {
        if (getType().basicProduction.getProduct().isIndustrial())
            setPriority(Factory.Priority.medium);
        else
        {
            if (getType().basicProduction.getProduct().isMilitary())
                setPriority(Factory.Priority.low);
            else //isConsumer()
                setPriority(Factory.Priority.none);
        }
    }
    internal void setPriority(int priority)
    {
        this.priority = priority;
    }
    internal void setPriority(Priority priority)
    {
        switch (priority)
        {
            case Priority.none:
                this.priority = 0;
                break;
            case Priority.low:
                this.priority = 1;
                break;
            case Priority.medium:
                this.priority = 2;
                break;
            case Priority.high:
                this.priority = 3;
                break;
            default:
                break;
        }

    }

    internal int howManyEmployed(PopUnit pop)
    {
        int result = 0;
        foreach (var link in hiredWorkForce)
            if (link.Key == pop)
                result += link.Value;
        return result;
    }

    internal bool isThereMoreWorkersToHire()
    {
        int totalAmountWorkers = getProvince().getPopulationAmountByType(PopType.Workers);
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

        if (getFreeJobSpace() > 0 && isThereMoreWorkersToHire())
            return true;
        else
            return false;
    }

    internal void paySalary()
    {
        if (isWorking() && getCountry().economy.getValue() != Economy.PlannedEconomy)
        {
            // per 1000 men            
            if (Economy.isMarket.checkIftrue(getCountry()))
            {
                foreach (var link in hiredWorkForce)
                {
                    Value howMuchPay = new Value(0);
                    howMuchPay.set(salary.get() * link.Value / (float)workForcePerLevel);
                    if (canPay(howMuchPay))
                        pay(link.Key, howMuchPay);
                    else
                        if (isSubsidized()) //take money and try again
                    {
                        getCountry().takeFactorySubsidies(this, howMuchMoneyCanNotPay(howMuchPay));
                        if (canPay(howMuchPay))
                            pay(link.Key, howMuchPay);
                        else
                            salary.set(getCountry().getMinSalary());
                    }
                    else
                        salary.set(getCountry().getMinSalary());
                    //todo else don't pay if there is nothing to pay
                }
            }
            // don't pay nothing if where is planned economy
            else if (getCountry().economy.getValue() == Economy.NaturalEconomy)
            {
                // non market!!
                Storage foodSalary = new Storage(Product.Grain, 1f);
                foreach (var link in hiredWorkForce)
                {
                    Storage howMuchPay = new Storage(foodSalary.getProduct(), foodSalary.get() * link.Value / (float)workForcePerLevel);
                    Country countryPayer = getOwner() as Country;
                    if (countryPayer != null)
                    {
                        if (countryPayer.countryStorageSet.has(howMuchPay))
                        {
                            countryPayer.countryStorageSet.send(link.Key, howMuchPay);
                            link.Key.addProduct(howMuchPay); // todo fails if is abstract
                            salary.set(foodSalary);
                        }
                        //todo no salary cuts yet
                        //else salary.set(0);
                    }
                    else // assuming - PopUnit
                    {
                        PopUnit popPayer = getOwner() as PopUnit;

                        if (popPayer.storage.has(howMuchPay))
                        {
                            popPayer.storage.send(link.Key.storage, howMuchPay);
                            link.Key.addProduct(howMuchPay);
                            salary.set(foodSalary);
                        }
                        //todo no resources to pay salary
                        //else salary.set(0);
                    }
                    //else dont pay if there is nothing to pay
                }
            }
        }
    }

    internal Procent getMargin()
    {
        float x = getProfit() / (Game.market.getCost(getUpgradeNeeds()).get() * level);
        return new Procent(x, false);
    }
    internal Value getReopenCost()
    {
        return new Value(Options.factoryMoneyReservePerLevel);

    }
    //internal Value getUpgradeCost()
    //{
    //    Value result = Game.market.getCost(getUpgradeNeeds());
    //    result.add(Options.factoryMoneyReservPerLevel);
    //    return result;
    //    //return Game.market.getCost(type.getUpgradeNeeds());
    //}


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
    public void setZeroSalary()
    {
        salary.setZero();
    }
    internal void changeSalary()
    {
        //if (getLevel() > 0)
        if (isWorking() && Economy.isMarket.checkIftrue(getCountry()))
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
            if (getWorkForce() < 100 && getProvince().getUnemployedWorkers() == 0 && this.cash.get() > 10f)// && getInputFactor() == 1)
                salary.add(0.09f);

            float minSalary = getCountry().getMinSalary();
            // reduce salary on non-profit
            if (getProfit() < 0f && daysUnprofitable >= Options.minDaysBeforeSalaryCut && !isJustHiredPeople() && !isSubsidized())
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
    /// 
    /// </summary>    
    public int howMuchWorkForceWants()
    {
        if (!isWorking())
            return 0;
        int wants = getMaxWorkforceCapacity();// * getInputFactor());

        int workForce = getWorkForce();
        int difference = wants - workForce;

        int maxHiringSpeed = getMaxHiringSpeed();
        // clamp difference in Options.maxFactoryFireHireSpeed []
        if (difference > maxHiringSpeed)
            difference = maxHiringSpeed;
        else
            if (difference < -1 * maxHiringSpeed) difference = -1 * maxHiringSpeed;

        if (difference > 0)
        {
            float inputFactor = getInputFactor().get();
            //fire people if no enough input.            
            if (inputFactor < 0.95f && !isSubsidized() && !isJustHiredPeople() && workForce > 0)// && getWorkForce() >= Options.maxFactoryFireHireSpeed)
                difference = -1 * maxHiringSpeed;

            if (getCountry().economy.getValue() != Economy.PlannedEconomy)// commies don't care about profits
            {
                //fire people if unprofitable. 
                if (getProfit() < 0f && !isSubsidized() && !isJustHiredPeople() && daysUnprofitable >= Options.minDaysBeforeSalaryCut)// && getWorkForce() >= Options.maxFactoryFireHireSpeed)
                    difference = -1 * maxHiringSpeed;

                // just don't hire more..
                //if ((getProfit() < 0f || inputFactor < 0.95f) && !isSubsidized() && !isJustHiredPeople() && workForce > 0)
                if (getProfit() < 0f && !isSubsidized() && !isJustHiredPeople() && workForce > 0)
                    difference = 0;
            }
        }
        //todo optimize getWorkforce() calls
        int result = workForce + difference;
        if (result < 0)
            return 0;
        return result;
    }
    internal int getHowMuchHiredLastTurn()
    {
        return hiredLastTurn;
    }

    /// <summary>
    /// per 1000 men    
    /// </summary>
    /// <returns></returns>
    internal Procent getWorkForceFulFilling()
    {
        return Procent.makeProcent(getWorkForce(), workForcePerLevel * level, false);
        //return getWorkForce() / (float)(workForcePerLevel * level);
    }
    override public List<Storage> getRealAllNeeds()
    {
        return getRealNeeds(new Value(getEfficiency(false).get() * getLevel()));
    }
    /// <summary>  Return in pieces basing on current prices and needs  /// </summary>        
    //override public float getLocalEffectiveDemand(Product product)
    //{
    //    return getLocalEffectiveDemand(product, getWorkForceFulFilling());
    //}

    /// <summary>
    /// per level
    /// </summary>    
    internal Procent getEfficiency(bool useBonuses)
    {
        //limit production by smallest factor
        Procent efficencyFactor;
        Procent workforceProcent = getWorkForceFulFilling();
        Procent inputFactor = getInputFactor();
        if (inputFactor.isZero() & isJustHiredPeople())
            inputFactor = Procent.HundredProcent;

        if (inputFactor.isSmallerThan(workforceProcent))
            efficencyFactor = inputFactor;
        else
            efficencyFactor = workforceProcent;
        //float basicEff = efficencyFactor * getLevel();
        //Procent result = new Procent(basicEff);
        //Procent result = new Procent(efficencyFactor);
        if (useBonuses)
            efficencyFactor.set(efficencyFactor.get() * modifierEfficiency.getModifier(this), false);
        return efficencyFactor;
    }



    private void stopUpgrading()
    {
        building = false;
        upgrading = false;
        constructionNeeds.setZero();
        daysInConstruction = 0;
    }
    internal void markToDestroy()
    {
        toRemove = true;

        //return loans only if banking invented
        if (getCountry().isInvented(Invention.Banking))
        {
            if (loans.get() > 0f)
            {
                Value howMuchToReturn = new Value(loans);
                if (howMuchToReturn.get() <= cash.get())
                    howMuchToReturn.set(cash);
                getBank().takeMoney(this, howMuchToReturn);
                if (loans.get() > 0f)
                    getBank().defaultLoaner(this);
            }
        }
        sendAllAvailableMoney(getOwner());
        MainCamera.factoryPanel.removeFactory(this);
    }
    internal void destroyImmediately()
    {
        markToDestroy();
        getProvince().allFactories.Remove(this);
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
        return getExpences() * Factory.xMoneyReservForResources + Options.factoryMoneyReservePerLevel * level;
    }
    public void simulateClosing()
    {
        if (getProfit() <= 0)
        {
            daysUnprofitable++;
            if (daysUnprofitable == Options.maxDaysUnprofitableBeforeFactoryClosing && !isSubsidized())
                this.close();
        }
        else
            daysUnprofitable = 0;
    }
    public void simulateOpening()
    {
        if (!isWorking())
            //closed
            if (!isBuilding())
            {
                daysClosed++;
                if (daysClosed == Options.maxDaysClosedBeforeRemovingFactory)
                    markToDestroy();
                else
                if (Game.Random.Next(Options.howOftenCheckForFactoryReopenning) == 1)
                {
                    if (getCountry().economy.getValue() == Economy.PlannedEconomy)
                    {
                        open(this);
                    }
                    else
                    {
                        //take loan for reopen
                        if (getCountry().isInvented(Invention.Banking) && this.getType().getPossibleProfit(getProvince()).get() > 10f)
                        {
                            float leftOver = cash.get() - wantsMinMoneyReserv();
                            if (leftOver < 0)
                            {
                                Value loanSize = new Value(leftOver * -1f);
                                if (getBank().canGiveMoney(this, loanSize))
                                    getBank().giveMoney(this, loanSize);
                            }
                            leftOver = cash.get() - wantsMinMoneyReserv();
                            if (leftOver >= 0f)
                                open(this);
                        }
                    }
                }
            }
    }
    internal void payDividend()
    {
        if (isWorking())
        {
            float saveForYourSelf = wantsMinMoneyReserv();
            float divident = cash.get() - saveForYourSelf;

            if (divident > 0)
            {
                Value sentToOwner = new Value(divident);
                pay(getOwner(), sentToOwner);
                var owner = getOwner() as Country;
                if (owner != null)
                    owner.ownedFactoriesIncomeAdd(sentToOwner);
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
        constructionNeeds.setZero();
        daysInConstruction = 0;
    }
    internal void open(Agent byWhom)
    {
        working = true;
        daysUnprofitable = 0;
        daysClosed = 0;
        if (byWhom.getCountry().economy.getValue() != Economy.PlannedEconomy)
        {
            if (daysUnprofitable > 20)
                salary.set(getProvince().getLocalMinSalary());

            if (byWhom != this)
                byWhom.payWithoutRecord(this, getReopenCost());
        }
    }

    internal bool isWorking()
    {
        return working && !building; // todo WTF?
    }

    internal float getSalaryCost()
    {
        return getWorkForce() * getSalary() / workForcePerLevel;
    }

    internal bool canUpgrade()
    {
        return !isUpgrading() && !isBuilding() && level < Options.maxFactoryLevel && isWorking();
    }
    internal void upgrade(Agent byWhom)
    {
        upgrading = true;
        constructionNeeds.add(getUpgradeNeeds().getCopy());
        if (byWhom.getCountry().economy.getValue() != Economy.PlannedEconomy)
            byWhom.payWithoutRecord(this, Game.market.getCost(getUpgradeNeeds()));
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
    override internal float getProfit()
    {
        return base.getProfit() - getSalaryCost();
    }

    public override List<Storage> getHowMuchInputProductsReservesWants()
    {
        //if (getCountry().economy.getValue() == Economy.PlannedEconomy)
        //    return getHowMuchInputProductsReservesWants(new Value(getWorkForceFulFilling().get() * getLevel())); // only 1 day reserves with PE
        //else
        return getHowMuchInputProductsReservesWants(new Value(getWorkForceFulFilling().get() * getLevel() * Options.FactoryInputReservInDays));
    }

    internal override Procent getInputFactor()
    {
        return getInputFactor(getWorkForceFulFilling());
    }
    /// <summary>
    /// Fills storageNow and gainGoodsThisTurn
    /// </summary>
    public override void produce()
    {
        if (isWorking())
        {
            int workers = getWorkForce();
            if (workers > 0)
                base.produce(new Value(getEfficiency(true).get() * getLevel()));

            if (getType() == FactoryType.GoldMine)
            {
                this.ConvertFromGoldAndAdd(storage);
                //send 50% to government
                Value sentToGovernment = new Value(moneyIncomethisTurn.get() * Options.GovernmentTakesShareOfGoldOutput);
                pay(getCountry(), sentToGovernment);
                getCountry().goldMinesIncomeAdd(sentToGovernment);
            }
            else
            {
                if (Economy.isMarket.checkIftrue(getCountry()))
                {
                    //sentToMarket.set(gainGoodsThisTurn);
                    //storage.setZero();
                    //Game.market.sentToMarket.add(gainGoodsThisTurn);
                    sell(getGainGoodsThisTurn());
                }
                else if (getCountry().economy.getValue() == Economy.NaturalEconomy)
                {
                    Country countryOwner = getOwner() as Country;
                    if (countryOwner != null)
                        storage.sendAll(countryOwner.countryStorageSet);
                    else // assuming owner is aristocrat/capitalist
                    {
                        sell(getGainGoodsThisTurn());
                        //sentToMarket.set(gainGoodsThisTurn);
                        //storage.setZero();
                        //Game.market.sentToMarket.add(gainGoodsThisTurn);
                    }
                }
                else if (getCountry().economy.getValue() == Economy.PlannedEconomy)
                {
                    storage.sendAll(getCountry().countryStorageSet);
                }
            }
        }
    }
    private void onConstructionComplete()
    {
        level++;
        building = false;
        upgrading = false;
        constructionNeeds.setZero();
        daysInConstruction = 0;
        open(this);
    }
    /// <summary>
    /// Now includes workforce/efficiency. Also buying for upgrading\building are happening here 
    /// </summary>
    override public void consumeNeeds()
    {
        // consume resource needs
        if (isWorking() && !getType().isResourceGathering())
        {
            List<Storage> shoppingList = getHowMuchInputProductsReservesWants();
            if (shoppingList.Count > 0)
                if (getCountry().economy.getValue() == Economy.PlannedEconomy)
                {
                    var realNeed = getCountry().countryStorageSet.hasAllOfConvertToBiggest(shoppingList);
                    if (realNeed != null)
                    {
                        //getCountry().countryStorageSet.send(this.getInputProductsReserve(), shoppingList);
                        consumeFromCountryStorage(realNeed, getCountry());
                        getInputProductsReserve().add(realNeed);
                    }
                }
                else
                {
                    if (isSubsidized())
                        Game.market.buy(this, new StorageSet(shoppingList), getCountry());
                    else
                        Game.market.buy(this, new StorageSet(shoppingList), null);
                }
        }
        if (isUpgrading() || isBuilding())
        {
            daysInConstruction++;
            bool isBuyingComplete = false;

            if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            {
                if (daysInConstruction >= Options.fabricConstructionTimeWithoutCapitalism)
                    //if (isBuilding())
                    //{
                    //    var buildingNeeds = getType().getBuildNeeds();
                    //    if (getCountry().countryStorageSet.has(buildingNeeds))
                    //        isBuyingComplete = getCountry().countryStorageSet.send(this, buildingNeeds);
                    //}
                    //else if (isUpgrading())
                    //{                               
                    if (getCountry().countryStorageSet.has(constructionNeeds))
                        isBuyingComplete = getCountry().countryStorageSet.send(this.getInputProductsReserve(), constructionNeeds);
                //}
            }
            else
            {
                if (isBuilding())
                    isBuyingComplete = Game.market.buy(this, constructionNeeds, Options.BuyInTimeFactoryUpgradeNeeds, getType().getBuildNeeds());
                else if (isUpgrading())
                    isBuyingComplete = Game.market.buy(this, constructionNeeds, Options.BuyInTimeFactoryUpgradeNeeds, getUpgradeNeeds());

                // what if there is no enough money to complete buildinG?
                float minimalFond = cash.get() - 50f;

                if (minimalFond < 0 && getOwner().canPay(new Value(minimalFond * -1f)))
                    getOwner().payWithoutRecord(this, new Value(minimalFond * -1f));
            }
            if (isBuyingComplete
               || (getCountry().economy.getValue() == Economy.NaturalEconomy && daysInConstruction == Options.fabricConstructionTimeWithoutCapitalism))

            {
                onConstructionComplete();
                //todo avoid extra subtraction and redo whole method
                if (isBuilding())
                    getInputProductsReserve().subtract(getType().getBuildNeeds(), false);
                else // assuming isUpgrading()
                    getInputProductsReserve().subtract(getUpgradeNeeds(), false);
            }
            else
            {
                if (daysInConstruction == Options.maxDaysBuildingBeforeRemoving)
                    if (isBuilding())
                        markToDestroy();
                    else // upgrading
                        stopUpgrading();
            }
        }
    }
    override internal float getExpences()
    {
        return base.getExpences() + getSalaryCost();
    }
    /// <summary>
    /// Should optimize? Seek for changes..
    /// </summary>
    /// <returns></returns>
    public int getWorkForce()
    {
        int result = 0;
        foreach (var pop in hiredWorkForce)
            result += pop.Value;
        return result;
    }
}


