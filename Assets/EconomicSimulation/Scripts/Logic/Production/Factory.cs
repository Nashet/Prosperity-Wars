﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Factory : SimpleProduction, IClickable, IInvestable, IShareable, INameable
    {
        public enum Priority { none, low, medium, high }

        public static readonly int workForcePerLevel = 1000;
        private static readonly int xMoneyReservForResources = 10;

        private int level;
        private bool building = true;
        private bool upgrading;
        private bool _isOpen;
        private bool toRemove;

        private readonly Money payedDividends = new Money(0m);
        private bool dontHireOnSubsidies, subsidized;
        private int priority;
        private readonly Money salary = new Money(0);

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
        public readonly Owners ownership;

        /// <summary>used only on initial factory building</summary>
        //private bool buildByPlannedEconomy;
        private IShareOwner currentInvestor;

        private readonly Procent averageWorkersEducation = new Procent(0f);

        /// <summary>
        /// new Value
        /// </summary>
        public Procent AverageWorkersEducation
        {
            get
            {
                if (hiredWorkForce.Count == 0)
                    //return Province.GetAveragePop(x => x.Education);
                    return Province.GetAllPopulation().Where(x => x.Type == PopType.Workers).GetAverageProcent(x => x.Education);
                else
                    return averageWorkersEducation.Copy();
            }
        }

        internal static readonly Modifier
            modifierHasResourceInProvince = new Modifier(x => !(x as Factory).Type.isResourceGathering() &&
            ((x as Factory).Province.isProducingOnEnterprises((x as Factory).Type.resourceInput)
            || ((x as Factory).Province.getResource() == Product.Grain && (x as Factory).Type == ProductionType.Barnyard)
            ),
                  "Has input resource in this province", 0.35f, false),

            modifierConcentrationBonus = new Modifier(
            //(x as Factory).GetEmploymentLevel() - 1) / 100f
            delegate (object x)
            {
                var isFactory = x as Factory;
                if (isFactory.Type.IsRural || isFactory.Type == ProductionType.GoldMine)
                    return 0f;
                else
                    return isFactory.GetEmploymentLevel() - 1f;
            },
                "High production concentration bonus", 0.25f, false),

            modifierInventedMiningAndIsShaft = new Modifier(x => (x as Factory).Country.Invented(Invention.Mining) && (x as Factory).Type.isShaft(),
               new StringBuilder("Invented ").Append(Invention.Mining).ToString(), 0.50f, false),

            modifierBelongsToCountry = new Modifier(x => (x as Factory).ownership.IsCountryOwnsControlPacket(), "Control packet belongs to government", -0.35f, false),
            modifierIsSubsidised = new Modifier(x => (x as Factory).isSubsidized(), "Is subsidized", -0.20f, false);

        internal static readonly Condition
            conNotFullyBelongsToCountry = new DoubleCondition((agent, factory) => !(factory as Factory).ownership.IsOnlyOwner(agent as IShareOwner), x => "Doesn't fully belongs to government", false),

            conNotUpgrading = new DoubleCondition((agent, factory) => !(factory as Factory).isUpgrading(), factory => "Not upgrading", false),
            conNotBuilding = new DoubleCondition((agent, factory) => !(factory as Factory).isBuilding(), factory => "Not building", false),
            conOpen = new DoubleCondition((agent, factory) => (factory as Factory).IsOpen, factory => "Open", false),
            conClosed = new DoubleCondition((agent, factory) => !(factory as Factory).IsOpen, factory => "Closed", false),
            conMaxLevelAchieved = new DoubleCondition((agent, factory) => (factory as Factory).getLevel() != Options.maxFactoryLevel, factory => "Max level not achieved", false),
            conUpgradeProductsInventedByAnyone = new DoubleCondition((agent, factory) => (factory as Factory).getUpgradeNeeds().All(y => y.Product.IsInventedByAnyOne()),  //Game.market.isAvailable( y.Product)),
                factory => "All upgrade products are invented", false),

            conPlayerHaveMoneyToReopen = new DoubleCondition((agent, factory) => Game.Player.CanPay((factory as Factory).getReopenCost()), delegate
            {
                var sb = new StringBuilder();
                //sb.Append("Have ").Append((x as Factory).getReopenCost());
                sb.Append("Have enough money");
                return sb.ToString();
            }, true);

        internal static readonly DoubleCondition
            conHaveMoneyOrResourcesToUpgrade = new DoubleCondition(
                delegate (object upgrader, object factory)
                {
                    if (upgrader == Game.Player)
                    {
                        var agent = upgrader as Agent;
                        var typedfactory = factory as Factory;
                        if (agent.Country.economy.getValue() == Economy.PlannedEconomy)
                        {
                            return agent.Country.countryStorageSet.has(typedfactory.getUpgradeNeeds());
                        }
                        else
                        {
                            MoneyView cost = Game.market.getCost(typedfactory.getUpgradeNeeds());
                            return agent.CanPay(cost);
                        }
                    }
                    else
                        return true; // Assuming it would be checked somewhere else
                },

                delegate
                {
                    //var sb = new StringBuilder();
                    //var factory = x as Factory;
                    //MoneyView cost = Game.market.getCost(factory.getUpgradeNeeds());
                    //sb.Append("Have ").Append(cost).Append(" coins");
                    //sb.Append(" or (with ").Append(Economy.PlannedEconomy).Append(") have ").Append(factory.getUpgradeNeeds().getString(", "));
                    //return sb.ToString();
                    return "Have enough money or (with Planned Economy) have enough resources";
                }
                , true),
            conPlacedInOurCountry = new DoubleCondition((agent, factory) => (factory as Factory).Country == (agent as Consumer).Country,
            factory => "Enterprise placed in our country", true),
            ///duplicated in FactoryTypes
            conAllowsForeignInvestments = new DoubleCondition((agent, factory) =>
                agent == null
                || (factory as Factory).Country == (agent as Country)
                || ((factory as Factory).Country.economy.getTypedValue().AllowForeignInvestments
                    && agent is Country
                    && (agent as Country).economy.getTypedValue() != Economy.PlannedEconomy),
                factory => "Country allows foreign investments or it isn't foreign investment", true),//(factory as Factory).Country+
            conNotLForNotCountry = new DoubleCondition((agent, factory) => agent == null || !(agent is Country) || (agent as Country).economy.getValue() != Economy.LaissezFaire, factory => "Economy policy is not Laissez Faire", true)
            ;

        internal static readonly DoubleConditionsList
            conditionsUpgrade = new DoubleConditionsList(new List<Condition>
            {// thats a universal condition now, be careful
                conNotUpgrading, conNotBuilding, conOpen, conMaxLevelAchieved, conUpgradeProductsInventedByAnyone,
                conNotLForNotCountry, conAllowsForeignInvestments, conHaveMoneyOrResourcesToUpgrade
            }),
            conditionsClose = new DoubleConditionsList(new List<Condition> { conNotBuilding, conOpen, conPlacedInOurCountry, conNotLForNotCountry }),
            conditionsReopen = new DoubleConditionsList(new List<Condition> { conNotBuilding, conClosed, conPlayerHaveMoneyToReopen, conAllowsForeignInvestments, conNotLForNotCountry }),
            conditionsDestroy = new DoubleConditionsList(new List<Condition> {
            //new Condition(Economy.isNotLF, x=>(x as Producer).Country),
             conPlacedInOurCountry,  Economy.isNotLF }),//}).addForSecondObject(new List<Condition> {
            // (status == Economy.PlannedEconomy || status == Economy.NaturalEconomy || status == Economy.StateCapitalism)
            conditionsNatinalize = new DoubleConditionsList(new List<Condition> { conNotFullyBelongsToCountry, conPlacedInOurCountry,
                Economy.isNotLF, Economy.isNotInterventionism }),//}) .addForSecondObject(new List<Condition> {
            conditionsSubsidize = new DoubleConditionsList(new List<Condition> { conPlacedInOurCountry ,Economy.isNotLF, Economy.isNotNatural,
                Economy.isNotPlanned }),//}).addForSecondObject(new List<Condition> {
            conditionsDontHireOnSubsidies = new DoubleConditionsList(new List<Condition> { conPlacedInOurCountry, Economy.isNotLF,
                Economy.isNotNatural, Condition.IsNotImplemented }),//})            .addForSecondObject(new List<Condition> {
            conditionsChangePriority = new DoubleConditionsList(new List<Condition> { conPlacedInOurCountry, Economy.isPlanned });//})            .addForSecondObject(new List<Condition> {

        internal static readonly DoubleConditionsList
            conditionsSell = new DoubleConditionsList(new List<Condition> {Economy.isNotPlanned, //todo temporally removed , Economy.isNotState
            new DoubleCondition((agent, factory)=>(factory as Factory).ownership.HasOwner(agent as IShareOwner), x=>"Has something to sale", false)
            }),

            conditionsBuy = new DoubleConditionsList(new List<Condition> {Economy.isNotLF, Economy.isNotPlanned,
                new DoubleCondition ((agent, factory)=>(factory as Factory).ownership.IsOnSale(), x=>"Is on sale", true),
                new DoubleCondition ((agent, factory)=> (agent as Agent).CanPay( (factory as Factory).ownership.GetShareMarketValue(Options.PopBuyAssetsAtTime) ),
                    x=> "Have money to buy share", false),
                conAllowsForeignInvestments
            })
            ;

        internal static readonly ModifiersList
            modifierEfficiency = new ModifiersList(new List<Condition>
            {
           Modifier.modifierDefault1,
            new Modifier(Invention.SteamPowerInvented, x => (x as Factory).Country, 0.25f, false),
            new Modifier(Invention.CombustionEngineInvented, x => (x as Factory).Country, 0.5f, false),

            new Modifier(Economy.isStateCapitlism, x => (x as Factory).Country,  0.10f, false),
            new Modifier(Economy.isInterventionism, x => (x as Factory).Country,  0.30f, false),
            new Modifier(Economy.isLF, x => (x as Factory).Country, 0.50f, false),
            new Modifier(Economy.isPlanned, x => (x as Factory).Country, -0.10f, false),
            new Modifier(x=>
            {
                var factory = x as Factory;
                if (factory.Type == ProductionType.University)
                    return Mathf.Max((factory.AverageWorkersEducation.get() - 0.5f)  * 4f, -0.5f);
                else if (factory.Type.isResourceGathering())
                    return factory.AverageWorkersEducation.get() / 10f *2;
                else
                    return factory.AverageWorkersEducation.get();
            }, "Average workforce education", 1f, false),

            modifierInventedMiningAndIsShaft,
            modifierHasResourceInProvince,
            modifierConcentrationBonus, modifierBelongsToCountry, modifierIsSubsidised,
            // copied in popUnit
             new Modifier(x => Government.isPolis.checkIfTrue((x as Factory).Country)
             && (x as Factory).Country.Capital == (x as Factory).Province, "Capital of Polis", 0.50f, false),
             new Modifier(x=>(x as Factory).Province.hasModifier(TemporaryModifier.recentlyConquered), TemporaryModifier.recentlyConquered.ToString(), -0.20f, false),
             new Modifier(Government.isTribal, x=>(x as Factory).Country, -0.3f, false),
             new Modifier(Government.isDespotism, x=>(x as Factory).Country, -0.20f, false), // remove this?
             new Modifier(x=>!(x as Factory).Country.InventedFactory((x as Factory).Type), "Uses uninvented technologies", -0.3f, false)
            });

        /// <summary>
        /// Don't call it directly
        /// </summary>

        public Factory(Province province, IShareOwner investor, ProductionType type, MoneyView cost)
            : base(type, province)
        {
            //this.buildByPlannedEconomy = buildByPlannedEconomy;
            ownership = new Owners(this);
            if (investor != null) // that mean that factory is a fake
            {
                currentInvestor = investor;
                //assuming this is level 0 building
                constructionNeeds = new StorageSet(Type.GetBuildNeeds());

                ownership.Add(investor, cost);

                salary.Set(province.getLocalMinSalary());
                if (Country.economy.getValue() == Economy.PlannedEconomy)
                    setPriorityAutoWithPlannedEconomy();
                //else
                //    Debug.Log(investor + " invested " + cost + " in building new " + this);
            }
        }

        public MoneyView GetInvestmentCost()
        {
            if (IsOpen)
            {
                var res = Game.market.getCost(getUpgradeNeeds()).Copy();
                res.Add(Options.factoryMoneyReservePerLevel);
                return res;
            }
            else
                return getReopenCost();
        }

        /// <summary>
        /// Returns copy
        /// </summary>
        internal List<Storage> getUpgradeNeeds()
        {
            if (getLevel() < Options.FactoryMediumTierLevels)
                return Type.GetUpgradeNeeds(1);
            else
                if (getLevel() < Options.FactoryMediumHighLevels)
                return Type.GetUpgradeNeeds(2);
            else
                return Type.GetUpgradeNeeds(3);
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

        /// <summary>
        /// Level based on hired worker
        /// </summary>
        public int GetEmploymentLevel()
        {
            var res = getWorkForce() / workForcePerLevel;// forget about remainder
            if (res == 0 && level > 0)
                res++;
            return res;
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

        public override string ToString()
        {
            return FullName;
        }

        public string FullName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(ShortName).Append(" in ").Append(Province.FullName);
                return sb.ToString();
            }
        }

        public string ShortName
        {
            get { return Type.name + " L" + getLevel(); }
        }

        //abstract internal string getName();
        public override void simulate()
        {
            //hireWorkForce();
            //produce();

            //paySalary();
            //consume();
        }

        public void ClearWorkforce()
        {
            averageWorkersEducation.SetZero();
            hiredWorkForce.Clear();
        }

        /// <summary>
        /// returns how much factory hired in reality
        /// </summary>
        public int hireWorkers(int amount, IEnumerable<PopUnit> popList)
        {
            //check on no too much workers?
            //if (amount > HowMuchWorkForceWants())
            //    amount = HowMuchWorkForceWants();
            int wasWorkforce = getWorkForce();
            if (amount > 0 && wasWorkforce == 0)
                justHiredPeople = true;
            else
                justHiredPeople = false;

            ClearWorkforce();
            if (amount > 0)
            {
                int leftToHire = amount;
                hiredLastTurn = 0;
                popList = popList.OrderByDescending(x => x.Education.get()).ThenBy(x => x.getPopulation()).ToList();

                foreach (Workers pop in popList)
                {
                    if (pop.GetUnemployedPopulation() >= leftToHire) // satisfied demand
                    {
                        hiredWorkForce.Add(pop, leftToHire);
                        pop.Hire(this, leftToHire);
                        //hiredLastTurn = getWorkForce() - wasWorkforce;

                        averageWorkersEducation.AddPoportionally(hiredLastTurn, leftToHire, pop.Education);
                        hiredLastTurn += leftToHire;
                        return hiredLastTurn;
                        //break;
                    }
                    else
                    {
                        var toHire = pop.GetUnemployedPopulation();
                        hiredWorkForce.Add(pop, toHire); // hire everyone left
                        pop.Hire(this, toHire);
                        averageWorkersEducation.AddPoportionally(hiredLastTurn, toHire, pop.Education);
                        hiredLastTurn += toHire;
                        leftToHire -= toHire;
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
            if (Type.basicProduction.Product.isIndustrial())
                setPriority(Priority.medium);
            else
            {
                if (Type.basicProduction.Product.isMilitary())
                    setPriority(Priority.low);
                else //isConsumer()
                    setPriority(Priority.none);
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

        /// <summary>
        /// Returns new value. Includes tax, salary and modifiers. New value
        /// </summary>
        public Procent GetMargin()
        {
            return GetMargin(false);
        }

        /// <summary>
        /// Returns new value. Includes tax, salary and modifiers. New value
        /// </summary>
        private Procent GetMargin(bool basedOnProfit)
        {
            if (Country.economy.getValue() == Economy.PlannedEconomy)
                return Procent.ZeroProcent.Copy();
            else
            {
                if (IsClosed)
                    return Type.GetPossibleMargin(Province);//potential margin
                else
                {
                    Money income;
                    if (basedOnProfit)
                        income = new Money((decimal)getProfit(), false);
                    else
                        income = payedDividends.Copy();
                    var taxes = income.Copy().Multiply(Country.taxationForRich.getTypedValue().tax);
                    income.Subtract(taxes);
                    return new Procent(income, ownership.GetMarketValue(), false);
                }
            }
        }

        internal MoneyView getReopenCost()
        {
            return Options.factoryMoneyReservePerLevel;
        }

        internal int HowManyEmployed(PopUnit pop)
        {
            int result = 0;
            //foreach (var link in hiredWorkForce)
            //    if (link.Key == pop)
            //        result += link.Value;
            hiredWorkForce.TryGetValue(pop, out result);

            return result;
        }

        //internal bool isThereMoreWorkersToHire()
        //{
        //    int totalAmountWorkers = Province.getPopulationAmountByType(PopType.Workers);
        //    int result = totalAmountWorkers - getWorkForce();
        //    return (result > 0);
        //}
        internal int GetVacancies()
        {
            return getMaxWorkforceCapacity() - getWorkForce();
        }

        //internal bool IsTherePossibilityToHireMore()
        //{
        //    //if there is other pops && there is space on factory
        //    if (GetVacancies() > 0 && isThereMoreWorkersToHire())
        //        return true;
        //    else
        //        return false;
        //}
        //internal void LearnByWorking()
        //{
        //    if (IsOpen && !Type.isResourceGathering() && Rand.Chance(Options.PopLearnByWorkingChance))
        //        foreach (var employee in hiredWorkForce)
        //            if (employee.Value > employee.Key.getPopulation() * 0.75f)
        //                employee.Key.LearnByWork();
        //}
        internal void paySalary()
        {
            if (IsOpen && Country.economy.getValue() != Economy.PlannedEconomy)
            {
                // per 1000 men
                if (Economy.isMarket.checkIfTrue(Country))
                    foreach (var employee in hiredWorkForce)
                    {
                        MoneyView howMuchPay = salary.Copy().Multiply(employee.Value).Divide(workForcePerLevel);
                        if (CanPay(howMuchPay))
                            Pay(employee.Key, howMuchPay);
                        else if (isSubsidized()
                        && Country.GiveFactorySubsidies(this, HowMuchLacksMoneyIncludingDeposits(howMuchPay))) //take money and try again
                            Pay(employee.Key, howMuchPay);
                        else
                        {
                            //todo else don't pay if there is nothing to pay
                            close();
                            return;
                        }
                    }
                // don't pay nothing if where is planned economy
                else if (Country.economy.getValue() == Economy.NaturalEconomy)
                {
                    //todo natural e.
                    // non market!!
                    //Storage foodSalary = new Storage(Product.Grain, 1f);
                    //foreach (var link in hiredWorkForce)
                    //{
                    //    Storage howMuchPay = new Storage(foodSalary.Product, foodSalary.get() * link.Value / (float)workForcePerLevel);
                    //    Country countryPayer = getOwner() as Country;
                    //    if (countryPayer != null)
                    //    {
                    //        if (countryPayer.countryStorageSet.has(howMuchPay))
                    //        {
                    //            countryPayer.countryStorageSet.send(link.Key, howMuchPay);
                    //            link.Key.addProduct(howMuchPay); // todo fails if is abstract
                    //            salary.set(foodSalary);
                    //        }
                    //        //todo no salary cuts yet
                    //        //else salary.set(0);
                    //    }
                    //    else // assuming - PopUnit
                    //    {
                    //        PopUnit popPayer = getOwner() as PopUnit;

                    //        if (popPayer.storage.has(howMuchPay))
                    //        {
                    //            popPayer.storage.send(link.Key.storage, howMuchPay);
                    //            link.Key.addProduct(howMuchPay);
                    //            salary.set(foodSalary);
                    //        }
                    //        //todo no resources to pay salary
                    //        //else salary.set(0);
                    //    }
                    //    //else dont pay if there is nothing to pay
                    //}
                }
            }
        }

        /// <summary> only make sense if called before HireWorkforce()
        ///  PEr 1000 men!!!
        /// !!! Mirroring PaySalary
        /// Returns new value
        /// </summary>
        public Money getSalary()
        {
            return salary.Copy();
        }

        public int getMaxWorkforceCapacity()
        {
            int cantakeMax = level * workForcePerLevel;
            return cantakeMax;
        }

        /// <summary>
        /// Enterprise rises salary if labor demand is bigger than labor supply (assuming enterprise is profitable). And vice versa - enterprise lowers salary if labor demand is lower than labor supply.
        ///More profitable enterprise rises salary faster.
        /// </summary>
        public void ChangeSalary()
        {
            //Should be rise salary if: small unemployment, has profit, need has other resources

            if (IsOpen && Economy.isMarket.checkIfTrue(Country))
            {
                var unemployment = Province.GetAllPopulation().Where(x => x.Type == PopType.Workers).GetAverageProcent(x => x.getUnemployment());
                var margin = GetMargin(true);

                // rise salary to attract  workforce, including workforce from other factories
                if (margin.isBiggerThan(Options.minMarginToRiseSalary)
                    && unemployment.isSmallerThan(Options.ProvinceLackWorkforce) //demand >= supply
                    && GetVacancies() > 10)// && getInputFactor() == 1)
                {
                    // cant catch up salaries like that. Check for zero workforce?
                    decimal salaryRaise = 0.001m; //1%
                    if (margin.get() > 1000f) //100000%
                        salaryRaise = 0.800m;
                    else if (margin.get() > 100f) //10000%
                        salaryRaise = 0.200m;
                    else if (margin.get() > 10f) //1000%
                        salaryRaise = 0.048m;
                    else if (margin.get() > 1f) //100%
                        salaryRaise = 0.012m;
                    else if (margin.get() > 0.3f) //30%
                        salaryRaise = 0.003m;
                    else if (margin.get() > 0.1f) //10%
                        salaryRaise = 0.002m;
                    else if (margin.get() >= 0.01f) //1%
                        salaryRaise = 0.001m;

                    salary.Add(salaryRaise);
                }

                // Reduce salary on non-profit
                if (margin.isZero()
                    && daysUnprofitable >= Options.minDaysBeforeSalaryCut
                    && !isJustHiredPeople() && !isSubsidized())
                    salary.Subtract(Options.FactoryReduceSalaryOnNonProfit, false);

                // if supply > demand
                if (unemployment.isBiggerThan(Options.ProvinceExcessWorkforce))
                    salary.Subtract(Options.FactoryReduceSalaryOnMarket, false);

                if (getWorkForce() == 0)// && getInputFactor() == 1)
                    salary.Set(Province.getLocalMinSalary());
                // to help factories catch up other factories salaries
                //    salary.set(province.getLocalMinSalary());
                // freshly built factories should rise salary to concurrency with old ones
                //if (getWorkForce() < 100 && Province.getUnemployedWorkers() == 0 && this.Cash.get() > 10f)// && getInputFactor() == 1)
                //    //salary.set(province.getLocalMinSalary());
                //    salary.add(0.09f);

                // limit salary country's min wage
                var minSalary = Country.getMinSalary();
                if (salary.isSmallerThan(minSalary))
                    salary.Set(minSalary);
            }
        }

        /// <summary>
        /// Use it for PE only!
        /// </summary>
        public void setZeroSalary()
        {
            salary.SetZero();
        }

        private int getMaxHiringSpeed()
        {
            return Options.maxFactoryFireHireSpeed * getLevel();
        }

        /// <summary>
        ///
        /// </summary>
        public int howMuchWorkForceWants()
        {
            if (IsClosed)
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

                if (Country.economy.getValue() != Economy.PlannedEconomy)// commies don't care about profits
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
            //if (result > wants)
            //    result = wants;
            if (result < 0)
                return 0;
            return result;
        }

        internal int getHowMuchHiredLastTurn()
        {
            return hiredLastTurn;
        }

        public Procent GetWorkForceFulFilling()
        {
            return new Procent(getWorkForce(), workForcePerLevel * level, false);
        }

        public override List<Storage> getRealAllNeeds()
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
            Procent workforceProcent = GetWorkForceFulFilling();
            Procent inputFactor = getInputFactor();
            if (inputFactor.isZero() & isJustHiredPeople())
                inputFactor = Procent.HundredProcent.Copy();

            if (inputFactor.isSmallerThan(workforceProcent))
                efficencyFactor = inputFactor;
            else
                efficencyFactor = workforceProcent;
            //float basicEff = efficencyFactor * getLevel();
            //Procent result = new Procent(basicEff);
            //Procent result = new Procent(efficencyFactor);
            if (useBonuses)
                efficencyFactor.Set(efficencyFactor.get() * modifierEfficiency.getModifier(this), false);
            return efficencyFactor;
        }

        private void stopUpgrading()
        {
            currentInvestor = null;
            building = false;
            upgrading = false;
            constructionNeeds.setZero();
            daysInConstruction = 0;
        }

        internal void markToDestroy()
        {
            toRemove = true;

            //return loans only if banking invented
            if (Country.Invented(Invention.Banking))
            {
                if (loans.isNotZero())
                {
                    Money howMuchToReturn = loans.Copy();
                    if (howMuchToReturn.isSmallerOrEqual(Cash))
                        howMuchToReturn.Set(Cash);
                    Bank.ReceiveMoney(this, howMuchToReturn);
                    if (loans.isNotZero())
                        Bank.OnLoanerRefusesToPay(this);
                }
            }
            // send remaining money to owners
            foreach (var item in ownership.GetAllShares())
            {
                Pay(item.Key as Agent, Cash.Copy().Multiply(item.Value), false);
                //pay(item.Key as Agent, item.Value.SendProcentOf(Cash), false);
            }

            MainCamera.factoryPanel.removeFactory(this);
        }

        internal void destroyImmediately()
        {
            markToDestroy();
            Province.DestroyFactory(this);
            // + GUI 2 places
            MainCamera.factoryPanel.removeFactory(this);
            //MainCamera.productionWindow.removeFactory(this);
            MainCamera.productionWindow.Refresh();
        }

        internal bool isToRemove()
        {
            return toRemove;
        }

        /// <summary>
        ///new value
        /// </summary>
        private MoneyView wantsMinMoneyReserv()
        {
            return (getExpences().Copy()).Multiply(xMoneyReservForResources).Add(
                Options.factoryMoneyReservePerLevel.Copy().Multiply(level)
                );
            //return getExpences().get() * Factory.xMoneyReservForResources + Options.factoryMoneyReservePerLevel * level;
        }

        public void CloseUnprofitable()
        {
            if (getProfit() <= 0)
            {
                daysUnprofitable++;
                if (daysUnprofitable == Options.maxDaysUnprofitableBeforeFactoryClosing && !isSubsidized())
                    close();
                if (IsClosed)
                {
                    daysClosed++;
                    if (daysClosed == Options.maxDaysClosedBeforeRemovingFactory)
                        markToDestroy();
                }
            }
            else
                daysUnprofitable = 0;
        }

        public void OpenFactoriesPE()
        {
            if (IsClosed && !isBuilding() && Country.economy.getValue() == Economy.PlannedEconomy)
            {
                Rand.Call(() => open(Country, false), Options.howOftenCheckForFactoryReopenning);
            }
        }

        /// <summary>
        /// New value, readonly
        /// </summary>
        public Money GetDividends()
        {
            return payedDividends.Copy();
        }

        internal void payDividend()
        {
            if (IsOpen)
            {
                MoneyView dividends = Cash.Copy().Subtract(wantsMinMoneyReserv(), false);
                //new Value(Cash.get() - wantsMinMoneyReserv(), false);
                payedDividends.Set(dividends);

                if (dividends.isNotZero())
                {
                    // pay to each owner
                    foreach (var item in ownership.GetAllShares())
                    {
                        var owner = item.Key as Agent;
                        MoneyView sentToOwner = dividends.Copy().Multiply(item.Value);
                        //Value sentToOwner = item.Value.SendProcentOf(dividends);
                        Pay(owner, sentToOwner);
                        //Country.TakeIncomeTax(owner, sentToOwner, false);
                        var isCountry = item.Key as Country;
                        if (isCountry != null)
                            isCountry.ownedFactoriesIncomeAdd(sentToOwner);
                    }
                }
            }
        }

        internal void close()
        {
            _isOpen = false;
            upgrading = false;
            constructionNeeds.setZero();
            daysInConstruction = 0;
        }

        internal void open(IShareOwner byWhom, bool payMoney)
        {
            var agent = byWhom as Agent;
            if (agent.Country.economy.getValue() != Economy.PlannedEconomy)
                salary.Set(Province.getLocalMinSalary());
            if (payMoney)
            {
                agent.PayWithoutRecord(this, getReopenCost());
                ownership.Add(byWhom, getReopenCost());
                //Debug.Log(byWhom + " invested " + getReopenCost() + " in reopening " + this);
            }
            _isOpen = true;
            daysUnprofitable = 0;
            daysClosed = 0;
        }

        /// <summary>
        /// Enterprise finished building and makes business
        /// </summary>
        internal bool IsOpen
        {
            get { return _isOpen; }
        }

        internal bool IsClosed
        {
            get { return !_isOpen; }
        }

        //internal bool IsClosed()
        //{
        //    return !_isOpen;
        //}
        /// <summary>
        /// New value
        /// </summary>
        /// <returns></returns>
        internal Money getSalaryCost()
        {
            return getSalary().Multiply(getWorkForce()).Divide(workForcePerLevel);
        }

        //internal bool canUpgrade()
        //{
        //    return !isUpgrading() && !isBuilding() && level < Options.maxFactoryLevel && IsOpen;
        //}

        internal void upgrade(IShareOwner byWhom)
        {
            currentInvestor = byWhom;
            upgrading = true;
            constructionNeeds.Add(getUpgradeNeeds());
            if ((byWhom as Agent).Country.economy.getValue() != Economy.PlannedEconomy)
            {
                var cost = Game.market.getCost(getUpgradeNeeds());
                (byWhom as Agent).PayWithoutRecord(this, cost);
                ownership.Add(byWhom, cost);
                //Debug.Log(byWhom + " invested " + cost + " in upgrading " + this);
            }
            //else
            //    Debug.Log(byWhom + " invested in upgrading " + this);
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

        internal override float getProfit()
        {
            if (Country.economy.getValue() == Economy.PlannedEconomy)
                return 0f;
            else
                return base.getProfit() - (float)getSalaryCost().Get();
        }

        public override List<Storage> getHowMuchInputProductsReservesWants()
        {
            //if (Country.economy.getValue() == Economy.PlannedEconomy)
            //    return getHowMuchInputProductsReservesWants(new Value(getWorkForceFulFilling().get() * getLevel())); // only 1 day reserves with PE
            //else
            return getHowMuchInputProductsReservesWants(new Value(GetWorkForceFulFilling().get() * getLevel() * Options.FactoryInputReservInDays));
        }

        internal override Procent getInputFactor()
        {
            return getInputFactor(GetWorkForceFulFilling());
        }

        /// <summary>
        /// Fills storageNow and gainGoodsThisTurn
        /// </summary>
        public override void produce()
        {
            if (IsOpen)
            {
                int workers = getWorkForce();
                if (workers > 0)
                    base.produce(new Value(getEfficiency(true).get() * getLevel()));

                if (Type == ProductionType.GoldMine)
                {
                    GiveMoneyFromGoldPit(storage);
                }
                else
                {
                    if (Economy.isMarket.checkIfTrue(Country))
                        sell(getGainGoodsThisTurn());
                    else if (Country.economy.getValue() == Economy.NaturalEconomy)
                    {
                        // todo Send product proportionally to all owners? with NE?
                        //Country countryOwner = getOwner() as Country;
                        //if (countryOwner != null)
                        //    storage.sendAll(countryOwner.countryStorageSet);
                        //else // assuming owner is aristocrat/capitalist
                        {
                            sell(getGainGoodsThisTurn());
                        }
                    }
                    else if (Country.economy.getValue() == Economy.PlannedEconomy)
                    {
                        storage.sendAll(Country.countryStorageSet);
                    }
                }
            }
        }

        private void onConstructionComplete(bool freshlyBuild)
        {
            level++;
            building = false;
            upgrading = false;
            constructionNeeds.setZero();
            daysInConstruction = 0;
            if (freshlyBuild)
                open(currentInvestor, false);
            currentInvestor = null;
        }

        /// <summary>
        /// Now includes workforce/efficiency. Also buying for upgrading\building are happening here
        /// </summary>
        public override void consumeNeeds()
        {
            // consume resource needs
            if (IsOpen && !Type.isResourceGathering())
            {
                List<Storage> shoppingList = getHowMuchInputProductsReservesWants();
                if (shoppingList.Count > 0)
                    if (Country.economy.getValue() == Economy.PlannedEconomy)
                    {
                        var realNeed = Country.countryStorageSet.hasAllOfConvertToBiggest(shoppingList);
                        if (realNeed != null)
                        {
                            //Country.countryStorageSet.send(this.getInputProductsReserve(), shoppingList);
                            consumeFromCountryStorage(realNeed, Country);
                            getInputProductsReserve().Add(realNeed);
                        }
                    }
                    else
                    {
                        //if (isSubsidized())
                        //    Game.market.SellList(this, new StorageSet(shoppingList), Country);
                        //else
                        //    Game.market.SellList(this, new StorageSet(shoppingList), null);
                        Country subsidizer = isSubsidized() ? Country : null;
                        foreach (Storage item in shoppingList)
                            if (item.isNotZero())
                                Game.market.Sell(this, item, subsidizer);
                    }
            }
            if (isUpgrading() || isBuilding())
            {
                daysInConstruction++;
                bool isBuyingComplete = false;

                //if (buildByPlannedEconomy)
                if (Country.economy.getValue() == Economy.PlannedEconomy)
                {
                    if (daysInConstruction >= Options.fabricConstructionTimeWithoutCapitalism)
                        if (Country.countryStorageSet.has(constructionNeeds))
                        {
                            isBuyingComplete = true; //Country.countryStorageSet.send(this.getInputProductsReserve(), constructionNeeds);
                            //buildByPlannedEconomy = false;
                            //Country.countryStorageSet.send(this, )
                        }
                }
                else
                {
                    if (isBuilding())
                        isBuyingComplete = Game.market.Sell(this, constructionNeeds, Options.BuyInTimeFactoryUpgradeNeeds, Type.GetBuildNeeds());
                    else if (isUpgrading())
                        isBuyingComplete = Game.market.Sell(this, constructionNeeds, Options.BuyInTimeFactoryUpgradeNeeds, getUpgradeNeeds());

                    // get money from current investor, not owner
                    MoneyView needExtraFonds = wantsMinMoneyReserv().Copy().Subtract(Cash, false);
                    if (needExtraFonds.isNotZero())
                    {
                        var investor = currentInvestor as Agent;
                        if (investor.CanPay(needExtraFonds))
                        {
                            investor.PayWithoutRecord(this, needExtraFonds);
                            ownership.Add(currentInvestor, needExtraFonds);
                        }
                        else
                        {
                            investor.Bank.GiveLackingMoneyInCredit(investor, needExtraFonds);
                            if (investor.CanPay(needExtraFonds))
                            {
                                investor.PayWithoutRecord(this, needExtraFonds);
                                ownership.Add(currentInvestor, needExtraFonds);
                            }
                        }
                    }
                }
                if (isBuyingComplete
                   || (Country.economy.getValue() == Economy.NaturalEconomy && daysInConstruction == Options.fabricConstructionTimeWithoutCapitalism))

                {
                    //todo avoid extra subtraction and redo whole method
                    if (isBuilding())
                    {
                        onConstructionComplete(true);
                        getInputProductsReserve().subtract(Type.GetBuildNeeds(), false);
                    }
                    else // assuming isUpgrading()
                    {
                        onConstructionComplete(false);
                        getInputProductsReserve().subtract(getUpgradeNeeds(), false);
                    }
                }
                else
                {
                    if (daysInConstruction == Options.maxDaysBuildingBeforeRemoving)
                        if (isBuilding())
                        {
                            currentInvestor = null;
                            markToDestroy();
                        }
                        else // upgrading
                            stopUpgrading();
                }
            }
        }

        /// <summary>
        ///new value
        /// </summary>
        internal override MoneyView getExpences()
        {
            return (base.getExpences().Copy()).Add(getSalaryCost());
        }

        //Not necessary ti optimize -  cost 0.1% of tick
        public int getWorkForce()
        {
            int result = 0;
            foreach (var pop in hiredWorkForce)
                result += pop.Value;
            return result;
        }

        public bool HasAnyWorforce()
        {
            return hiredWorkForce.Count > 0;
        }

        public void OnClicked()
        {
            MainCamera.factoryPanel.show(this);
        }

        public bool CanProduce(Product product)
        {
            return Type.CanProduce(product);
        }
    }
}