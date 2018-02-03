using UnityEngine;

using Nashet.ValueSpace;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class Aristocrats : Investor
    {
        public Aristocrats(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Aristocrats, where, culture)
        { }
        public Aristocrats(int iamount, Culture iculture, Province where) : base(iamount, PopType.Aristocrats, iculture, where)
        { }
        public override bool canThisDemoteInto(PopType targetType)
        {
            if (targetType == PopType.Farmers && GetCountry().isInvented(Invention.Farming)
                || targetType == PopType.Soldiers && GetCountry().isInvented(Invention.ProfessionalArmy)
                || targetType == PopType.Tribesmen)
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            return false;
        }
        internal void dealWithMarket()
        {
            if (storage.get() > Options.aristocratsFoodReserv)
            {
                Storage howMuchSend = new Storage(storage.getProduct(), storage.get() - Options.aristocratsFoodReserv);
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
            if (GetCountry().economy.getValue() == Economy.PlannedEconomy)
                return false;
            else
                return true;
        }
        override internal bool canSellProducts()
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
                && (isStateCulture() || GetCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
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
            if (getProvince().getResource() != null)
            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)
                var projects = getProvince().getAllInvestmentProjects().Where(x => x.GetMargin().isBiggerThan(Options.minMarginToInvest)
                && x.CanProduce(getProvince().getResource())
                );

                var project = projects.MaxByRandom(x => x.GetMargin().get());
                if (project != null)
                {
                    var factoryProject = project as FactoryProject; // build new one
                    if (factoryProject != null)
                    {
                        // todo remove connection to grain
                        Storage resourceToBuild = factoryProject.Type.GetBuildNeeds().GetFirstSubstituteStorage(Product.Grain);

                        // try to build for grain
                        if (storage.has(resourceToBuild))
                        {
                            var factory = getProvince().BuildFactory(this, factoryProject.Type, Game.market.getCost(resourceToBuild));
                            storage.send(factory.getInputProductsReserve(), resourceToBuild);
                            factory.constructionNeeds.setZero();
                        }
                        else // build for money
                        {
                            Value investmentCost = Game.market.getCost(resourceToBuild);
                            if (!canPay(investmentCost))
                                getBank().giveLackingMoney(this, investmentCost);
                            if (canPay(investmentCost))
                            {
                                var factory = getProvince().BuildFactory(this, factoryProject.Type, investmentCost);  // build new one              
                                payWithoutRecord(factory, investmentCost);
                            }
                        }
                    }
                    else
                    {
                        var factory = project as Factory;// existing one                               
                        if (factory != null)
                        {
                            Value investmentCost = factory.GetInvestmentCost();
                            if (!canPay(investmentCost))
                                getBank().giveLackingMoney(this, investmentCost);
                            if (canPay(investmentCost))
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
                                Value investmentCost = buyShare.GetInvestmentCost();
                                if (!canPay(investmentCost))
                                    getBank().giveLackingMoney(this, investmentCost);
                                if (canPay(investmentCost))
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