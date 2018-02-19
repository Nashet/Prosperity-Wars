using UnityEngine;

using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Tribesmen : CattleGetter
    {
        public Tribesmen(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Tribesmen, where, culture)
        {
        }
        public Tribesmen(int iamount, Culture iculture, Province where) : base(iamount, PopType.Tribesmen, iculture, where)
        {
        }
        public override bool canThisDemoteInto(PopType targetType)
        {
            if (targetType == PopType.Workers
                || targetType == PopType.Farmers && Country.Invented(Invention.Farming) 
                || targetType == PopType.Soldiers && Country.Invented(Invention.ProfessionalArmy))
                return true;
            else
                return false;
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
                producedAmount = new Storage(Type.getBasicProduction().Product, Type.getBasicProduction().Multiply(getPopulation()).Divide(1000));
            else
                producedAmount = new Storage(Type.getBasicProduction().Product, Type.getBasicProduction().Multiply(getPopulation()).Divide(1000).Divide(overpopulation));


            if (producedAmount.isNotZero())
            {
                storage.add(producedAmount);
                addProduct(producedAmount);
                calcStatistics();
            }
        }
        internal override bool canTrade()
        {
            return false;
        }
        public override bool shouldPayAristocratTax()
        {
            return true;
        }

        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Tribal || reform == Government.Democracy)
                && (isStateCulture() || Country.minorityPolicy.getValue() == MinorityPolicy.Equality))
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
        public override void consumeNeeds()
        {
            //life needs First
            // Don't need education
            consumeWithNaturalEconomy(getRealLifeNeeds());
        }
    }
}