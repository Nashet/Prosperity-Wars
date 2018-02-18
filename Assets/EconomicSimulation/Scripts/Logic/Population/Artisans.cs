using UnityEngine;

using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Linq;

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
            if (//|| targetType == PopType.Farmers && !Country.isInvented(Invention.Farming)
                targetType == PopType.Soldiers && Country.Invented(Invention.ProfessionalArmy)
                || targetType == PopType.Workers
                )
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Capitalists && Country.Invented(Invention.Manufactures))
                return true;
            else
                return false;
        }
        public override void produce()
        {
            // artisan shouldn't work with PE
            if (Country.economy.getValue() == Economy.PlannedEconomy)
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
                        if (Economy.isMarket.checkIfTrue(Country))
                        {
                            sell(getGainGoodsThisTurn());
                        }
                        else if (Country.economy.getValue() == Economy.NaturalEconomy)
                        {
                            // send to market?
                            sell(getGainGoodsThisTurn());
                        }
                        else if (Country.economy.getValue() == Economy.PlannedEconomy)
                        {
                            storage.sendAll(Country.countryStorageSet);
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
                PayWithoutRecord(artisansProduction, Cash);

                // take loan if don't have enough money to buy inputs            
                if (Country.Invented(Invention.Banking) && !artisansProduction.isAllInputProductsCollected())
                    if (artisansProduction.Type.getPossibleProfit().isNotZero())
                    {
                        var needs = artisansProduction.getRealAllNeeds();
                        if (!artisansProduction.CanAfford(needs))
                        {
                            var loanSize = Game.market.getCost(needs); // takes little more than really need, could be fixed                            
                            Bank.GiveCredit(this, loanSize);
                            PayWithoutRecord(artisansProduction, Cash);
                        }
                    }

                artisansProduction.consumeNeeds();
                artisansProduction.PayWithoutRecord(this, artisansProduction.Cash);

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
            if (Country.economy.getValue() == Economy.PlannedEconomy)
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
                && (isStateCulture() || Country.minorityPolicy.getValue() == MinorityPolicy.Equality))
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
            //foreach (FactoryType factoryType in FactoryType.getAllNonResourceTypes(Country))
            foreach (FactoryType factoryType in FactoryType.getAllInventedTypes(Country).
                Where(x => !x.isResourceGathering() && x.basicProduction.Product != Product.Education))
            {
                float possibleProfit = factoryType.getPossibleProfit().get();
                if (possibleProfit > result.Value)
                    result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
            }
            if (result.Key != null && (artisansProduction == null || artisansProduction != null && result.Key != artisansProduction.Type))
            {
                artisansProduction = new ArtisanProduction(result.Key, Province, this);
                base.changeProductionType(artisansProduction.Type.basicProduction.Product);
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
        public FactoryType Type
        {
            get
            {
                if (artisansProduction == null)
                    return null;
                else
                    return artisansProduction.Type;
            }
        }

        internal void checkProfit()
        {
            // todo doesn't include taxes. Should it?
            if (artisansProduction == null || moneyIncomeThisTurn.Copy().subtract( artisansProduction.getExpences()).isZero())
                changeProductionType();
        }
    }
}