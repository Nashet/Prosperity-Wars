using System.Linq;
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

        internal void dealWithMarket()
        {
            if (storage.get() > Options.aristocratsFoodReserv)
            {
                Storage howMuchSend = new Storage(storage.Product, storage.get() - Options.aristocratsFoodReserv);
                storage.send(getSentToMarket(), howMuchSend);
                //sentToMarket.set(howMuchSend);
                Game.market.sentToMarket.Add(howMuchSend);
            }
        }

        public override void produce()
        {
            //Aristocrats don't produce anything
        }

        internal override bool canTrade()
        {
            if (Country.economy.getValue() == Economy.PlannedEconomy)
                return false;
            else
                return true;
        }

        internal override bool canSellProducts()
        {
            return true;
        }

        public override bool shouldPayAristocratTax()
        {
            return false;
        }

        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
                || reform == Government.Aristocracy || reform == Government.Tribal)
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
            // Aristocrats invests only in resource factories (and banks)
            if (Province.getResource() != null)
            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)
                var projects = Province.getAllInvestmentProjects().Where(x => x.CanProduce(Province.getResource()));

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
                            var factory = Province.BuildFactory(this, factoryProject.Type, Game.market.getCost(resourceToBuild));
                            storage.send(factory.getInputProductsReserve(), resourceToBuild);
                            factory.constructionNeeds.setZero();
                        }
                        else // build for money
                        {
                            MoneyView investmentCost = Game.market.getCost(resourceToBuild);
                            if (!CanPay(investmentCost))
                                Bank.GiveLackingMoneyInCredit(this, investmentCost);
                            if (CanPay(investmentCost))
                            {
                                var factory = Province.BuildFactory(this, factoryProject.Type, investmentCost);  // build new one
                                PayWithoutRecord(factory, investmentCost);
                            }
                        }
                    }
                    else
                    {
                        var factory = project as Factory;// existing one
                        if (factory != null)
                        {
                            MoneyView investmentCost = factory.GetInvestmentCost();
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