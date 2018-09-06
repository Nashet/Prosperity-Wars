using System.Collections.Generic;
using System.Linq;
using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Aristocrats : Investor
    {
        public Aristocrats(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Aristocrats, where, culture, oldLife)
        { }

        public Aristocrats(int iamount, Culture iculture, Province where) : base(iamount, PopType.Aristocrats, iculture, where)
        { }

        public override bool canThisPromoteInto(PopType targetType)
        {
            return false;
        }

        public void SentExtraGoodsToMarket()
        {
            if (storage.get() > Options.aristocratsFoodReserv)
            {
                Storage howMuchSend = new Storage(storage.Product, storage.get() - Options.aristocratsFoodReserv);

                SendToMarket(howMuchSend);                
            }
        }

        public override void produce()
        {
            //Aristocrats don't produce anything
        }

        public override bool canTrade()
        {
            if (Country.economy == Economy.PlannedEconomy)
                return false;
            else
                return true;
        }

        public override bool canSellProducts()
        {
            return true;
        }

        public override bool shouldPayAristocratTax()
        {
            return false;
        }

        public override bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
                || reform == Government.Aristocracy || reform == Government.Tribal)
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
            // Aristocrats invests only in resource factories (and banks)
            if (Province.getResource() != null)
            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)
                var projects = Province.AllInvestmentProjects().Where(
                   //x => x.CanProduce(Province.getResource())
                   delegate (IInvestable x)
                   {                       
                       if (!x.CanProduce(Province.getResource()))
                           return false;
                       var isFactory = x as Factory;
                       if (isFactory != null)
                           return Country.Science.IsInventedFactory(isFactory.Type);
                       else
                       {
                           var newFactory = x as NewFactoryProject;
                           if (newFactory != null)
                               return Country.Science.IsInventedFactory(newFactory.Type);
                           else
                           {
                               var isBuyingShare = x as Owners;
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
                    c = c.OrderByDescending(x => x.GetMargin().get()).ToList();
                    var d = c.MaxBy(x => x.GetMargin().get());
                    var e = c.MaxByRandom(x => x.GetMargin().get());
                }
                var project = projects.MaxByRandom(x => x.GetMargin().Multiply(getBusinessSecurity(x)).get());
                if (project != null && project.GetMargin().Multiply(getBusinessSecurity(project)).isBiggerThan(Options.minMarginToInvest))
                {
                    var factoryProject = project as NewFactoryProject; // build new one
                    if (factoryProject != null)
                    {
                        // todo remove connection to grain
                        Storage resourceToBuild = factoryProject.Type.GetBuildNeeds().GetFirstSubstituteStorage(Product.Grain);

                        // try to build for grain
                        if (storage.has(resourceToBuild))
                        {
                            var factory = Province.BuildFactory(this, factoryProject.Type, Country.market.getCost(resourceToBuild));
                            storage.send(factory.getInputProductsReserve(), resourceToBuild);
                            factory.constructionNeeds.setZero();
                        }
                        else // build for money
                        {
                            MoneyView investmentCost = Country.market.getCost(resourceToBuild);
                            if (!CanPay(investmentCost))
                                Bank.GiveLackingMoneyInCredit(this, investmentCost);
                            if (CanPay(investmentCost))
                            {
                                var factory = Province.BuildFactory(this, factoryProject.Type, investmentCost);  // build new one
                                PayWithoutRecord(factory, investmentCost, Register.Account.Construction);
                            }
                        }
                    }
                    else
                    {
                        var factory = project as Factory;// existing one
                        if (factory != null)
                        {
                            MoneyView investmentCost = factory.GetInvestmentCost(Country.market);
                            if (!CanPay(investmentCost))
                                Bank.GiveLackingMoneyInCredit(this, investmentCost);
                            if (CanPay(investmentCost))
                            {
                                if (factory.IsOpen)
                                    factory.upgrade(this);
                                else
                                    factory.open(this, true);
                            }
                        }
                        else
                        {
                            Owners buyShare = project as Owners;
                            if (buyShare != null) // buy part of existing factory
                            {
                                MoneyView investmentCost = buyShare.GetInvestmentCost();
                                if (!CanPay(investmentCost))
                                    Bank.GiveLackingMoneyInCredit(this, investmentCost);
                                if (CanPay(investmentCost))
                                    buyShare.BuyStandardShare(this);
                            }
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