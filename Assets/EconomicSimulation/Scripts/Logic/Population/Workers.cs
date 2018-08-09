namespace Nashet.EconomicSimulation
{
    public class Workers : GrainGetter
    {
        public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Workers, where, culture, oldLife)
        { }

        public Workers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Workers, iculture, where)
        { }

        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Farmers && Country.Inventions.IsInvented(Invention.Farming)
             || targetType == PopType.Artisans
             )
                return true;
            else
                return false;
        }

        public override void produce()
        { }

        public override bool shouldPayAristocratTax()
        {
            return true;
        }

        public override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.ProletarianDictatorship) // temporally
                && (isStateCulture() || Country.minorityPolicy.getValue() == MinorityPolicy.Equality)
                )
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

        public void Hire(Factory factory, int leftToHire)
        {
            employed += leftToHire;
        }

        public void Fire()
        {
            employed = 0;
        }
    }
}