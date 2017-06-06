using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class FactoryType
{
    internal readonly string name;
    static internal readonly List<FactoryType> allTypes = new List<FactoryType>();

    ///<summary> per 1000 workers </summary>
    public Storage basicProduction;

    /// <summary>resource input list 
    /// per 1000 workers & per 1 unit outcome</summary>
    internal PrimitiveStorageSet resourceInput;

    /// <summary>Per 1 level upgrade</summary>
    public readonly PrimitiveStorageSet upgradeResourceLowTier;
    public readonly PrimitiveStorageSet upgradeResourceMediumTier;
    public readonly PrimitiveStorageSet upgradeResourceHighTier;
    internal static FactoryType GoldMine, Furniture, MetalDigging, MetalSmelter;

    //internal ConditionsList conditionsBuild;
    internal Condition enoughMoneyOrResourcesToBuild;
    internal ConditionsList conditionsBuild;
    bool shaft;
    internal FactoryType(string iname, Storage ibasicProduction, PrimitiveStorageSet iresourceInput, bool shaft)
    {

        name = iname;
        if (iname == "Gold pit") GoldMine = this;
        if (iname == "Furniture factory") Furniture = this;
        if (iname == "Metal pit") MetalDigging = this;
        if (iname == "Metal smelter") MetalSmelter = this;
        allTypes.Add(this);
        basicProduction = ibasicProduction;
        if (iresourceInput == null) resourceInput = new PrimitiveStorageSet();
        else
            resourceInput = iresourceInput;
        //upgradeResource.Set(new Storage(Product.Wood, 10f));
        upgradeResourceLowTier = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Stone, 2f), new Storage(Product.Wood, 10f) });
        upgradeResourceMediumTier= new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Stone, 10f), new Storage(Product.Lumber, 3f), new Storage(Product.Cement, 2f), new Storage(Product.Metal, 1f) });
        upgradeResourceHighTier = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Cement, 10f),  new Storage(Product.Metal, 4f), new Storage(Product.Machinery, 2f) });
        enoughMoneyOrResourcesToBuild = new Condition(
          (delegate (System.Object forWhom)
          {
              Country who = forWhom as Country;
              if (Economy.isMarket.checkIftrue(who))
                  return who.canPay(getBuildCost());
              else              
                  return who.storageSet.has(getBuildNeeds());              

          }), "Have enough money or resources to build", true
          );
        //Condition factoryPlacedInOurCountry = new Condition((Owner forWhom) => province.getOwner() == forWhom, "Enterprise placed in our country", false);
        //, factoryPlacedInOurCountry
        conditionsBuild = new ConditionsList(new List<AbstractCondition>() {
        Economy.isNotLF, enoughMoneyOrResourcesToBuild}); // can build
        this.shaft = shaft;
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
        PrimitiveStorageSet result = new PrimitiveStorageSet();
        result.set(new Storage(Product.Food, 40f));
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
        if (resourceInput.Count() == 0)
            return true;
        else return false;
    }
    internal bool isShaft()
    {
        return shaft;
    }

    //todo improve getPossibleProfit
    internal Value getPossibleProfit(Province province)
    {
        foreach (Storage st in resourceInput)
            //if (Game.market.getDemandSupplyBalance(st.getProduct()) > 20f || Game.market.getDemandSupplyBalance(st.getProduct()) == 0f)
            if (Game.market.getDemandSupplyBalance(st.getProduct()) == 0f)
                return new Value(0);
        Value income = Game.market.getCost(basicProduction);
        Value outCome = Game.market.getCost(resourceInput);
        return income.subtractOutside(outCome);
    }
    internal Procent getPossibleMargin(Province province)
    {
        Value cost = Game.market.getCost(getBuildNeeds());
        cost.add(Options.factoryMoneyReservPerLevel);
        //if (cost.get() > 0)
        //return new Procent(getPossibleProfit(province) / cost.get());
        return Procent.makeProcent(getPossibleProfit(province), cost);
    }
    internal static FactoryType getMostTeoreticalProfitable(Province province)
    {
        List<FactoryType> possiblefactories = province.WhatFactoriesCouldBeBuild();
        float possibleProfit = 0;
        float maxProfitFound = 0;
        foreach (FactoryType ft in possiblefactories)
        {
            //todo refactor efficiency
            // if (province.CanBuildNewFactory(ft) || province.CanUpgradeFactory(ft))
            {
                possibleProfit = ft.getPossibleProfit(province).get();
                if (possibleProfit > maxProfitFound)
                    maxProfitFound = possibleProfit;
            }
        }
        if (maxProfitFound > 0f)
            foreach (FactoryType ft in possiblefactories)
                if (ft.getPossibleProfit(province).get() == maxProfitFound && (province.CanBuildNewFactory(ft) || province.CanUpgradeFactory(ft)))
                    return ft;
        return null;
    }

    internal static Factory getMostPracticlyProfitable(Province province)
    {
        List<FactoryType> possiblefactories = province.WhatFactoriesCouldBeBuild();
        float profit = 0;
        float maxProfitFound = 0;
        foreach (Factory ft in province.allFactories)
        {
            if (province.CanUpgradeFactory(ft.type))
            {
                profit = ft.getProfit();
                if (profit > maxProfitFound)
                    maxProfitFound = profit;
            }
        }
        if (maxProfitFound > 0f)
            foreach (Factory factory in province.allFactories)
                if (factory.getProfit() == maxProfitFound && province.CanUpgradeFactory(factory.type))
                    return factory;
        return null;
    }
}