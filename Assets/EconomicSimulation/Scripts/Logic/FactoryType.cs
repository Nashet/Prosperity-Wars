using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class FactoryType : IClickable, IInvestable
    {
        static private readonly List<FactoryType> allTypes = new List<FactoryType>();
        internal static FactoryType GoldMine, Furniture, MetalDigging, MetalSmelter, Barnyard;

        internal readonly string name;

        ///<summary> per 1000 workers </summary>
        public Storage basicProduction;

        /// <summary>resource input list 
        /// per 1000 workers & per 1 unit outcome</summary>
        internal StorageSet resourceInput;

        /// <summary>Per 1 level upgrade</summary>
        public readonly StorageSet upgradeResourceLowTier;
        public readonly StorageSet upgradeResourceMediumTier;
        public readonly StorageSet upgradeResourceHighTier;

        //internal ConditionsList conditionsBuild;
        internal Condition enoughMoneyOrResourcesToBuild;
        internal ConditionsListForDoubleObjects conditionsBuild;
        private readonly bool shaft;

        static FactoryType()
        {
            new FactoryType("Forestry", new Storage(Product.Wood, 2f), false);
            new FactoryType("Gold pit", new Storage(Product.Gold, 2f), true);
            new FactoryType("Metal pit", new Storage(Product.MetalOre, 2f), true);
            new FactoryType("Coal pit", new Storage(Product.Coal, 3f), true);
            new FactoryType("Cotton farm", new Storage(Product.Cotton, 2f), false);
            new FactoryType("Quarry", new Storage(Product.Stone, 2f), true);
            new FactoryType("Orchard", new Storage(Product.Fruit, 2f), false);
            new FactoryType("Fishery", new Storage(Product.Fish, 2f), false);
            new FactoryType("Tobacco farm", new Storage(Product.Tobacco, 2f), false);

            new FactoryType("Oil rig", new Storage(Product.Oil, 2f), true);
            new FactoryType("Rubber plantation", new Storage(Product.Rubber, 1f), false);

            StorageSet resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Grain, 1f));
            new FactoryType("Barnyard", new Storage(Product.Cattle, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Lumber, 1f));
            new FactoryType("Furniture factory", new Storage(Product.Furniture, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Wood, 1f));
            new FactoryType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Fuel, 0.5f));
            resourceInput.set(new Storage(Product.MetalOre, 2f));
            new FactoryType("Metal smelter", new Storage(Product.Metal, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Fibers, 1f));
            new FactoryType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Fuel, 0.5f));
            resourceInput.set(new Storage(Product.Stone, 2f));
            new FactoryType("Cement factory", new Storage(Product.Cement, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Sugar, 1f));
            new FactoryType("Distillery", new Storage(Product.Liquor, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Metal, 1f));
            new FactoryType("Smithery", new Storage(Product.ColdArms, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Stone, 1f));
            resourceInput.set(new Storage(Product.Metal, 1f));
            new FactoryType("Ammunition factory", new Storage(Product.Ammunition, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Lumber, 1f));
            resourceInput.set(new Storage(Product.Metal, 1f));
            new FactoryType("Firearms factory", new Storage(Product.Firearms, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Lumber, 1f));
            resourceInput.set(new Storage(Product.Metal, 1f));
            new FactoryType("Artillery factory", new Storage(Product.Artillery, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Oil, 1f));
            new FactoryType("Oil refinery", new Storage(Product.MotorFuel, 2f), resourceInput);


            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Metal, 1f));
            new FactoryType("Machinery factory", new Storage(Product.Machinery, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Machinery, 1f));
            resourceInput.set(new Storage(Product.Metal, 1f));
            resourceInput.set(new Storage(Product.Rubber, 1f));
            new FactoryType("Car factory", new Storage(Product.Cars, 6f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Machinery, 1f));
            resourceInput.set(new Storage(Product.Metal, 1f));
            resourceInput.set(new Storage(Product.Artillery, 1f));
            new FactoryType("Tank factory", new Storage(Product.Tanks, 6f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Lumber, 1f));
            resourceInput.set(new Storage(Product.Metal, 1f));
            resourceInput.set(new Storage(Product.Machinery, 1f));
            new FactoryType("Airplane factory", new Storage(Product.Airplanes, 6f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.set(new Storage(Product.Metal, 1f));
            resourceInput.set(new Storage(Product.Oil, 1f));
            resourceInput.set(new Storage(Product.Rubber, 1f));
            new FactoryType("Electronics factory", new Storage(Product.Electronics, 6f), resourceInput);
        }
        /// <summary>
        /// Basic constructor for resource getting FactoryType
        /// </summary>    
        internal FactoryType(string name, Storage basicProduction, bool shaft)
        {
            this.name = name;
            if (name == "Gold pit") GoldMine = this;
            if (name == "Furniture factory") Furniture = this;
            if (name == "Metal pit") MetalDigging = this;
            if (name == "Metal smelter") MetalSmelter = this;
            if (name == "Barnyard") Barnyard = this;
            allTypes.Add(this);
            this.basicProduction = basicProduction;

            //upgradeResource.Set(new Storage(Product.Wood, 10f));
            upgradeResourceLowTier = new StorageSet(new List<Storage> { new Storage(Product.Stone, 2f), new Storage(Product.Wood, 10f) });
            upgradeResourceMediumTier = new StorageSet(new List<Storage> { new Storage(Product.Stone, 10f), new Storage(Product.Lumber, 3f), new Storage(Product.Cement, 2f), new Storage(Product.Metal, 1f) });
            upgradeResourceHighTier = new StorageSet(new List<Storage> { new Storage(Product.Cement, 10f), new Storage(Product.Metal, 4f), new Storage(Product.Machinery, 2f) });

            enoughMoneyOrResourcesToBuild = new Condition(
                delegate (object forWhom)
                {
                    var agent = forWhom as Agent;
                    if (agent.getCountry().economy.getValue() == Economy.PlannedEconomy)
                    {
                        return agent.getCountry().countryStorageSet.has(this.getBuildNeeds());
                    }
                    else
                    {
                        //Value cost = Game.market.getCost(this.getBuildNeeds());
                        //cost.add(Options.factoryMoneyReservPerLevel);
                        Value cost = getCost();
                        return agent.canPay(cost);
                    }
                },
                delegate
                {
                    var sb = new StringBuilder();
                    //Value cost = Game.market.getCost(this.getBuildNeeds());
                    //cost.add(Options.factoryMoneyReservPerLevel);
                    Value cost = getCost();
                    sb.Append("Have ").Append(cost).Append(" coins");
                    sb.Append(" or (with ").Append(Economy.PlannedEconomy).Append(") have ").Append(this.getBuildNeeds());
                    return sb.ToString();
                }, true);

            conditionsBuild = new ConditionsListForDoubleObjects(new List<Condition>() {
                Economy.isNotLF, enoughMoneyOrResourcesToBuild, Province.doesCountryOwn }); // can build
            this.shaft = shaft;
        }
        /// <summary>
        /// Constructor for resource processing FactoryType
        /// </summary>    
        internal FactoryType(string name, Storage basicProduction, StorageSet resourceInput) : this(name, basicProduction, false)
        {
            //if (resourceInput == null)
            //    this.resourceInput = new PrimitiveStorageSet();
            //else
            this.resourceInput = resourceInput;
        }
        public static IEnumerable<FactoryType> getAllInventedTypes(Country country)
        {
            foreach (var next in allTypes)
                if (next.basicProduction.getProduct().isInventedBy(country))
                    yield return next;
        }
        public static IEnumerable<FactoryType> getAllInventedTypes(Country country, Predicate<FactoryType> predicate)
        {
            foreach (var next in allTypes)
                if (next.basicProduction.getProduct().isInventedBy(country) && predicate(next))
                    yield return next;
        }
        public static IEnumerable<FactoryType> getAllResourceTypes(Country country)
        {
            foreach (var next in getAllInventedTypes(country))
                if (next.isResourceGathering())
                    yield return next;
        }
        public static IEnumerable<FactoryType> getAllNonResourceTypes(Country country)
        {
            foreach (var next in getAllInventedTypes(country))
                if (!next.isResourceGathering())
                    yield return next;
        }

        public Value getCost()
        {
            Value result = Game.market.getCost(getBuildNeeds());
            result.add(Options.factoryMoneyReservePerLevel);
            return result;
        }
        internal StorageSet getBuildNeeds()
        {
            //return new Storage(Product.Food, 40f);
            // thats weird place
            StorageSet result = new StorageSet();
            result.set(new Storage(Product.Grain, 40f));
            //TODO!has connection in pop.invest!!
            //if (whoCanProduce(Product.Gold) == this)
            //        result.Set(new Storage(Product.Wood, 40f));
            return result;
        }
        /// <summary>
        /// Returns first correct value
        /// Assuming there is only one  FactoryType for each Product
        /// </summary>   
        internal static FactoryType whoCanProduce(Product product)
        {
            foreach (FactoryType ft in allTypes)
                if (ft.basicProduction.isSameProductType(product))
                    return ft;
            return null;
        }
        override public string ToString() { return name; }
        internal bool isResourceGathering()
        {
            if (hasInput())
                return false;
            else
                return true;
            //resourceInput.Count() == 0
        }
        internal bool isManufacture()
        {
            return !isResourceGathering() && this != Barnyard;
        }
        internal bool isShaft()
        {
            return shaft;
        }

        //internal static FactoryType getMostTheoreticalProfitable(Province province)
        //{
        //    KeyValuePair<FactoryType, float> result = new KeyValuePair<FactoryType, float>(null, 0f);
        //    foreach (FactoryType factoryType in province.getAllBuildableFactories())
        //    {
        //        float possibleProfit = factoryType.getPossibleProfit().get();
        //        if (possibleProfit > result.Value)
        //            result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
        //    }
        //    return result.Key;
        //}

        //internal static Factory getMostPracticallyProfitable(Province province)
        //{
        //    KeyValuePair<Factory, float> result = new KeyValuePair<Factory, float>(null, 0f);
        //    foreach (Factory factory in province.allFactories)
        //    {
        //        if (province.canUpgradeFactory(factory.getType()))
        //        {
        //            float profit = factory.getProfit();
        //            if (profit > result.Value)
        //                result = new KeyValuePair<Factory, float>(factory, profit);
        //        }
        //    }
        //    return result.Key;
        //}

        internal bool hasInput()
        {
            return resourceInput != null;
        }

        
        internal Value getPossibleProfit()
        {
            Value income = Game.market.getCost(basicProduction);
            if (hasInput())
            {
                foreach (Storage inputProduct in resourceInput)
                    if (!Game.market.isAvailable(inputProduct.getProduct()))
                        return new Value(0);// inputs are unavailable
                                            //if (Game.market.getBouthOnMarket(basicProduction.getProduct(), false) == 0f)
                if (Game.market.getDemandSupplyBalance(basicProduction.getProduct()) == Options.MarketZeroDSB)
                    return new Value(0); // no demand for result product
                Value outCome = Game.market.getCost(resourceInput);
                return income.subtractOutside(outCome, false);
            }
            else
                return income;
        }
        /// <summary>
        /// That is possible margin in that case
        /// </summary>        
        public Procent getMargin()
        {
            return Procent.makeProcent(getPossibleProfit(), getCost(), false);
        }

        //internal bool canBuildNewFactory(FactoryType type)
        //{
        //    if (HaveFactory(type))
        //        return false;
        //    if (type.isResourceGathering() && type.basicProduction.getProduct() != this.resource
        //        || !type.basicProduction.getProduct().isInventedBy(getCountry())
        //        || type.isManufacture() && !getCountry().isInvented(Invention.Manufactories)
        //        || (type.basicProduction.getProduct() == Product.Cattle && !getCountry().isInvented(Invention.Domestication))
        //        )
        //        return false;
        //    return true;
        //}
        internal bool canBuildNewFactory(Province where)
        {
            if (where.hasFactory(this))
                return false;
            if (isResourceGathering() && basicProduction.getProduct() != where.getResource()
                || !basicProduction.getProduct().isInventedBy(where.getCountry())
                || isManufacture() && !where.getCountry().isInvented(Invention.Manufactures)
                || (basicProduction.getProduct() == Product.Cattle && !where.getCountry().isInvented(Invention.Domestication))
                )
                return false;
            return true;
        }

        public void OnClicked()
        {
            MainCamera.buildPanel.selectFactoryType(this);
            MainCamera.buildPanel.Refresh();
        }
        public bool canProduce(Product product)
        {
            return basicProduction.getProduct() == product;
        }

        
        //public Procent GetWorkForceFulFilling()
        //{
        //    return Procent.HundredProcent;
        //}
    }
}