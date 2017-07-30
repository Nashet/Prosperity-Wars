using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
public class Factory : Producer
{
    //public enum PopTypes { Forestry, GoldMine, MetalMine };
    private static readonly int workForcePerLevel = 1000;
    private static int xMoneyReservForResources = 10;

    internal readonly FactoryType type;

    private int level = 0;

    private bool building = true;
    private bool upgrading = false;
    private bool working = false;
    private bool toRemove = false;

    private bool dontHireOnSubsidies, subsidized;
    private byte priority = 0;
    private readonly Value salary = new Value(0);
    private Agent factoryOwner;
    internal readonly PrimitiveStorageSet needsToUpgrade;
    internal readonly PrimitiveStorageSet inputReservs = new PrimitiveStorageSet();

    private readonly Dictionary<PopUnit, int> hiredWorkForce = new Dictionary<PopUnit, int>();

    private int daysInConstruction;
    private int daysUnprofitable;
    private int daysClosed;
    private bool justHiredPeople;
    private int hiredLastTurn;

    internal static readonly Modifier
        modifierHasResourceInProvince = new Modifier(x => !(x as Factory).type.isResourceGathering() && (x as Factory).province.isProducingOnFactories((x as Factory).type.resourceInput),
              "Has input resource in this province", 20f, false),

        modifierLevelBonus = new Modifier(x => (x as Factory).getLevel() - 1, "High production concentration bonus", 1f, false),

        modifierInventedMiningAndIsShaft = new Modifier(x => (x as Factory).getCountry().isInvented(Invention.Mining) && (x as Factory).type.isShaft(),
           new StringBuilder("Invented ").Append(Invention.Mining.ToString()).ToString(), 50f, false),

        modifierBelongsToCountry = new Modifier(x => (x as Factory).factoryOwner is Country, "Belongs to government", -20f, false),
        modifierIsSubsidised = new Modifier((x) => (x as Factory).isSubsidized(), "Is subsidized", -10f, false);

    internal static readonly Condition
        conNotBelongsToCountry = new Condition(x => !((x as Factory).factoryOwner is Country), "Doesn't belongs to government", false),
        conNotUpgrading = new Condition(x => !(x as Factory).isUpgrading(), "Not upgrading", false),
        conNotBuilding = new Condition(x => !(x as Factory).isBuilding(), "Not building", false),
        conOpen = new Condition(x => (x as Factory).isWorking(), "Open", false),
        conClosed = new Condition(x => !(x as Factory).isWorking(), "Closed", false),
        conMaxLevelAchieved = new Condition(x => (x as Factory).getLevel() != Options.maxFactoryLevel, "Max level not achieved", false),
        conNotLForNotCountry = new Condition(x => (x as Factory).getCountry().economy.status != Economy.LaissezFaire || !(x is Country), "Economy policy is not Laissez Faire", true),
        conPlayerHaveMoneyToReopen = new Condition(x => Game.Player.canPay((x as Factory).getReopenCost()), delegate (object x)
        {
            Game.threadDangerSB.Clear();
            Game.threadDangerSB.Append("Have ").Append((x as Factory).getReopenCost()).Append(" coins");
            return Game.threadDangerSB.ToString();
        }, true);
    internal static readonly ConditionForDoubleObjects
        conHaveMoneyToUpgrade = new ConditionForDoubleObjects((factory, agent) => (agent as Agent).canPay((factory as Factory).getUpgradeCost()),
            //x=> "Have "+" coins"
            (factory) => "Have " + (factory as Factory).getUpgradeCost() + " coins"
            //delegate (object x)
            //{
            //    Game.threadDangerSB.Clear();
            //    Game.threadDangerSB.Append("Have ").Append((x as Factory).getUpgradeCost()).Append(" coins");
            //    return Game.threadDangerSB.ToString();
            //}
            , true),
        conPlacedInOurCountry = new ConditionForDoubleObjects((factory, country) => (factory as Factory).getCountry() == country as Country,
        (factory) => "Enterprise placed in our country", true)
        ;

