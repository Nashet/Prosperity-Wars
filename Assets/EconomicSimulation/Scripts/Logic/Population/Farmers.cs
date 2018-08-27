using Nashet.EconomicSimulation.Reforms;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Farmers : GrainGetter
    {
        public Farmers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Farmers, where, culture, oldLife)
        { }

        public Farmers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Farmers, iculture, where)
        {
        }

        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Aristocrats
              || targetType == PopType.Capitalists && Country.Science.IsInvented(Invention.Manufactures)
                )
                return true;
            else
                return false;
        }

        public override void produce()
        {
            Storage producedAmount = new Storage(Type.getBasicProduction().Product, population.Get() * Type.getBasicProduction().get() / 1000f);
            producedAmount.Multiply(modEfficiency.getModifier(this), false); // could be negative with bad modifiers, defaults to zero
            if (producedAmount.isNotZero())
            {
                addProduct(producedAmount);
                storage.add(getGainGoodsThisTurn());
                calcStatistics();
            }
            if (Economy.isMarket.checkIfTrue(Country))
            {
                //sentToMarket.set(gainGoodsThisTurn);
                //Country.market.sentToMarket.add(gainGoodsThisTurn);
                if (getGainGoodsThisTurn().isNotZero())
                    SendToMarket(getGainGoodsThisTurn());
            }
            else
            {
                if (Country.economy == Economy.PlannedEconomy)
                {
                    Country.countryStorageSet.Add(getGainGoodsThisTurn());
                }
            }
        }

        public override bool canSellProducts()
        {
            if (Economy.isMarket.checkIfTrue(Country))
                return true;
            else
                return false;
        }

        public override bool shouldPayAristocratTax()
        {
            return true;
        }

        //public override bool getSayingYes(AbstractReformValue reform)
        //{
        //    if (reform is Government.ReformValue)
        //    {
        //        if (reform == Government.Tribal)
        //        {
        //            var baseOpinion = new Procent(0f);
        //            baseOpinion.add(this.loyalty);
        //            return baseOpinion.get() > Options.votingPassBillLimit;
        //        }
        //        else if (reform == Government.Aristocracy)
        //        {
        //            var baseOpinion = new Procent(0.2f);
        //            baseOpinion.add(this.loyalty);
        //            return baseOpinion.get() > Options.votingPassBillLimit;
        //        }
        //        else if (reform == Government.Democracy)
        //        {
        //            var baseOpinion = new Procent(1f);
        //            baseOpinion.add(this.loyalty);
        //            return baseOpinion.get() > Options.votingPassBillLimit;
        //        }
        //        else if (reform == Government.Despotism)
        //        {
        //            var baseOpinion = new Procent(0.2f);
        //            baseOpinion.add(this.loyalty);
        //            return baseOpinion.get() > Options.votingPassBillLimit;
        //        }
        //        else if (reform == Government.ProletarianDictatorship)
        //        {
        //            var baseOpinion = new Procent(0.3f);
        //            baseOpinion.add(this.loyalty);
        //            return baseOpinion.get() > Options.votingPassBillLimit;
        //        }
        //        else
        //            return false;
        //    }
        //    else if (reform is TaxationForPoor.ReformValue)
        //    {
        //        TaxationForPoor.ReformValue taxReform = reform as TaxationForPoor.ReformValue;
        //        var baseOpinion = new Procent(1f);
        //        baseOpinion.set(baseOpinion.get() - taxReform.tax.get() * 2);
        //        baseOpinion.set(baseOpinion.get() + loyalty.get() - 0.5f);
        //        return baseOpinion.get() > Options.votingPassBillLimit;
        //    }
        //    else
        //        return false;
        //}
        public override bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy)
                && (isStateCulture() || Country.minorityPolicy == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }

        public override int getVotingPower(Government.GovernmentReformValue reformValue)
        {
            if (CanVoteWithThatGovernment(reformValue))
                return 1;
            else
                return 0;
        }
    }
}