using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
public class Factory : Producer
{
    //public enum PopTypes { Forestry, GoldMine, MetalMine };
    //public PopTypes type;
    internal FactoryType type;
    protected static uint workForcePerLevel = 1000;
    protected byte level = 0;
    /// <summary>shownFactory in a process of building - level 1 </summary>
    private bool building = true;
    private bool upgrading = false;
    private bool working = false;
    private bool toRemove = false;
    private bool dontHireOnSubsidies, subsidized;
    private byte priority = 0;
    protected Value salary = new Value(0);
    internal Owner factoryOwner;
    internal PrimitiveStorageSet needsToUpgrade;
    protected List<PopLinkage> workForce = new List<PopLinkage>();
    private static int xMoneyReservForResources = 10;
    private uint daysInConstruction;
    private uint daysUnprofitable;
    private uint daysClosed;
    internal bool justHiredPeople;
    private int hiredLastTurn;
    internal ConditionsList conditionsUpgrade, conditionsClose, conditionsReopen,
        conditionsDestroy, conditionsSell, conditionsBuy, conditionsNatinalize,
        conditionsSubsidize, conditionsDontHireOnSubsidies, conditionsChangePriority;
    internal ModifiersList modifierEfficiency;

    internal Factory(Province iprovince, Owner inowner, FactoryType intype)
    { //assuming this is level 0 building
        type = intype;
        needsToUpgrade = type.getBuildNeeds();
        iprovince.allFactories.Add(this);
        factoryOwner = inowner;
        province = iprovince;

        gainGoodsThisTurn = new Storage(type.basicProduction.getProduct());
        storageNow = new Storage(type.basicProduction.getProduct());
        sentToMarket = new Storage(type.basicProduction.getProduct());

        salary.set(province.getLocalMinSalary());
        conditionsUpgrade = new ConditionsList(new List<Condition>()
        {
            new Condition(delegate (Owner forWhom) { return province.owner.economy.status != Economy.LaissezFaire || forWhom is PopUnit; }, "Economy policy is not Laissez Faire", true),
            new Condition(delegate (Owner forWhom) { return !isUpgrading(); }, "Not upgrading", false),
            new Condition(delegate (Owner forWhom) { return !isBuilding(); }, "Not building", false),
            new Condition(delegate (Owner forWhom) { return isWorking(); }, "Open", false),
            new Condition(delegate (Owner forWhom) { return level != Game.maxFactoryLevel; }, "Max level not achieved", false),
            new Condition(delegate (Owner forWhom) {
                 Value cost = this.getUpgradeCost();
                return forWhom.wallet.canPay(cost);}, delegate () {
                    Game.threadDangerSB.Clear();
                    Game.threadDangerSB.Append("Have ").Append(getUpgradeCost()).Append(" coins");
                    return Game.threadDangerSB.ToString();
                }, true)
        });

        conditionsClose = new ConditionsList(new List<Condition>()
        {
            new Condition(delegate (Owner forWhom) { return province.owner.economy.status != Economy.LaissezFaire || forWhom is PopUnit; }, "Economy policy is not Laissez Faire", true),
            new Condition(delegate (Owner forWhom) { return !isBuilding(); }, "Not building", false),
            new Condition(delegate (Owner forWhom) { return isWorking(); }, "Open", false),
        });
        conditionsReopen = new ConditionsList(new List<Condition>()
        {
            new Condition(delegate (Owner forWhom) { return province.owner.economy.status != Economy.LaissezFaire || forWhom is PopUnit; }, "Economy policy is not Laissez Faire", true),
            new Condition(delegate (Owner forWhom) { return !isBuilding(); }, "Not building", false),
            new Condition(delegate (Owner forWhom) { return !isWorking(); }, "Close", false),
            new Condition(delegate (Owner forWhom) {
                return forWhom.wallet.canPay(getReopenCost());},  delegate () {
                    Game.threadDangerSB.Clear();
                    Game.threadDangerSB.Append("Have ").Append(getReopenCost()).Append(" coins");
                    return Game.threadDangerSB.ToString();
                }, true)
        });
        conditionsDestroy = new ConditionsList(new List<Condition>() { Economy.isNotLF });
        //status == Economy.LaissezFaire || status == Economy.Interventionism || status == Economy.NaturalEconomy
        conditionsSell = ConditionsList.IsNotImplemented; // !Planned and ! State

        //(status == Economy.StateCapitalism || status == Economy.Interventionism || status == Economy.NaturalEconomy)
        conditionsBuy = ConditionsList.IsNotImplemented; // ! LF and !Planned

        // (status == Economy.PlannedEconomy || status == Economy.NaturalEconomy || status == Economy.StateCapitalism)
        conditionsNatinalize = ConditionsList.IsNotImplemented; //!LF and ! Inter


        conditionsSubsidize = new ConditionsList(new List<Condition>()
        {
            Economy.isNotLF, Economy.isNotNatural, Condition.IsNotImplemented
        });
        conditionsDontHireOnSubsidies = new ConditionsList(new List<Condition>()
        {
            Economy.isNotLF, Economy.isNotNatural, Condition.IsNotImplemented
        });
        conditionsChangePriority = new ConditionsList(new List<Condition>() { Economy.isNotLF, Condition.IsNotImplemented });

        Modifier modifierHasResourceInProvince = new Modifier(delegate (Country forWhom)
        {
            return !type.isResourceGathering() && province.isProducingOnFactories(type.resourceInput);
        },
            "Has input resource in thst province", true, 20f);
        Modifier modifierLevelBonus = new Modifier(delegate () { return this.getLevel(); }, "High production concetration bonus", true, 5f);
        Modifier conditionInventedMiningAndIsShaft = new Modifier(
            delegate (Country forWhom)
            {
                return forWhom.isInvented(InventionType.mining) && type.isShaft();
            },
            new StringBuilder("Invented ").Append(InventionType.mining.ToString()).ToString(), false, 50f);
        modifierEfficiency = new ModifiersList(new List<Condition>()
        {
            new Modifier(InventionType.steamPower, true, 25f),
            conditionInventedMiningAndIsShaft,
            new Modifier(Economy.StateCapitalism, true, 10f),
            new Modifier(Economy.Interventionism, true, 30f),
            new Modifier(Economy.LaissezFaire, true, 50f),
            new Modifier(Economy.PlannedEconomy, true, -10f),
            modifierHasResourceInProvince, modifierLevelBonus
        });
    }

