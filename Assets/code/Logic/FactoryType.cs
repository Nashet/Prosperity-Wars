using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class FactoryType
{
    static internal readonly List<FactoryType> allTypes = new List<FactoryType>();
    internal static FactoryType GoldMine, Furniture, MetalDigging, MetalSmelter;

    internal readonly string name;

    ///<summary> per 1000 workers </summary>
    public Storage basicProduction;

    /// <summary>resource input list 
    /// per 1000 workers & per 1 unit outcome</summary>
    internal PrimitiveStorageSet resourceInput;

    /// <summary>Per 1 level upgrade</summary>
    public readonly PrimitiveStorageSet upgradeResourceLowTier;
    public readonly PrimitiveStorageSet upgradeResourceMediumTier;
    public readonly PrimitiveStorageSet upgradeResourceHighTier;


    //internal ConditionsList conditionsBuild;
    internal Condition enoughMoneyOrResourcesToBuild;
    internal ConditionsList conditionsBuild;
    private readonly bool shaft;

    static FactoryType()
    {
        new FactoryType("Forestry", new Storage(Product.Wood, 2f), false);
        new FactoryType("Gold pit", new Storage(Product.Gold, 2f), true);
        new FactoryType("Metal pit", new Storage(Product.MetallOre, 2f), true);
        new FactoryType("Sheepfold", new Storage(Product.Wool, 2f), false);
        new FactoryType("Quarry", new Storage(Product.Stone, 2f), true);
        new FactoryType("Orchard", new Storage(Product.Fruit, 2f), false);

        new FactoryType("Oil rig", new Storage(Product.Oil, 2f), true);
        new FactoryType("Rubber plantation", new Storage(Product.Rubber, 1f), false);

        new FactoryType("Fishery", new Storage(Product.Fish, 2f), false);
        //new FactoryType("Orchard", new Storage(Product.Fruit, 2f), false);
        new FactoryType("Barnyard", new Storage(Product.Cattle, 2f), false);

        PrimitiveStorageSet resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        new FactoryType("Furniture factory", new Storage(Product.Furniture, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 1f));
        new FactoryType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 0.5f));
        resourceInput.set(new Storage(Product.MetallOre, 2f));
        new FactoryType("Metal smelter", new Storage(Product.Metal, 4f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wool, 1f));
        new FactoryType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 0.5f));
        resourceInput.set(new Storage(Product.Stone, 2f));
        new FactoryType("Cement factory", new Storage(Product.Cement, 4f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Sugar, 1f));
        new FactoryType("Winery", new Storage(Product.Wine, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Smithery", new Storage(Product.ColdArms, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Stone, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Ammunition factory", new Storage(Product.Ammunition, 4f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Firearms factory", new Storage(Product.Firearms, 4f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Artillery factory", new Storage(Product.Artillery, 4f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Oil, 1f));
        new FactoryType("Oil refinery", new Storage(Product.Fuel, 2f), resourceInput);


        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Machinery factory", new Storage(Product.Machinery, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Machinery, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Rubber, 1f));
        new FactoryType("Car factory", new Storage(Product.Cars, 6f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Machinery, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Artillery, 1f));
        new FactoryType("Tank factory", new Storage(Product.Tanks, 6f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Machinery, 1f));
        new FactoryType("Airplane factory", new Storage(Product.Airplanes, 6f), resourceInput);
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
        allTypes.Add(this);
        this.basicProduction = basicProduction;

        //upgradeResource.Set(new Storage(Product.Wood, 10f));
        upgradeResourceLowTier = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Stone, 2f), new Storage(Product.Wood, 10f) });
        upgradeResourceMediumTier = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Stone, 10f), new Storage(Product.Lumber, 3f), new Storage(Product.Cement, 2f), new Storage(Product.Metal, 1f) });
        upgradeResourceHighTier = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Cement, 10f), new Storage(Product.Metal, 4f), new Storage(Product.Machinery, 2f) });

        enoughMoneyOrResourcesToBuild = new Condition(
            delegate (object forWhom)
            {
                Value cost = this.getBuildCost();
                return (forWhom as Agent).canPay(cost);
            },
            delegate
            {
                Game.threadDangerSB.Clear();
                Game.threadDangerSB.Append("Have ").Append(getBuildCost()).Append(" coins");
                return Game.threadDangerSB.ToString();
            }, true);

        conditionsBuild = new ConditionsList(new List<Condition>() {
        Economy.isNotLF, enoughMoneyOrResourcesToBuild}); // can build
        this.shaft = shaft;
    }
    /// <summary>
    /// Constructor for resource processing FactoryType
    /// </summary>    
    internal FactoryType(string name, Storage basicProduction, PrimitiveStorageSet resourceInput) : this(name, basicProduction, false)
    {
        //if (resourceInput == null)
        //    this.resourceInput = new PrimitiveStorageSet();
        //else
        this.resourceInput = resourceInput;
    }
    public static IEnumerable<FactoryType> getInventedTypes(Country country)
    {
        foreach (var next in allTypes)
            if (next.basicProduction.getProduct().isInvented(country))
                yield return next;
    }
    public static IEnumerable<FactoryType> getResourceTypes(Country country)
    {
        foreach (var next in getInventedTypes(country))
            if (next.isResourceGathering())
                yield return next;
    }
    public static IEnumerable<FactoryType> getNonResourceTypes(Country country)
    {
        foreach (var next in getInventedTypes(country))
            if (!next.isResourceGathering())
                yield return next;
    }

    internal Value getBuildCost()
    {
        Value result = Game.market.getCost(getBuildNeeds());
        result.add(Options.factoryMoneyReservPerLevel);
        return result;
    }
    internal PrimitiveStorageSet getBuildNeeds()
    {
        //return new Storage(Product.Food, 40f);
        // thats weird place
        PrimitiveStorageSet result = new PrimitiveStorageSet();
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
    internal static FactoryType whoCanProduce(Product pro)
    {
        foreach (FactoryType ft in allTypes)
            if (ft.basicProduction.getProduct() == pro)
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
    internal bool isShaft()
    {
        return shaft;
    }
    internal static FactoryType getMostTeoreticalProfitable(Province province)
    {
        KeyValuePair<FactoryType, float> result = new KeyValuePair<FactoryType, float>(null, 0f);
        foreach (FactoryType factoryType in province.whatFactoriesCouldBeBuild())
        {
            float possibleProfit = factoryType.getPossibleProfit(province).get();
            if (possibleProfit > result.Value)
                result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
        }
        return result.Key;
    }

    internal static Factory getMostPracticlyProfitable(Province province)
    {
        KeyValuePair<Factory, float> result = new KeyValuePair<Factory, float>(null, 0f);
        foreach (Factory factory in province.allFactories)
        {
            if (province.canUpgradeFactory(factory.getType()))
            {
                float profit = factory.getProfit();
                if (profit > result.Value)
                    result = new KeyValuePair<Factory, float>(factory, profit);
            }
        }
        return result.Key;
    }

    internal bool hasInput()
    {
        return resourceInput != null;
    }

    //todo improve getPossibleProfit
    internal Value getPossibleProfit(Province province)
    {
        Value income = Game.market.getCost(basicProduction);
        if (hasInput())
        {
            foreach (Storage inputProduct in resourceInput)                
                if (!Game.market.isAvailable(inputProduct.getProduct()) || Game.market.getDemandSupplyBalance(basicProduction.getProduct()) == Options.MarketZeroDSB)
                    return new Value(0);
            Value outCome = Game.market.getCost(resourceInput);
            return income.subtractOutside(outCome, false);
        }
        else
            return income;
    }
    internal Procent getPossibleMargin(Province province)
    {
        return Procent.makeProcent(getPossibleProfit(province), getBuildCost());
    }

}