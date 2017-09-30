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
        if (resourceInput == null)
            return true;
        else
            return false;

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
            {
                float possibleProfit = factoryType.getPossibleProfit(province).get();
                if (possibleProfit > result.Value)
                    result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
            }
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
    //todo improve getPossibleProfit
    internal Value getPossibleProfit(Province province)
    {
        foreach (Storage inputProduct in resourceInput)
            //if (Game.market.getDemandSupplyBalance(st.getProduct()) > 20f || Game.market.getDemandSupplyBalance(st.getProduct()) == 0f)
            if (!Game.market.isAvailable(inputProduct.getProduct()) || Game.market.getDemandSupplyBalance(basicProduction.getProduct()) == Options.MarketZeroDSB)
                return new Value(0);
        Value income = Game.market.getCost(basicProduction);
        Value outCome = Game.market.getCost(resourceInput);
        return income.subtractOutside(outCome, false);
    }
    internal Procent getPossibleMargin(Province province)
    {
        return Procent.makeProcent(getPossibleProfit(province), getBuildCost());
    }

}