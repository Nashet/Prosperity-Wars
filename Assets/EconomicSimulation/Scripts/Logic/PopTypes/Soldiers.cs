using UnityEngine;

using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Soldiers : GrainGetter
    {
        public Soldiers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Soldiers, where, culture)
        { }
        public Soldiers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Soldiers, iculture, where)
        { }
        public override bool canThisDemoteInto(PopType targetType)
        {
            if (//targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
                //||
                targetType == PopType.Tribesmen
                || targetType == PopType.Workers
                )
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Aristocrats // should be officers
             || targetType == PopType.Artisans
             || targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
             )
                return true;
            else
                return false;
        }

        public override bool shouldPayAristocratTax()
        {
            return false;
        }

        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Junta)
                && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }
        internal override int getVotingPower(Government.ReformValue reformValue)
        {
            if (canVote(reformValue))
                return 1;
            else
                return 0;
        }

        public override void produce()
        {

        }

        internal void takePayCheck()
        {
            Value payCheck = new Value(getCountry().getSoldierWage());
            payCheck.multiply(getPopulation() / 1000f);
            if (getCountry().canPay(payCheck))
            {
                getCountry().pay(this, payCheck);
                getCountry().soldiersWageExpenseAdd(payCheck);
                this.didntGetPromisedSalary = false;
            }
            else
            {
                this.didntGetPromisedSalary = true;
                getCountry().failedToPaySoldiers = true;
            }
        }
    }
}