    internal float getPriority()
    {
        return priority;
    }

    internal bool isdontHireOnSubsidies()
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
    internal byte getLevel()
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
    internal Wallet getOwnerWallet()
    {
        //if (factoryOwner != null) return factoryOwner.wallet;
        //else return factoryOwner.wallet;
        return factoryOwner.wallet;
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

        // need to know huw much i Consumed inside my needs
        Storage need = type.resourceInput.findStorage(product);
        if (need != null)
        {
            Storage realNeed = new Storage(need.getProduct(), need.get() * getWorkForceFullFilling());
            //Storage realNeed = new Storage(need.getProduct(), need.get() * getInputFactor());
            Storage canAfford = wallet.HowMuchCanAfford(realNeed);
            return canAfford.get();
        }
        else return 0f;
    }



    /// <summary>
    /// Should optimize? Seek for changes..
    /// </summary>
    /// <returns></returns>
    public uint getWorkForce()
    {
        uint result = 0;
        foreach (PopLinkage pop in workForce)
            result += pop.amount;
        return result;
    }
    public void HireWorkforce(uint amount, List<PopUnit> popList)
    {
        //check on no too much workers?
        //if (amount > HowMuchWorkForceWants())
        //    amount = HowMuchWorkForceWants();
        uint wasWorkforce = getWorkForce();
        workForce.Clear();
        if (amount > 0)
        {

            uint leftToHire = amount;
            foreach (PopUnit pop in popList)
            {
                if (pop.population >= leftToHire) // satisfied demand
                {
                    workForce.Add(new PopLinkage(pop, leftToHire));
                    hiredLastTurn = (int)getWorkForce() - (int)wasWorkforce;
                    break;
                }
                else
                {
                    workForce.Add(new PopLinkage(pop, pop.population)); // hire as we can
                    leftToHire -= pop.population;
                }
            }
            hiredLastTurn = (int)getWorkForce() - (int)wasWorkforce;
        }
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

    internal uint HowManyEmployed(PopUnit pop)
    {
        uint result = 0;
        foreach (PopLinkage link in workForce)
            if (link.pop == pop)
                result += link.amount;
        return result;
    }
    internal bool IsThereMoreWorkersToHire()
    {
        uint totalAmountWorkers = province.FindPopulationAmountByType(PopType.workers);
        uint result = totalAmountWorkers - getWorkForce();
        return (result > 0);
    }
    internal uint getFreeJobSpace()
    {
        return getMaxWorkforceCapacity() - getWorkForce();
    }
    internal bool ThereIsSpaceToHireMore()
    {
        //if there is other pops && there is space on shownFactory
        // uint totalAmountWorkers = province.FindPopulationAmountByType(PopType.workers);
        uint freeSpace = getMaxWorkforceCapacity() - getWorkForce();
        if (freeSpace > 0 && IsThereMoreWorkersToHire())
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
            Storage foodSalary = new Storage(Product.Food, 1f);
            // Value moneySalary = new Value(0.8f);
            //if (province.owner.isInvented(InventionType.capitalism))
            //if (province.owner.economy.isMarket())
            if (Economy.isMarket.checkIftrue(province.owner))
            {
                foreach (PopLinkage link in workForce)
                {
                    Value howMuchPay = new Value(0);
                    howMuchPay.set(salary.get() * link.amount / 1000f);
                    if (wallet.canPay(howMuchPay))
                    {
                        wallet.pay(link.pop.wallet, howMuchPay);
                        //link.pop.producedLastTurn.add(howMuchPay);
                    }
                    else
                        salary.set(province.owner.getMinSalary());
                    //todo else dont pay if there is nothing to pay
                }
            }
            else
            {
                // non market
                foreach (PopLinkage link in workForce)
                {
                    Storage howMuchPay = new Storage(foodSalary.getProduct(), foodSalary.get() * link.amount / 1000f);
                    if (factoryOwner is Country)
                    {
                        Country payer = factoryOwner as Country;

                        if (payer.storageSet.has(howMuchPay))
                        {
                            payer.storageSet.pay(link.pop.storageNow, howMuchPay);
                            link.pop.gainGoodsThisTurn.add(howMuchPay);
                            salary.set(foodSalary);
                        }
                        //todo no resiuces tio pay salary
                        //else salary.set(0);
                    }
                    else // assuming - PopUnit
                    {
                        PopUnit payer = factoryOwner as PopUnit;

                        if (payer.storageNow.canPay(link.pop.storageNow, howMuchPay))
                        {
                            payer.storageNow.pay(link.pop.storageNow, howMuchPay);
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
        float z = wallet.moneyIncomethisTurn.get() - getConsumedCost() - getSalaryCost();
        return z;
    }
    internal Procent getMargin()
    {
        float x = getProfit() / (getUpgradeCost().get() * level);
        return new Procent(x);
    }
    internal Value getReopenCost()
    {
        return new Value(Game.factoryMoneyReservPerLevel);

    }
    internal Value getUpgradeCost()
    {
        Value result = Game.market.getCost(type.getUpgradeNeeds());
        result.add(Game.factoryMoneyReservPerLevel);
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
        //if (getLevel() > 0)
        if (isWorking())
        {
            uint workers = getWorkForce();
            if (workers > 0)
            {
                Value producedAmount;
                producedAmount = new Value(type.basicProduction.get() * getEfficiency(true).get() * getLevel()); // * getLevel());

                storageNow.add(producedAmount);
                gainGoodsThisTurn.set(producedAmount);
                if (type == FactoryType.GoldMine)
                //if (province.owner.isInvented(InventionType.capitalism))
                {
                    this.wallet.ConvertFromGoldAndAdd(storageNow);
                    //send 50% to government
                    wallet.pay(province.owner.wallet, new Value(wallet.moneyIncomethisTurn.get() * Game.GovernmentTakesShareOfGoldOutput));
                }
                //else // send all production to owner
                //    storageNow.sendAll(province.owner.storageSet);
                else
                {
                    sentToMarket.set(gainGoodsThisTurn);
                    storageNow.set(0f);
                    Game.market.tmpMarketStorage.add(gainGoodsThisTurn);
                }

                //if (province.owner.isInvented(InventionType.capitalism))
                if (Economy.isMarket.checkIftrue(province.owner))
                {
                    // Buyers should come and buy something...
                    // its in other files.
                }
                else // send all production to owner
                    ; // write ! capitalism
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
    public uint getMaxWorkforceCapacity()
    {
        uint cantakeMax = level * workForcePerLevel;
        return cantakeMax;
    }
    internal void changeSalary()
    {
        //if (getLevel() > 0)
        if (isWorking() && Economy.isMarket.checkIftrue(province.owner))
        //        province.owner.isInvented(InventionType.capitalism))
        {
            // rise salary to entice workforce
            if (ThereIsSpaceToHireMore() && getMargin().get() > Game.minMarginToRiseSalary)// && getInputFactor() == 1)
                salary.add(0.01f);
            //too allocate workers form other popTypes
            if (getFreeJobSpace() > 100 && province.FindPopulationAmountByType(PopType.workers) < 600
                && getMargin().get() > Game.minMarginToRiseSalary && getInputFactor() == 1)
                salary.add(0.01f);

            // to help factories catch up other factories salaries
            //if (getWorkForce() <= 100 && province.getUnemployed() == 0 && this.wallet.haveMoney.get() > 10f)
            //    salary.set(province.getLocalMinSalary());
            // freshly builded factories should rise salary to concurency with old ones
            if (getWorkForce() <= 100 && province.getUnemployed() == 0 && this.wallet.haveMoney.get() > 10f)// && getInputFactor() == 1)
                salary.add(0.03f);

            // reduce salary on non-profit
            if (getProfit() < 0 && daysUnprofitable >= Game.minDaysBeforeSalaryCut && !justHiredPeople)
                if (salary.get() - 0.3f >= province.owner.getMinSalary())
                    salary.subtract(0.3f);
                else
                    salary.set(province.owner.getMinSalary());
        }
    }
    /// <summary>
    /// max - max capacity
    /// </summary>
    /// <returns></returns>
    public uint HowMuchWorkForceWants()
    {
        //if (getLevel() == 0) return 0;
        if (!isWorking()) return 0;
        uint wants = (uint)Mathf.RoundToInt(getMaxWorkforceCapacity());// * getInputFactor());
        int difference = (int)wants - (int)getWorkForce();

        if (difference > Game.maxFactoryFireHireSpeed)
            difference = Game.maxFactoryFireHireSpeed;
        else
            if (difference < -1 * Game.maxFactoryFireHireSpeed) difference = -1 * Game.maxFactoryFireHireSpeed;

        //reduce hiring if no enough input. getHowMuchHiredLastTurn() - to avoid last turn input error
        if (difference > 0 && !justHiredPeople && getInputFactor() < 0.95f && !(getHowMuchHiredLastTurn() > 0))// && getWorkForce() >= Game.maxFactoryFireHireSpeed)
            difference = -1 * Game.maxFactoryFireHireSpeed;
        //don't hire more if unprofitable. Even fire some
        if (difference > 0 && (getProfit() < 0f) && !justHiredPeople && daysUnprofitable >= Game.minDaysBeforeSalaryCut)// && getWorkForce() >= Game.maxFactoryFireHireSpeed)
            difference = -1 * Game.maxFactoryFireHireSpeed;
        // just dont't hire more..
        if (difference > 0 && (getProfit() < 0f || getInputFactor() < 0.95f))
            difference = 0;
        //todo optimaze getWorkforce()
        int result = (int)getWorkForce() + difference;
        if (result < 0) return 0;
        return (uint)result;
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
        Storage available;

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
            available = consumedLastTurn.findStorage(input.getProduct());
            if (available == null)
                ;// do nothing - pretend there is 100%, it fires only on shownFactory start
            else
            if (!justHiredPeople && available.get() < input.get())
                input.set(available);
        }
        // checking if there is enough money to pay for
        foreach (Storage input in realInput)
        {
            Storage howMuchCan = wallet.HowMuchCanAfford(input);
            input.set(howMuchCan.get());
        }
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
        {
            result.set(result.get() * (1f + modifierEfficiency.getModifier(province.owner) / 100f));
            //result.add();

            //result.add(basicEff * Game.factoryEachLevelEfficiencyBonus);
            //// if (!type.isResourceGathering() && this.type.resourceInput.findStorage(province.resource) != null)
            //if (!type.isResourceGathering() && province.isProducingOnFactories(type.resourceInput))
            //    result.add(basicEff * 0.2f); //Game.factoryHaveResourceInProvinceBonus
        }
        return result;
    }
    // Should remove market assamption since its goes to double- calculation?
    public List<Storage> getRealNeeds()
    {
        Value multiplier = new Value(getEfficiency(false).get() * getLevel());

        List<Storage> result = new List<Storage>();

        foreach (Storage next in type.resourceInput)
        {
            Storage nStor = new Storage(next.getProduct(), next.get());
            nStor.multipleInside(multiplier);
            result.Add(nStor);
        }
        return result;
    }
    /// <summary>
    /// Now includes workforce/efficineneece. Here also happening buying dor upgrading\building
    /// </summary>
    override public void consume()
    {
        //if (getLevel() > 0)
        if (isWorking())
        {
            List<Storage> needs = getRealNeeds();

            //todo !CAPITALISM part

            Game.market.Buy(this, new PrimitiveStorageSet(needs));
        }
        if (isUpgrading() || isBuilding())
        {
            bool isBuyingComplete = false;
            daysInConstruction++;
            bool isMarket = Economy.isMarket.checkIftrue(province.owner);// province.owner.isInvented(InventionType.capitalism);
            if (isMarket)
            {
                if (isBuilding())
                    isBuyingComplete = Game.market.Buy(this, needsToUpgrade, Game.BuyInTimeFactoryUpgradeNeeds, type.getBuildNeeds());
                else
                    if (isUpgrading())
                    isBuyingComplete = Game.market.Buy(this, needsToUpgrade, Game.BuyInTimeFactoryUpgradeNeeds, type.getUpgradeNeeds());
                // what if not enough money to complete buildinG?
                float minimalFond = wallet.haveMoney.get() - 50f;

                if (minimalFond < 0 && getOwnerWallet().canPay(minimalFond * -1f))
                    getOwnerWallet().payWithoutRecord(this.wallet, new Value(minimalFond * -1f));
            }
            if (isBuyingComplete || (!isMarket && daysInConstruction == Game.fabricConstructionTimeWithoutCapitalism))
            {
                level++;
                building = false;
                upgrading = false;
                needsToUpgrade.SetZero();
                daysInConstruction = 0;
                reopen(this);
            }
            else if (daysInConstruction == Game.maxDaysBuildingBeforeRemoving)
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
        needsToUpgrade.SetZero();
        daysInConstruction = 0;
    }
    internal void markToDestroy()
    {
        toRemove = true;

        //return loasns only if banking invented
        if (province.owner.isInvented(InventionType.banking))
        {
            Value howMuchToReturn = new Value(loans.get());
            if (howMuchToReturn.get() < wallet.haveMoney.get())
                howMuchToReturn.set(wallet.haveMoney.get());
            province.owner.bank.returnLoan(this, howMuchToReturn);
        }
        wallet.pay(getOwnerWallet(), wallet.haveMoney);
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
        return getExpences() * Factory.xMoneyReservForResources + Game.factoryMoneyReservPerLevel * level;
    }
    internal void PayDividend()
    {
        //if (getLevel() > 0)
        if (isWorking())
        {
            float saveForYourSelf = wantsMinMoneyReserv();
            float pay = wallet.haveMoney.get() - saveForYourSelf;

            if (pay > 0)
                wallet.pay(getOwnerWallet(), new Value(pay));
            if (getProfit() <= 0) // to avoid iternal zero profit factories
            {
                daysUnprofitable++;
                if (daysUnprofitable == Game.maxDaysUnprofitableBeforeFactoryClosing)
                    this.close();
            }
            else
                daysUnprofitable = 0;
        }
        else //closed
        if (!isBuilding())
        {
            daysClosed++;
            if (daysClosed == Game.maxDaysClosedBeforeRemovingFactory)
                markToDestroy();
            else if (Game.random.Next(Game.howOftenCheckForFactoryReopenning) == 1)
            {//take loan for reopen
                if (province.owner.isInvented(InventionType.banking) && this.type.getPossibleProfit(province) > 10f)
                {
                    float leftOver = wallet.haveMoney.get() - wantsMinMoneyReserv();
                    if (leftOver < 0)
                    {
                        Value loanSize = new Value(leftOver * -1f);
                        if (province.owner.bank.CanITakeThisLoan(loanSize))
                            province.owner.bank.TakeLoan(this, loanSize);
                    }
                    leftOver = wallet.haveMoney.get() - wantsMinMoneyReserv();
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
        needsToUpgrade.SetZero();
        daysInConstruction = 0;
    }
    internal void reopen(Owner byWhom)
    {
        working = true;
        if (daysUnprofitable > 20)
            salary.set(province.getLocalMinSalary());
        daysUnprofitable = 0;
        daysClosed = 0;
        if (byWhom != this)
            byWhom.wallet.payWithoutRecord(wallet, getReopenCost());

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
        return !isUpgrading() && !isBuilding() && level < Game.maxFactoryLevel && isWorking();
    }
    internal void upgrade(Owner byWhom)
    {
        upgrading = true;
        needsToUpgrade = type.getUpgradeNeeds().getCopy();
        byWhom.wallet.payWithoutRecord(wallet, getUpgradeCost());
    }

    internal uint getDaysInConstruction()
    {
        return daysInConstruction;
    }

    internal uint getDaysUnprofitable()
    {
        return daysUnprofitable;
    }
    internal uint getDaysClosed()
    {
        return daysClosed;



    }

    /// <summary>Returns False if something failed</summary>
    internal bool getConditionsForFactorySubsidize(Country country, bool fastReturn, out string description)
    {
        string result = null;
        bool atLeastOneNoAnswer = false;
        // - first lines - why we cant

        if (province.owner.economy.status == Economy.LaissezFaire)
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Economy politics is Laissez Faire";
            atLeastOneNoAnswer = true;
        }
        else
            result += "\n(+) Economy politics is not Laissez Faire";
        description = result;
        if (atLeastOneNoAnswer) return false;
        else
            return true;
    }

    internal bool getConditionsForFactoryUpgradeFast(Country country)
    {
        return getConditionsForFactoryUpgradeT(country, true, out Game.dumpString);
    }
    internal bool getConditionsForFactoryUpgrade(Country country, out string description)
    {
        return getConditionsForFactoryUpgradeT(country, false, out description);
    }
    /// <summary>Returns False if something failed</summary>
    bool getConditionsForFactoryUpgradeT(Country country, bool fastReturn, out string description)
    {
        string result = null;
        bool atLeastOneNoAnswer = false;
        // - first lines - why we cant
        if (province.owner.economy.status == Economy.LaissezFaire)
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Economy politics is Laissez Faire";
            atLeastOneNoAnswer = true;
        }
        else
            result += "\n(+) Economy politics is not Laissez Faire";

        if (isUpgrading())
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Already upgrading";
            atLeastOneNoAnswer = true;
        }

        if (isBuilding())
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Not builded yet";
            atLeastOneNoAnswer = true;
        }
        if (!isWorking())
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Closed";
            atLeastOneNoAnswer = true;
        }
        if (level == Game.maxFactoryLevel)
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Max level achieved";
            atLeastOneNoAnswer = true;
        }

        Value cost = this.getUpgradeCost();
        if (!country.wallet.canPay(cost))// no money
        {
            if (fastReturn) { description = null; return false; }
            result += "\n(-) Not enough " + (cost.get() - country.wallet.haveMoney.get()) + " money";
            atLeastOneNoAnswer = true;
        }
        else
            result += "\n(+) Have money " + cost;
        //if (level == Game.maxFactoryLevel) // no resources
        //    result += "\nMax level achieved";
        description = result;
        if (atLeastOneNoAnswer) return false;
        else
            return true;
    }
    /// <summary>Return null if there is no obstacles </summary>    
    internal string whyCantUpgradeFactory(Country country)
    {
        string result = null;
        if (province.owner.economy.status == Economy.LaissezFaire)
            result += "\n(-) Economy politics - LaissezFaire";
        if (isUpgrading())
            result += "\n(-) Already upgrading";
        if (isBuilding())
            result += "\n(-) Not builded yet";
        if (!isWorking())
            result += "\n(-) Closed";
        if (level == Game.maxFactoryLevel)
            result += "\n(-) Max level achieved";
        //var resourceToBuild = this.type.getUpgradeNeeds();
        Value cost = this.getUpgradeCost();
        if (!country.wallet.canPay(cost))// no money
            //country.wallet.CanAfford()
            result += "\nNot enough " + (cost.get() - country.wallet.haveMoney.get()) + " money";
        //if (level == Game.maxFactoryLevel) // no resources
        //    result += "\nMax level achieved";
        return result;
    }
    /// <summary>Return null if there is no obstacles </summary>    
    internal string whyCantDestroyFactory()
    {
        string result = null;
        if (province.owner.economy.status == Economy.LaissezFaire)
            result += "\n(-) Economy politics - LaissezFaire";
        //if (isUpgrading())
        //    result += "\n(-) Already constructing";
        return result;
    }
    /// <summary>Return null if there is no obstacles </summary>    
    internal string whyCantCloseFactory()
    {
        string result = null;
        if (province.owner.economy.status == Economy.LaissezFaire)
            result += "\n(-) Economy politics - LaissezFaire";
        if (isBuilding())
            result += "\n(-) Not opened yet";
        if (!isWorking())
            result += "\n(-) Already closed";
        return result;
    }
    /// <summary>Return null if there is no obstacles </summary>    
    internal string whyCantReopenFactory()
    {
        string result = null;
        if (province.owner.economy.status == Economy.LaissezFaire)
            result += "\n(-) Economy politics - LaissezFaire";
        if (isWorking())
            result += "\n(-) Already opened";
        if (isBuilding())
            result += "\n(-) Not builded yet";
        return result;
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
//        uint workers = getWorkForce();
//        if (workers > 0)
//        {
//            Value producedAmount;
//            producedAmount = new Value(getWorkForce() * type.basicProduction.get() / 1000f);
//            storageNow.add(producedAmount);
//            gainGoodsThisTurn.set(producedAmount);

//            if (province.owner.capitalism.Invented())
//            {

//                this.wallet.ConvertFromGoldAndAdd(storageNow);
//                //send 50% to government

//                wallet.pay(province.owner.wallet, new Value(wallet.moneyIncomethisTurn.get() / 2f));
//            }
//            else // send all production to owner
//                storageNow.sendAll(province.owner.storageSet);
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

public class PopLinkage
{
    public PopUnit pop;
    public uint amount;
    internal PopLinkage(PopUnit p, uint a)
    {
        pop = p;
        amount = a;
    }
}
public class Owner
{
    /// <summary>
    /// money should be here??
    /// </summary>
    public Wallet wallet = new Wallet(0f);
}

public abstract class Producer : Owner
{    /// <summary>How much product actually left for now. Goes to zero each turn. Early used for food storage (without capitalism)</summary>
    public Storage storageNow;

    /// <summary>How much was gained (before any payments). Not money!! Generally, gets value in POpunit.produce and Factore.Produce </summary>
    public Storage gainGoodsThisTurn;

    /// <summary>How much sent to market, Some other amount could be consumedTotal or stored for future </summary>
    public Storage sentToMarket;

    internal Value loans = new Value(0);
    public PrimitiveStorageSet consumedTotal = new PrimitiveStorageSet();
    public PrimitiveStorageSet consumedLastTurn = new PrimitiveStorageSet();
    public PrimitiveStorageSet consumedInMarket = new PrimitiveStorageSet();

    //protected Country owner; //TODO Could be any Country or POP
    public Province province;

    /// <summary> /// Return in pieces  /// </summary>    
    abstract internal float getLocalEffectiveDemand(Product product);
    public abstract void simulate();
    public abstract void produce();
    public abstract void consume();
    public abstract void payTaxes();

    public void getMoneyFromMarket()
    {
        if (sentToMarket.get() > 0f)
        {
            Value DSB = new Value(Game.market.getDemandSupplyBalance(sentToMarket.getProduct()));
            if (DSB.get() > 1f) DSB.set(1f);
            Storage realSold = new Storage(sentToMarket);
            realSold.multipleInside(DSB);
            float cost = Game.market.getCost(realSold);
            storageNow.add(gainGoodsThisTurn.get() - realSold.get());//!!
            if (Game.market.wallet.canPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
            {
                Game.market.wallet.pay(this.wallet, new Value(cost));
                Game.market.tmpMarketStorage.subtract(realSold);
            }
            else
                Debug.Log("Failed market - producer payment"); // money in market endded... Only first lucky get money
        }
    }
}

