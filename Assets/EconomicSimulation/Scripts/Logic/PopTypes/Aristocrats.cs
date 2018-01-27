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
            if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
                || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
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
                Game.market.sentToMarket.add(howMuchSend);
            }
        }
        public override void produce()
        {
            //Aristocrats don't produce anything
        }
        internal override bool canTrade()
        {
            if (getCountry().economy.getValue() == Economy.PlannedEconomy)
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
            // Aristocrats invests only in resource factories (and banks)
            if (getProvince().getResource() != null)
            //universalInvest(x=>canProduce(getProvince().getResource()));
            //if (!getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew)
            //    && (getProvince().howMuchFactories() == 0 || getProvince().getAverageFactoryWorkforceFulfilling() > Options.minFactoryWorkforceFulfillingToInvest)
            //    )
            {
                // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)
                var projects = getProvince().getAllInvestmentsProjects().Where(x => x.getMargin().get() >= Options.minMarginToInvest
                && x.canProduce(getProvince().getResource())                
                );

                var project = projects.MaxBy(x => x.getMargin().get());
                if (project != null)
                {
                    {
                        var factoryToBuild = project as FactoryType; // build new one
                        if (factoryToBuild != null)
                        {
                            // todo remove connection to grain
                            Storage resourceToBuild = factoryToBuild.getBuildNeeds().getFirstStorage(Product.Grain);

                            // try to build for grain
                            if (storage.has(resourceToBuild))
                            {
                                var factory = new Factory(getProvince(), this, factoryToBuild, Game.market.getCost(resourceToBuild));
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
                                    var factory = new Factory(getProvince(), this, factoryToBuild, investmentCost);  // build new one              
                                    payWithoutRecord(factory, investmentCost);
                                }
                            }
                        }
                        else
                        {
                            var factory = project as Factory;// existing one                               
                            if (factory != null)
                            {
                                Value investmentCost = factory.getCost();
                                if (!canPay(investmentCost))
                                    getBank().giveLackingMoney(this, investmentCost);
                                if (canPay(investmentCost))
                                    factory.upgrade(this);
                                //payWithoutRecord(factory, investmentCost);
                            }
                            else
                            {
                                Owners buyShare = project as Owners;
                                if (buyShare != null) // buy part of existing factory
                                {
                                    Value investmentCost = buyShare.getCost();
                                    if (!canPay(investmentCost))
                                        getBank().GetLackingMoney(investmentCost);
                                    if (canPay(investmentCost))
                                        buyShare.BuyStandardShare(this);
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
        //override internal bool CanGainDividents()
        //{
        //    return true;
        //}
    }
}