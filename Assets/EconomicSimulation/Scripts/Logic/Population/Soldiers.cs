using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Soldiers : GrainGetter
    {
        public Soldiers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Soldiers, where, culture, oldLife)
        { }

        public Soldiers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Soldiers, iculture, where)
        { }

        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Aristocrats // should be officers
             || targetType == PopType.Artisans
             || targetType == PopType.Farmers && Country.Invented(Invention.Farming)
             )
                return true;
            else
                return false;
        }

        public override bool shouldPayAristocratTax()
        {
            return false;
        }

        public override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Junta)
                && (isStateCulture() || Country.minorityPolicy.getValue() == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }

        public override int getVotingPower(Government.ReformValue reformValue)
        {
            if (canVote(reformValue))
                return 1;
            else
                return 0;
        }

        public override void produce()
        {
        }

        public void takePayCheck()
        {
            Money payCheck = Country.getSoldierWage().Copy();
            payCheck.Multiply(population.Get() / 1000m);
            if (Country.CanPay(payCheck))
            {
                Country.Pay(this, payCheck);
                Country.soldiersWageExpenseAdd(payCheck);
                didntGetPromisedSalary = false;
            }
            else
            {
                didntGetPromisedSalary = true;
                Country.failedToPaySoldiers = true;
            }
        }
    }
}