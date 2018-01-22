using UnityEngine;
using UnityEditor;
using Nashet.ValueSpace;
using System.Linq;
using Nashet.Utils;
using System.Collections.Generic;
using System;

namespace Nashet.EconomicSimulation
{
    public interface IInvestable
    {
        Procent getMargin();
        Value getInvestmentsCost();
        bool canProduce(Product product);
        Procent GetWorkForceFulFilling();
    }
    public class Capitalists : GrainGetter
    {
        public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Capitalists, where, culture)
        { }
        public Capitalists(int iamount, Culture iculture, Province where) : base(iamount, PopType.Capitalists, iculture, where)
        { }
        public override bool canThisDemoteInto(PopType targetType)
        {
            if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
                || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
                || targetType == PopType.Artisans
                )
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            return false;
        }
        public override void produce()
        {
            // Caps don't produce products directly
        }
        internal override bool canTrade()
        {
            if (getCountry().economy.getValue() == Economy.PlannedEconomy)
                return false;
            else
                return true;
        }
        public override bool shouldPayAristocratTax()
        {
            return false;
        }
        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
                || reform == Government.BourgeoisDictatorship)
                && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }
        internal override int getVotingPower(Government.ReformValue reformValue)
        {
            if (canVote(reformValue))
                if (reformValue == Government.WealthDemocracy)
                    return Options.PopRichStrataVotePower;
                else
                    return 1;
            else
                return 0;
        }

        internal override void invest()
        {
            //should I invest?                
            if (Economy.isMarket.checkIftrue(getCountry()) && getCountry().isInvented(Invention.Manufactures))
            //if (!getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew)
            //&& (getProvince().howMuchFactories() == 0 || getProvince().getAverageFactoryWorkforceFulfilling() > Options.minFactoryWorkforceFulfillingToInvest)
            //)
            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)
                var projects = getProvince().getAllInvestmentsProjects(
                    x => x.getMargin().get() >= Options.minMarginToInvest
                    && x.GetWorkForceFulFilling().isBiggerThan(Options.minFactoryWorkforceFulfillingToInvest));
                var project = projects.MaxBy(x => x.getMargin().get());

                if (project != null)
                {
                    Value investmentCost = project.getInvestmentsCost();
                    if (!canPay(investmentCost))
                        getBank().giveLackingMoney(this, investmentCost);
                    if (canPay(investmentCost))
                    {
                        Factory factory;
                        var factoryToBuild = project as FactoryType;
                        if (factoryToBuild != null)
                        {
                            factory = new Factory(getProvince(), this, factoryToBuild);
                            payWithoutRecord(factory, investmentCost);
                        }
                        else
                        {
                            factory = project as Factory;
                            if (factory != null)
                                factory.upgrade(this);
                            else
                                Debug.Log("Unknown investment type");
                        }                        
                    }
                }
            }
            base.invest();
        }
    }
}