using UnityEngine;
using UnityEditor;
namespace Nashet.EconomicSimulation
{
    public class Workers : GrainGetter
    {
        public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Workers, where, culture)
        { }
        public Workers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Workers, iculture, where)
        { }

        public override bool canThisDemoteInto(PopType targetType)
        {
            if (targetType == PopType.Tribesmen
                || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy))
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
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

        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.ProletarianDictatorship) // temporally
                && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality)
                )
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
    }
}