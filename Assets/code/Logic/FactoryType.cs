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
    internal PrimitiveStorageSet upgradeResource = new PrimitiveStorageSet();
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
        upgradeResource.Set(new Storage(Product.Stone, 10f));
        //internal ConditionsList conditionsBuild;
        enoughMoneyOrResourcesToBuild = new Condition(
          (delegate (Country forWhom)
          {
              if (Economy.isMarket.checkIftrue(forWhom))
                  return forWhom.wallet.canPay(getBuildCost());
              else
              {

                  return forWhom.storageSet.has(getBuildNeeds());
              }

          }), "Have enough money or resources to build", true
          );
        conditionsBuild = new ConditionsList(new List<AbstractCondition>() {
        Economy.isNotLF, enoughMoneyOrResourcesToBuild}); // can build
        this.shaft = shaft;
    }
    internal PrimitiveStorageSet getUpgradeNeeds()
    {
        return upgradeResource;
    }
    internal Value getBuildCost()
    {
        Value result = Game.market.getCost(getBuildNeeds());
        result.add(Game.factoryMoneyReservPerLevel);
        return result;
    }
    internal PrimitiveStorageSet getBuildNeeds()
    {
        //return new Storage(Product.Food, 40f);
        PrimitiveStorageSet result = new PrimitiveStorageSet();
        result.Set(new Storage(Product.Food, 40f));
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

    //todo improove getPossibleProfit
    internal float getPossibleProfit(Province province)
    {
        foreach (Storage st in resourceInput)
            if (Game.market.getDemandSupplyBalance(st.getProduct()) > 20f || Game.market.getDemandSupplyBalance(st.getProduct()) == 0f)
                return 0;
        float income = Game.market.getCost(basicProduction);
        Value outCome = Game.market.getCost(resourceInput);
        return income - outCome.get();
    }
    internal Procent getPossibleMargin(Province province)
    {
        Value cost = Game.market.getCost(getBuildNeeds());
        cost.add(Game.factoryMoneyReservPerLevel);
        //if (cost.get() > 0)
        return new Procent(getPossibleProfit(province) / cost.get());
    }
    internal static FactoryType getMostTeoreticalProfitable(Province province)
    {
        List<FactoryType> possiblefactories = province.WhatFactoriesCouldBeBuild();
        float possibleProfit = 0;
        float maxProfitFound = 0;
        foreach (FactoryType ft in possiblefactories)
        {

            // if (province.CanBuildNewFactory(ft) || province.CanUpgradeFactory(ft))
            {
                possibleProfit = ft.getPossibleProfit(province);
                if (possibleProfit > maxProfitFound)
                    maxProfitFound = possibleProfit;
            }
        }
        if (maxProfitFound > 0f)
            foreach (FactoryType ft in possiblefactories)
                if (ft.getPossibleProfit(province) == maxProfitFound && (province.CanBuildNewFactory(ft) || province.CanUpgradeFactory(ft)))
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