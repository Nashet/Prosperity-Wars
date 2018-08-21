using Nashet.EconomicSimulation.Reforms;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Tribesmen : CattleGetter
    {
        public Tribesmen(PopUnit pop, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange oldLife) : base(pop, sizeOfNewPop, PopType.Tribesmen, where, culture, oldLife)
        {
        }

        public Tribesmen(int iamount, Culture iculture, Province where) : base(iamount, PopType.Tribesmen, iculture, where)
        {
        }

        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Aristocrats
                //|| targetType == PopType.Farmers && Country.isInvented(Invention.Farming)
                //|| targetType == PopType.Soldiers && !Country.isInvented(Invention.ProfessionalArmy))
                )
                return true;
            else
                return false;
        }

        public override void produce()
        {
            Storage producedAmount;
            var overpopulation = Province.GetOverpopulation();
            if (overpopulation.isSmallerOrEqual(Procent.HundredProcent)) // all is OK
                producedAmount = new Storage(Type.getBasicProduction().Product, Type.getBasicProduction().Multiply(population.Get()).Divide(1000));
            else
                producedAmount = new Storage(Type.getBasicProduction().Product, Type.getBasicProduction().Multiply(population.Get()).Divide(1000).Divide(overpopulation));

            if (producedAmount.isNotZero())
            {
                storage.add(producedAmount);
                addProduct(producedAmount);
                calcStatistics();
            }
        }

        public override bool canTrade()
        {
            return false;
        }

        public override bool shouldPayAristocratTax()
        {
            return true;
        }

        public override bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform)
        {
            if ((reform == Government.Tribal || reform == Government.Democracy)
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

        public override void consumeNeeds()
        {
            //life needs First
            // Don't need education
            consumeWithNaturalEconomy(population.getRealLifeNeeds());
        }
    }
}