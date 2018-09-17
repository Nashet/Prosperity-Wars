using Nashet.EconomicSimulation.Reforms;
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
             || targetType == PopType.Farmers && Country.Science.IsInvented(Invention.Farming)
             )
                return true;
            else
                return false;
        }

        public override bool shouldPayAristocratTax()
        {
            return false;
        }

        public override bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Junta)
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

        public override void produce()
        {
        }

        public void takePayCheck()
        {
            Money payCheck = Country.getSoldierWage().Copy();
            payCheck.Multiply(population.Get() / 1000m);

            if (Country.Pay(this, payCheck, Register.Account.Wage))
                didntGetPromisedSalary = false;
            else
                didntGetPromisedSalary = true;
        }
    }
}