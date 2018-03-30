﻿using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Capitalists : Investor
    {
        public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Capitalists, where, culture, oldLife)
        { }

        public Capitalists(int iamount, Culture iculture, Province where) : base(iamount, PopType.Capitalists, iculture, where)
        { }

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
            if (Country.economy.getValue() == Economy.PlannedEconomy)
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
                && (isStateCulture() || Country.minorityPolicy.getValue() == MinorityPolicy.Equality))
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
            if (Economy.isMarket.checkIfTrue(Country) && Country.Invented(Invention.Manufactures))

            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)

                //var projects = Province.getAllInvestmentProjects().Where(x => x.GetMargin(Province).isBiggerThan(Options.minMarginToInvest));
                var projects = World.GetAllAllowedInvestments(this).Where(
                delegate (KeyValuePair<IInvestable, Procent> x)
                {
                    var isFactory = x.Key as Factory;
                    if (isFactory != null)
                        return Country.InventedFactory(isFactory.Type);
                    else
                    {
                        var newFactory = x.Key as NewFactoryProject;
                        if (newFactory != null)
                            return Country.InventedFactory(newFactory.Type);
                    }
                    return true;
                }
                );
                var project = projects.MaxByRandom(x => x.Value.Multiply(getBusinessSecurity(x.Key)).get());

                if (!project.Equals(default(KeyValuePair<IInvestable, Procent>)) && project.Value.Multiply(getBusinessSecurity(project.Key)).isBiggerThan(Options.minMarginToInvest))
                {
                    MoneyView investmentCost = project.Key.GetInvestmentCost();
                    if (!CanPay(investmentCost))
                        Bank.GiveLackingMoneyInCredit(this, investmentCost);
                    if (CanPay(investmentCost))
                    {
                        project.Value.Set(Procent.Zero);
                        Factory factory = project.Key as Factory;
                        if (factory != null)
                        {
                            if (factory.IsOpen)// upgrade existing factory
                                factory.upgrade(this);
                            else
                                factory.open(this, true);
                        }
                        else
                        {
                            Owners buyShare = project.Key as Owners;
                            if (buyShare != null) // buy part of existing factory
                                buyShare.BuyStandardShare(this);
                            else
                            {
                                var factoryProject = project.Key as NewFactoryProject;
                                if (factoryProject != null)
                                {
                                    Factory factory2 = factoryProject.Province.BuildFactory(this, factoryProject.Type, investmentCost);
                                    PayWithoutRecord(factory2, investmentCost);
                                }
                                else
                                    Debug.Log("Unknown investment type");
                            }
                        }
                    }
                }
            }
            base.invest();
        }
    }
}