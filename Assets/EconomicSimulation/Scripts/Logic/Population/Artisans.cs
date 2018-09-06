using System;
using System.Linq;
using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Artisans : GrainGetter
    {
        private ArtisanProduction artisansProduction;

        public Artisans(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Artisans, where, culture, oldLife)
        {
            changeProductionType();
        }

        public Artisans(int amount, Culture culture, Province where) : base(amount, PopType.Artisans, culture, where)
        {
            //changeProductionType();
        }

        public override void Kill()
        {
            base.Kill();
            artisansProduction = null;
        }

        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Capitalists && Country.Science.IsInvented(Invention.Manufactures))
                return true;
            else
                return false;
        }

        public override void produce()
        {
            // artisan shouldn't work with PE
            if (Country.economy == Economy.PlannedEconomy)
                artisansProduction = null;
            else
            {
                Rand.Call(() => checkProfit(), 10);// changes production type if needed
                if (Rand.Chance(Options.ArtisansChangeProductionRate)) // check if it's best production type so far
                    changeProductionType();
                if (artisansProduction != null)
                {
                    //if (artisansProduction.isAllInputProductsCollected())
                    {
                        artisansProduction.produce();
                        if (Economy.isMarket.checkIfTrue(Country))
                        {
                            if (getGainGoodsThisTurn().isNotZero())
                                SendToMarket(getGainGoodsThisTurn());
                        }
                        else if (Country.economy == Economy.NaturalEconomy)
                        {
                            // send to market?
                            if (getGainGoodsThisTurn().isNotZero())
                                SendToMarket(getGainGoodsThisTurn());
                        }
                        else if (Country.economy == Economy.PlannedEconomy)
                        {
                            storage.sendAll(Country.countryStorageSet);
                        }
                    }
                    //else
                    //   changeProductionType();
                }
            }
        }

        public StorageSet GetResurceInput()
        {
            if (artisansProduction == null)
                return new StorageSet();
            else
                return artisansProduction.Type.resourceInput;
        }

        public override void consumeNeeds()
        {
            base.consumeNeeds();
            if (artisansProduction != null)
            {
                PayWithoutRecord(artisansProduction, Cash, Register.Account.MarketOperations);

                // take loan if don't have enough money to buy inputs
                if (Country.Science.IsInvented(Invention.Banking) && !artisansProduction.isAllInputProductsCollected()
                    && artisansProduction.Type.getPossibleProfit(Country.market).isNotZero())
                {
                    var needs = artisansProduction.getRealAllNeeds();
                    if (!artisansProduction.CanAfford(needs))
                    {
                        var loanSize = Country.market.getCost(needs); // takes little more than really need, could be fixed
                        Bank.GiveCredit(this, loanSize);
                        PayWithoutRecord(artisansProduction, Cash, Register.Account.MarketOperations);
                    }
                }

                artisansProduction.consumeNeeds();
                artisansProduction.PayWithoutRecord(this, artisansProduction.Cash, Register.Account.MarketOperations);

                // consuming made in artisansProduction.consumeNeeds()
                // here is data transferring
                // todo rework data transferring from artisans?
                foreach (var item in artisansProduction.AllConsumedInMarket())
                {
                    consumedInMarket.Add(item);
                }

                foreach (var item in artisansProduction.getConsumed())
                {
                    consumed.Add(item);
                }

                getConsumedLastTurn().Add(artisansProduction.getConsumedLastTurn());
            }
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
            return true;
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
                    return Options.PopMiddleStrataVotePower;
                else
                    return 1;
            else
                return 0;
        }

        private void changeProductionType()
        {
            var newProductionType = ProductionType.getAllInventedArtisanships(Country)
                .Where(x => !x.isResourceGathering()
                && x.basicProduction.Product != Product.Education
                && x.getPossibleProfit(Country.market).isNotZero())
                .MaxBy(x => x.getPossibleProfit(Country.market).Get());

            if (newProductionType != null)
                if (artisansProduction == null
                    || (artisansProduction != null && newProductionType != artisansProduction.Type))

                {
                    artisansProduction = new ArtisanProduction(newProductionType, Province, this);
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
        public Procent getInputFactor()
        {
            if (artisansProduction == null)
                return null;
            else
                return artisansProduction.getInputFactor();
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            if (artisansProduction != null)
                artisansProduction.SetStatisticToZero();
        }

        public ProductionType Type
        {
            get
            {
                if (artisansProduction == null)
                    return null;
                else
                    return artisansProduction.Type;
            }
        }

        public void checkProfit()
        {
            // todo doesn't include taxes. Should it?
            if (artisansProduction == null
                || Register.Income.Copy().Subtract(artisansProduction.getExpences(), false).isZero())
                changeProductionType();
        }
    }
}