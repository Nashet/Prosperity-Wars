using UnityEngine;

using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public class Artisans : GrainGetter
    {
        private ArtisanProduction artisansProduction;
        public Artisans(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Artisans, where, culture)
        {
            changeProductionType();
        }
        public Artisans(int amount, Culture culture, Province where) : base(amount, PopType.Artisans, culture, where)
        {
            changeProductionType();
        }
        override protected void deleteData()
        {
            base.deleteData();
            artisansProduction = null;
        }
        public override bool canThisDemoteInto(PopType targetType)
        {
            if (//|| targetType == PopType.Farmers && !getCountry().isInvented(Invention.Farming)
                targetType == PopType.Soldiers && GetCountry().isInvented(Invention.ProfessionalArmy)
                || targetType == PopType.Workers
                )
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Capitalists && GetCountry().isInvented(Invention.Manufactures))
                return true;
            else
                return false;
        }
        public override void produce()
        {
            // artisan shouldn't work with PE
            if (GetCountry().economy.getValue() == Economy.PlannedEconomy)
                artisansProduction = null;
            else
            {
                if (Game.Random.Next(Options.ArtisansChangeProductionRate) == 1
                   )// && (artisansProduction==null 
                    //|| (artisansProduction !=null && needsFulfilled.isSmallerThan(Options.ArtisansChangeProductionLevel))))
                    changeProductionType();

                if (artisansProduction != null)
                {
                    if (artisansProduction.isAllInputProductsCollected())
                    {
                        artisansProduction.produce();
                        if (Economy.isMarket.checkIftrue(GetCountry()))
                        {
                            sell(getGainGoodsThisTurn());
                            //sentToMarket.set(gainGoodsThisTurn);
                            //storage.setZero();
                            //Game.market.sentToMarket.add(gainGoodsThisTurn);
                        }
                        else if (GetCountry().economy.getValue() == Economy.NaturalEconomy)
                        {
                            // send to market?
                            sell(getGainGoodsThisTurn());
                            //sentToMarket.set(gainGoodsThisTurn);
                            //storage.setZero();
                            //Game.market.sentToMarket.add(gainGoodsThisTurn);
                        }
                        else if (GetCountry().economy.getValue() == Economy.PlannedEconomy)
                        {
                            storage.sendAll(GetCountry().countryStorageSet);
                        }
                    }
                    //else
                    //   changeProductionType();
                }
            }
        }
        public override void consumeNeeds()
        {
            base.consumeNeeds();
            if (artisansProduction != null)
            {
                payWithoutRecord(artisansProduction, cash);

                // take loan if don't have enough money to buy inputs            
                if (GetCountry().isInvented(Invention.Banking) && !artisansProduction.isAllInputProductsCollected())
                    if (artisansProduction.getType().getPossibleProfit().isNotZero())
                    {
                        var needs = artisansProduction.getRealAllNeeds();
                        if (!artisansProduction.canAfford(needs))
                        {
                            var loanSize = Game.market.getCost(needs); // takes little more than really need, could be fixed
                            if (getBank().canGiveMoney(this, loanSize))
                                getBank().giveMoney(this, loanSize);
                            payWithoutRecord(artisansProduction, cash);
                        }
                    }

                artisansProduction.consumeNeeds();
                artisansProduction.payWithoutRecord(this, artisansProduction.cash);

                // consuming made in artisansProduction.consumeNeeds()
                // here is data transfering
                // todo rework data transfering from artisans?
                this.getConsumedInMarket().Add(artisansProduction.getConsumedInMarket());
                this.getConsumed().Add(artisansProduction.getConsumed());
                this.getConsumedLastTurn().Add(artisansProduction.getConsumedLastTurn());
            }
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
            return true;
        }

        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
                || reform == Government.BourgeoisDictatorship)
                && (isStateCulture() || GetCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }
        internal override int getVotingPower(Government.ReformValue reformValue)
        {
            if (canVote(reformValue))
                if (reformValue == Government.WealthDemocracy)
                    return Options.PopMiddleStrataVotePower;
                else
                    return 1;
            else
                return 0;
        }
        private void changeProductionType()
        {
            KeyValuePair<FactoryType, float> result = new KeyValuePair<FactoryType, float>(null, 0f);
            foreach (FactoryType factoryType in FactoryType.getAllNonResourceTypes(GetCountry()))
            {
                float possibleProfit = factoryType.getPossibleProfit().get();
                if (possibleProfit > result.Value)
                    result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
            }
            if (result.Key != null && (artisansProduction == null || artisansProduction != null && result.Key != artisansProduction.getType()))
            {
                artisansProduction = new ArtisanProduction(result.Key, getProvince(), this);
                base.changeProductionType(artisansProduction.getType().basicProduction.getProduct());
            }
        }
        public StorageSet getInputProducts()
        {
            if (artisansProduction == null)
                return null;
            else
                return artisansProduction.getInputProductsReserve();
        }
        override public void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            if (artisansProduction != null)
                artisansProduction.SetStatisticToZero();
        }
        public FactoryType getType()
        {
            if (artisansProduction == null)
                return null;
            else
                return artisansProduction.getType();
        }

        internal void checkProfit()
        {
            // todo doesn't include taxes. Should it?
            if (artisansProduction == null || moneyIncomethisTurn.get() - artisansProduction.getExpences() <= 0f)
                changeProductionType();
        }
    }
}