using Nashet.Conditions;
using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class ProductionType : IClickable, ISortableName
    {
        private static readonly List<ProductionType> allTypes = new List<ProductionType>();
        public static ProductionType GoldMine, Furniture, MetalDigging, MetalSmelter, Barnyard, University, Orchard, WeaverFactory;

        public readonly string name;

        ///<summary> per 1000 workers </summary>
        public Storage basicProduction;

        /// <summary>resource input list
        /// per 1000 workers & per 1 unit outcome</summary>
        public StorageSet resourceInput;

        private readonly List<Storage> buildingNeeds = new List<Storage> { new Storage(Product.Grain, 40f) };

        /// <summary>Per 1 level upgrade</summary>
        private readonly List<Storage> upgradeResourceLowTier = new List<Storage> { new Storage(Product.Stone, 2f), new Storage(Product.Wood, 10f) };

        private readonly List<Storage> upgradeResourceMediumTier = new List<Storage> { new Storage(Product.Stone, 10f), new Storage(Product.Lumber, 3f), new Storage(Product.Metal, 1f) };//, new Storage(Product.Cement, 2f)
        private readonly List<Storage> upgradeResourceHighTier = new List<Storage> { new Storage(Product.Metal, 4f), new Storage(Product.Machinery, 2f) }; //new Storage(Product.Cement, 10f),

        public Condition enoughMoneyOrResourcesToBuild;

        ///duplicated in Factory
        public static DoubleCondition allowsForeignInvestments = new DoubleCondition((agent, province) =>
        (province as Province).Country == (agent as Agent).Country
        || ((province as Province).Country.economy.AllowForeignInvestments
        && (agent as Agent).Country.economy != Economy.PlannedEconomy),
            agent => "Local government allows foreign investments or it isn't foreign investment", true);

        // empty trade
        public DoubleConditionsList conditionsBuildThis;

        private readonly bool shaft;
        private readonly float nameWeight;

        protected Invention[] requiredInventions;

        public bool IsRural
        {
            get { return (!shaft && !IsResourceProcessing() || this == Barnyard) && this != University; }
        }

        static ProductionType()
        {
            new ProductionType("Forestry", new Storage(Product.Wood, 2f), false);
            GoldMine = new ProductionType("Gold pit", new Storage(Product.Gold, 2f * Options.goldToCoinsConvert), true);
            MetalDigging = new ProductionType("Metal pit", new Storage(Product.MetalOre, 2f), true);
            new ProductionType("Coal pit", new Storage(Product.Coal, 6f), true);
            new ProductionType("Cotton farm", new Storage(Product.Cotton, 2f), false);
            new ProductionType("Quarry", new Storage(Product.Stone, 2f), true);
            Orchard = new ProductionType("Orchard", new Storage(Product.Fruit, 2f), false);
            new ProductionType("Fishery", new Storage(Product.Fish, 2f), false);
            new ProductionType("Tobacco farm", new Storage(Product.Tobacco, 2f), false);

            new ProductionType("Oil rig", new Storage(Product.Oil, 2f), true);
            new ProductionType("Rubber plantation", new Storage(Product.Rubber, 1f), false);

            StorageSet resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Grain, 1f));
            Barnyard = new ProductionType("Barnyard", new Storage(Product.Cattle, 2f), resourceInput, Invention.Domestication);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            Furniture = new ProductionType("Furniture factory", new Storage(Product.Furniture, 4f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Wood, 1f));
            new ProductionType("Sawmill", new Storage(Product.Lumber, 4f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Fuel, 0.5f));
            resourceInput.Set(new Storage(Product.MetalOre, 2f));
            MetalSmelter = new ProductionType("Metal smelter", new Storage(Product.Metal, 8f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Fibers, 1f));
            WeaverFactory = new ProductionType("Weaver factory", new Storage(Product.Clothes, 4f), resourceInput, Invention.JohnKayFlyingshuttle);

            //resourceInput = new StorageSet();
            //resourceInput.Set(new Storage(Product.Fuel, 0.5f));
            //resourceInput.Set(new Storage(Product.Stone, 2f));
            //new ProductionType("Cement factory", new Storage(Product.Cement, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Sugar, 1f));
            new ProductionType("Distillery", new Storage(Product.Liquor, 4f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Smithery", new Storage(Product.ColdArms, 4f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Stone, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Ammunition factory", new Storage(Product.Ammunition, 8f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Firearms factory", new Storage(Product.Firearms, 8f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Artillery factory", new Storage(Product.Artillery, 8f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Oil, 1f));
            new ProductionType("Oil refinery", new Storage(Product.MotorFuel, 4f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Machinery factory", new Storage(Product.Machinery, 4f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Machinery, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Rubber, 1f));
            new ProductionType("Car factory", new Storage(Product.Cars, 12f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Machinery, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Artillery, 1f));
            new ProductionType("Tank factory", new Storage(Product.Tanks, 12f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Machinery, 1f));
            new ProductionType("Airplane factory", new Storage(Product.Airplanes, 12f), resourceInput, Invention.Manufactures);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Oil, 1f));
            resourceInput.Set(new Storage(Product.Rubber, 1f));
            new ProductionType("Electronics factory", new Storage(Product.Electronics, 12f), resourceInput, Invention.Manufactures);

            University = new ProductionType("University", new Storage(Product.Education, 4f), new StorageSet());
        }

        /// <summary>
        /// Basic constructor for resource getting FactoryType
        /// </summary>
        public ProductionType(string name, Storage basicProduction, bool shaft, params Invention[] requiredInventions)
        {
            this.requiredInventions = requiredInventions;
            this.name = name;
            this.shaft = shaft;
            nameWeight = name.GetWeight();

            allTypes.Add(this);
            this.basicProduction = basicProduction;

            enoughMoneyOrResourcesToBuild = new Condition(
                delegate (object forWhom)
                {
                    var agent = forWhom as Agent;
                    if (agent.Country.economy == Economy.PlannedEconomy)
                    {
                        return agent.Country.countryStorageSet.has(GetBuildNeeds());
                    }
                    else
                    {
                        var cost = GetBuildCost(agent.Country.market);
                        return agent.CanPay(cost);
                    }
                },
                delegate
                {
                    var sb = new StringBuilder();
                    // var cost = GetBuildCost(agent.Country.market);
                    //sb.Append("Has ").Append(cost).Append(" coins");
                    sb.Append("Has enough coins");
                    sb.Append(" or (with ").Append(Economy.PlannedEconomy).Append(") has ").Append(GetBuildNeeds().ToString(", "));
                    return sb.ToString();
                }, true);

            // Using: Country () , province, this <FactoryType>
            // used in BuildPanel only, only for Game.Player
            // Should be: de-facto Country, Investor, this <FactoryType> (change Economy.isNot..)
            // Ideally: Agent, FactoryProject
            // or put it in FactoryProject
            conditionsBuildThis = new DoubleConditionsList(new List<Condition> {
                Economy.isNotLF, Economy.isNotInterventionism, enoughMoneyOrResourcesToBuild,
                allowsForeignInvestments}); //            
        }

        /// <summary>
        /// Constructor for resource processing FactoryType
        /// </summary>
        public ProductionType(string name, Storage basicProduction, StorageSet resourceInput, params Invention[] requiredInventions) : this(name, basicProduction, false, requiredInventions)
        {
            this.resourceInput = resourceInput;
        }

        public static IEnumerable<ProductionType> getAllInventedFactories(Country country)
        {
            foreach (var next in allTypes)
                if (country.Science.IsInventedFactory(next))
                    yield return next;
        }

        public static IEnumerable<ProductionType> getAllInventedByAnyoneFactories()
        {
            foreach (var next in allTypes)
                if (next.basicProduction.Product.IsInventedByAnyOne())
                    yield return next;
        }

        public static IEnumerable<ProductionType> getAllInventedArtisanships(Country country)
        {
            foreach (var next in allTypes)
                if (country.Science.IsInventedArtisanship(next))
                    yield return next;
        }

        //public static IEnumerable<FactoryType> getAllResourceTypes(Country country)
        //{
        //    foreach (var next in getAllInventedTypes(country))
        //        if (next.isResourceGathering())
        //            yield return next;
        //}
        //public static IEnumerable<FactoryType> getAllNonResourceTypes(Country country)
        //{
        //    foreach (var next in getAllInventedTypes(country))
        //        if (!next.isResourceGathering())
        //            yield return next;
        //}

        public IEnumerable<Invention> AllRequiredInventions
        {
            get
            {
                foreach (var item in requiredInventions)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        ///Returns new value
        /// </summary>
        public MoneyView GetBuildCost(Market market)
        {
            Money result = market.getCost(GetBuildNeeds()).Copy();
            result.Add(Options.factoryMoneyReservePerLevel);
            return result;
        }

        /// <summary>
        /// Gives a copy of needs
        /// </summary>
        public List<Storage> GetBuildNeeds()
        //public StorageSet GetBuildNeeds()
        {
            return buildingNeeds.Copy();//.Copy();
            //TODO!has connection in pop.invest!!
            ////if (whoCanProduce(Product.Gold) == this)
            ////        result.Set(new Storage(Product.Wood, 40f));
            //return result;
        }

        /// <summary>
        /// Returns first correct value
        /// Assuming there is only one  FactoryType for each Product
        /// </summary>
        public static ProductionType whoCanProduce(Product product)
        {
            if (product != null)
                foreach (ProductionType ft in allTypes)
                    if (ft.basicProduction.isSameProductType(product))
                        return ft;
            return null;
        }

        /// <summary>
        /// Returns copy
        /// </summary>
        public List<Storage> GetUpgradeNeeds(int tier)
        {
            switch (tier)
            {
                case 1: return upgradeResourceLowTier.Copy();
                case 2: return upgradeResourceMediumTier.Copy();
                case 3: return upgradeResourceHighTier.Copy();
                default:
                    Debug.Log("Unknown tier");
                    return null;
            }
        }

        public override string ToString()
        {
            return name;
        }

        public bool isResourceGathering()
        {
            if (hasInput() || this == University)
                return false;
            else
                return true;
            //resourceInput.Count() == 0
        }

        public bool IsResourceProcessing()
        {
            return !isResourceGathering() && this != Barnyard && this != University;
        }

        public bool isShaft()
        {
            return shaft;
        }

        //public static FactoryType getMostTheoreticalProfitable(Province province)
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

        //public static Factory getMostPracticallyProfitable(Province province)
        //{
        //    KeyValuePair<Factory, float> result = new KeyValuePair<Factory, float>(null, 0f);
        //    foreach (Factory factory in province.allFactories)
        //    {
        //        if (province.canUpgradeFactory(factory.Type))
        //        {
        //            float profit = factory.getProfit();
        //            if (profit > result.Value)
        //                result = new KeyValuePair<Factory, float>(factory, profit);
        //        }
        //    }
        //    return result.Key;
        //}

        public bool hasInput()
        {
            return resourceInput != null && resourceInput.Count() != 0;
        }

        /// <summary>
        /// For 1 level / 1000 workers. Not includes tax. Includes modifiers. New value
        /// </summary>
        public MoneyView getPossibleProfit(Province province)
        {
            if (province.Country.market.getDemandSupplyBalance(basicProduction.Product, false) == Options.MarketZeroDSB)
                return new MoneyView(0); // no demand for result product
            Money income = province.Country.market.getCost(basicProduction).Copy();
            income.Multiply((decimal)Factory.modifierEfficiency.getModifier(new Factory(province, null, this, null)), false);
            var outCome = new Money(0m);// = province.getLocalMinSalary();//salary
            if (hasInput())
            {
                foreach (Storage inputProduct in resourceInput)
                    if (!province.Country.market.isAvailable(inputProduct.Product))
                        return new MoneyView(0);// inputs are unavailable
                outCome.Add(province.Country.market.getCost(resourceInput));
            }
            return income.Subtract(outCome, false);
        }

        /// <summary>
        /// For artisans. Not including salary
        /// </summary>
        public MoneyView getPossibleProfit(Market market)
        {
            if (market
                .
                getDemandSupplyBalance(
                basicProduction
                .
                Product, false) == Options.MarketZeroDSB)
                return new MoneyView(0); // no demand for result product
            MoneyView income = market.getCost(basicProduction);

            if (hasInput())
            {
                // change to minimal hire limits
                foreach (Storage inputProduct in resourceInput)
                    if (!market.isAvailable(inputProduct.Product))
                        return new MoneyView(0);// inputs are unavailable

                return income.Copy().Subtract(market.getCost(resourceInput), false);
            }
            return income;
        }

        /// <summary>
        /// That is possible margin in that case. Includes tax. New value
        /// </summary>
        public Procent GetPossibleMargin(Province province)
        {
            var profit = getPossibleProfit(province).Copy();
            var taxes = profit.Copy().Multiply(province.Country.taxationForRich.tax.Procent);
            profit.Subtract(taxes);
            return new Procent(profit, GetBuildCost(province.Country.market));
        }

        /// <summary>
        ///
        /// </summary>
        public bool canBuildNewFactory(Province where, Agent builder)
        {
            if (where.hasFactory(this))
                return false;
            if (isResourceGathering() && basicProduction.Product != where.getResource()
                || (builder != null && !builder.Country.Science.IsInventedFactory(this)) // check it out side
                || (builder != null && !allowsForeignInvestments.checkIftrue(builder, where))// check it out side
                                                                                             //|| !basicProduction.Product.IsInventedByAnyOne()
                )
                return false;
            return true;
        }

        public void OnClicked()
        {
            MainCamera.buildPanel.selectFactoryType(this);
            MainCamera.buildPanel.Refresh();
        }

        public bool CanProduce(Product product)
        {
            return basicProduction.Product == product;
        }

        public float NameWeight
        {
            get
            {
                return nameWeight;
            }
        }

        //public Procent GetWorkForceFulFilling()
        //{
        //    return Procent.HundredProcent;
        //}
    }
}