    internal static readonly ConditionsListForDoubleObjects
        conditionsUpgrade = new ConditionsListForDoubleObjects(new List<Condition>
        {
            conNotUpgrading, conNotBuilding, conOpen, conMaxLevelAchieved, conNotLForNotCountry,
            conHaveMoneyToUpgrade, conPlacedInOurCountry
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
        .addForSecondObject(new List<Condition> { Economy.isNotLF, Economy.isNotNatural }),

        conditionsDontHireOnSubsidies = new ConditionsListForDoubleObjects(new List<Condition> { conPlacedInOurCountry })
        .addForSecondObject(new List<Condition> { Economy.isNotLF, Economy.isNotNatural, Condition.IsNotImplemented }),

        conditionsChangePriority = new ConditionsListForDoubleObjects(new List<Condition> { conPlacedInOurCountry })
        .addForSecondObject(new List<Condition> { Economy.isNotLF, Condition.IsNotImplemented });
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
           Modifier.modifierDefault100,
            new Modifier(Invention.SteamPowerInvented, x => (x as Factory).getCountry(), 25f, false),
            new Modifier(Invention.CombustionEngineInvented, x => (x as Factory).getCountry(), 25f, false),

            new Modifier(Economy.isStateCapitlism, x => (x as Factory).getCountry(),  10f, false),
            new Modifier(Economy.isInterventionism, x => (x as Factory).getCountry(),  30f, false),
            new Modifier(Economy.isLF, x => (x as Factory).getCountry(), 50f, false),
            new Modifier(Economy.isPlanned, x => (x as Factory).getCountry(), -10f, false),
            modifierInventedMiningAndIsShaft, modifierHasResourceInProvince, modifierLevelBonus, modifierBelongsToCountry, modifierIsSubsidised,
             new Modifier(x => Government.isPolis.checkIftrue((x as Factory).getCountry())
             && (x as Factory).province.isCapital(), "Capital of Polis", 100f, false),
             new Modifier(x=>(x as Factory).province.hasModifier(Mod.recentlyConquered), Mod.recentlyConquered.ToString(), -20f, false),
             new Modifier(Government.isTribal, x=>(x as Factory).getCountry(), -100f, false),
             new Modifier(Government.isDespotism, x=>(x as Factory).getCountry(), -30f, false) // remove this?
        });

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
    public bool isJustHiredPeople()
    {
        return justHiredPeople;
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
        foreach (var pop in hiredWorkForce)
            result += pop.Value;
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
        if (amount > 0 && wasWorkforce == 0)
            justHiredPeople = true;
        else
            justHiredPeople = false;

        hiredWorkForce.Clear();
        if (amount > 0)
        {
            int leftToHire = amount;
            foreach (PopUnit pop in popList)
            {
                if (pop.getPopulation() >= leftToHire) // satisfied demand
                {
                    hiredWorkForce.Add(pop, leftToHire);
                    hiredLastTurn = getWorkForce() - wasWorkforce;
                    return hiredLastTurn;
                    //break;
                }
                else
                {
                    hiredWorkForce.Add(pop, pop.getPopulation()); // hire as we can
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

        if (getFreeJobSpace() > 0 && isThereMoreWorkersToHire())
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
                foreach (var link in hiredWorkForce)
                {
                    Value howMuchPay = new Value(0);
                    howMuchPay.set(salary.get() * link.Value / (float)workForcePerLevel);
                    if (canPay(howMuchPay))
                        pay(link.Key, howMuchPay);
                    else
                        if (isSubsidized()) //take money and try again
                    {
                        province.getCountry().takeFactorySubsidies(this, HowMuchMoneyCanNotPay(howMuchPay));
                        if (canPay(howMuchPay))
                            pay(link.Key, howMuchPay);
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
                foreach (var link in hiredWorkForce)
                {
                    Storage howMuchPay = new Storage(foodSalary.getProduct(), foodSalary.get() * link.Value / (float)workForcePerLevel);
                    if (factoryOwner is Country)
                    {
                        Country payer = factoryOwner as Country;

                        if (payer.storageSet.has(howMuchPay))
                        {
                            payer.storageSet.send(link.Key, howMuchPay);
                            link.Key.gainGoodsThisTurn.add(howMuchPay);
                            salary.set(foodSalary);
                        }
                        //todo no salary cuts yet
                        //else salary.set(0);
                    }
                    else // assuming - PopUnit
                    {
                        PopUnit payer = factoryOwner as PopUnit;

                        if (payer.storageNow.has(link.Key.storageNow, howMuchPay))
                        {
                            payer.storageNow.send(link.Key.storageNow, howMuchPay);
                            link.Key.gainGoodsThisTurn.add(howMuchPay);
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
    internal float getProfit()
    {
        float z = moneyIncomethisTurn.get() - Game.market.getCost(consumedTotal).get() - getSalaryCost();
        return z;
    }
    internal Procent getMargin()
    {
        float x = getProfit() / (getUpgradeCost().get() * level);
        return new Procent(x, false);
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

    /// <summary>
    /// Fills storageNow and gainGoodsThisTurn
    /// </summary>
    public override void produce()
    {
        if (isWorking())
        {
            int workers = getWorkForce();
            if (workers > 0)
            {

                Storage producedAmount = new Storage(type.basicProduction.getProduct(), type.basicProduction.get() * getEfficiency(true).get() * getLevel()); // * getLevel());

                storageNow.add(producedAmount);
                gainGoodsThisTurn.set(producedAmount);
                //consumeInputResources
                foreach (Storage next in getRealNeeds())
                    inputReservs.subtract(next, false);

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
                    storageNow.setZero();
                    Game.market.sentToMarket.add(gainGoodsThisTurn);
                }
                if (Economy.isMarket.checkIftrue(province.getCountry()))
                {
                    // Buyers should come and buy something...
                    // its in other files.
                }
                else // send all production to owner
                    ; // todo write !capitalism
                      //storageNow.sendAll(owner.storageSet);
            }
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
        // there is no corporate taxes yet
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
        return getWorkForce() / (float)(workForcePerLevel * level);
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
        if (inputFactor < workforceProcent)
            efficencyFactor = inputFactor;
        else
            efficencyFactor = workforceProcent;
        //float basicEff = efficencyFactor * getLevel();
        //Procent result = new Procent(basicEff);
        Procent result = new Procent(efficencyFactor);
        if (useBonuses)
            result.set(result.get() * (modifierEfficiency.getModifier(this) / 100f), false);
        return result;
    }

    public List<Storage> getHowMuchReservesWants()
    {
        Value multiplier = new Value(getWorkForceFullFilling() * getLevel() * Options.FactoryInputReservInDays);

        List<Storage> result = new List<Storage>();

        foreach (Storage next in type.resourceInput)
        {
            Storage howMuchWantBuy = new Storage(next.getProduct(), next.get());
            howMuchWantBuy.multiple(multiplier);
            Storage reserv = inputReservs.findStorage(next.getProduct());
            if (reserv == null)
                result.Add(howMuchWantBuy);
            else
            {
                if (howMuchWantBuy.isBiggerOrEqual(reserv))
                {
                    howMuchWantBuy.subtract(reserv);
                    result.Add(howMuchWantBuy);
                }//else  - there is enough reservs, don't buy that
            }
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
        return Game.market.getCost(consumedTotal).get() + getSalaryCost();
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
        needsToUpgrade.add(getUpgradeNeeds().getCopy());
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

