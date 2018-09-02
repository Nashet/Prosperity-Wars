using Nashet.EconomicSimulation.Reforms;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Workers : GrainGetter
    {
        protected int employed, unemployedButNotSeekingJob;

        public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Workers, where, culture, oldLife)
        { }

        public Workers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Workers, iculture, where)
        { }

        /// <summary>
        /// Only people who really wants to find a job
        /// </summary>        
        public int GetSeekingJobInt()
        {
            return population.Get() - employed - unemployedButNotSeekingJob;
        }

        /// <summary>
        /// All unemployed, including those who doesn't want to find a job
        /// </summary>        
        public override Procent GetUnemployment()
        {
            return new Procent(population.Get() - employed, population.Get(), false); // due to population changes that could be negative            
        }

        /// <summary>
        /// Only people who really wants to find a job
        /// </summary>        
        public override Procent GetSeekingJob()
        {
            //if (Type == PopType.Workers)
            {
                return new Procent(GetSeekingJobInt(), population.Get(), false); // due to population changes that could be negative
                //int employed = 0;
                //foreach (Factory factory in Province.getAllFactories())
                //    employed += factory.HowManyEmployed(this);
                //if (population.Get() - employed <= 0) //happening due population change by growth/demotion
                //    return new Procent(0);
                //return new Procent((population.Get() - employed) / (float)population.Get());
            }
            //else if (type == PopType.Farmers || type == PopType.Tribesmen)
            //{
            //    var overPopulation = Province.GetOverpopulation();
            //    if (overPopulation.isSmallerOrEqual(Procent.HundredProcent))
            //        return new Procent(0f);
            //    else
            //        return new Procent(1f - (1f / overPopulation.get()));
            //}
            //else
            //    return new Procent(0f);
        }


        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Farmers && Country.Science.IsInvented(Invention.Farming)
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

        public override bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.ProletarianDictatorship) // temporally
                && (isStateCulture() || Country.minorityPolicy == MinorityPolicy.Equality)
                )
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

        public void Hire(Factory factory, int toHire)
        {
            employed += toHire;
        }

        // todo add it somewhere
        /// <summary>
        /// States that pops doesn't want a job due to social benefits getting, excluding that worker from labor market
        /// </summary>        
        public void SitOnSocialBenefits(int howMuch)
        {
            unemployedButNotSeekingJob += howMuch;
        }

        public void Fire()
        {
            employed = 0;
            unemployedButNotSeekingJob = 0;
        }
    }
}