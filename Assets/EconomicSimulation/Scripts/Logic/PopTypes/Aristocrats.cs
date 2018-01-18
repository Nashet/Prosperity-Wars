using UnityEngine;
using UnityEditor;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Aristocrats : GrainGetter
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
            if (getProvince().getResource() != null && !getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew))
            {
                FactoryType ftype = FactoryType.whoCanProduce(getProvince().getResource());
                if (ftype != null)
                {
                    Factory resourceFactoryInHere = getProvince().getExistingResourceFactory();
                    if (resourceFactoryInHere == null) //build new factory
                    {
                        //Has money / resources?
                        StorageSet resourceToBuild = ftype.getBuildNeeds();
                        // todo remove connection to grain
                        Storage needFood = resourceToBuild.getFirstStorage(Product.Grain);
                        // try to build for food
                        if (storage.isBiggerOrEqual(needFood))
                        {
                            var newFactory = new Factory(getProvince(), this, ftype);
                            storage.send(newFactory.getInputProductsReserve(), needFood);
                            newFactory.constructionNeeds.setZero();
                        }
                        else
                        {
                            Value cost = Game.market.getCost(resourceToBuild);
                            if (canPay(cost))  //try to build for money  
                            {
                                var newFactory = new Factory(getProvince(), this, ftype);  // build new one              
                                pay(newFactory, cost);
                            }// take loans? Aristocrats wouldn't take loans for upgrades
                        }
                    }
                    else
                    {
                        //upgrade existing resource factory
                        // upgrade is only for money? Yes, because its complicated - lots of various products               
                        if (resourceFactoryInHere.getWorkForceFulFilling().isBiggerThan(Options.minWorkforceFullfillingToUpgradeFactory)
                            && resourceFactoryInHere.getMargin().get() >= Options.minMarginToUpgrade
                            && Factory.conditionsUpgrade.isAllTrue(resourceFactoryInHere, this))
                            resourceFactoryInHere.upgrade(this);
                    }
                }
            }
            base.invest();
        }
    }
}