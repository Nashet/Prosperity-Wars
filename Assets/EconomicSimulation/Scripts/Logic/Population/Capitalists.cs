using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Linq;
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

        public override bool canTrade()
        {
            if (Country.economy == Economy.PlannedEconomy)
                return false;
            else
                return true;
        }

        public override bool shouldPayAristocratTax()
        {
            return false;
        }

        public override bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
                || reform == Government.BourgeoisDictatorship)
                && (isStateCulture() || Country.minorityPolicy == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }

        public override int getVotingPower(Government.GovernmentReformValue reformValue)
        {
            if (CanVoteWithThatGovernment(reformValue))
                if (reformValue == Government.WealthDemocracy)
                    return Options.PopRichStrataVotePower;
                else
                    return 1;
            else
                return 0;
        }

        public override void invest()
        {
            //should I invest?
            if (Economy.isMarket.checkIfTrue(Country) && Country.Science.IsInvented(Invention.Manufactures))
            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)

                //var projects = Province.getAllInvestmentProjects().Where(x => x.GetMargin(Province).isBiggerThan(Options.minMarginToInvest));
                var projects = World.GetAllAllowedInvestments(this).Where(
                delegate (KeyValuePair<IInvestable, Procent> x)
                {
                    var isFactory = x.Key as Factory;
                    if (isFactory != null)
                        return Country.Science.IsInventedFactory(isFactory.Type);
                    else
                    {
                        var newFactory = x.Key as NewFactoryProject;
                        if (newFactory != null)
                            return Country.Science.IsInventedFactory(newFactory.Type);
                        else
                        {
                            var isBuyingShare = x.Key as Owners;
                            if (isBuyingShare != null)
                                if (isBuyingShare.HowMuchSelling(this).isNotZero())
                                    return false;
                        }
                    }
                    return true;
                }
                );
                if (Game.logInvestments)
                {
                    var c = projects.ToList();
                    c = c.OrderByDescending(x => x.Value.get()).ToList();
                    var d = c.MaxBy(x => x.Value.get());
                    var e = c.MaxByRandom(x => x.Value.get());
                    var f = c.MaxByRandom(x => x.Value.Copy().Multiply(getBusinessSecurity(x.Key)).get());
                    c.Any();
                }
                var project = projects.MaxByRandom(x => x.Value.Copy().Multiply(getBusinessSecurity(x.Key)).get());

                if (!project.Equals(default(KeyValuePair<IInvestable, Procent>)) && project.Value.Copy().Multiply(getBusinessSecurity(project.Key)).isBiggerThan(Options.minMarginToInvest))
                {
                    MoneyView investmentCost = project.Key.GetInvestmentCost(project.Key.Country.market);
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
                                    PayWithoutRecord(factory2, investmentCost, Register.Account.Construction);
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