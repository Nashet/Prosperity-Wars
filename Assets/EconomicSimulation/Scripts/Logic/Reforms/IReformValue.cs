using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation.Reforms
{
    public interface IReformValue
    {        
        bool IsAllowed(object firstObject, object secondObject, out string description);
        bool IsAllowed(object firstObject, object secondObject);
        float getVotingPower(PopUnit forWhom);
        /// <summary>
        /// Could be wrong for some reforms! Assumes that reforms go in conservative-liberal order
        /// </summary> 
        bool IsMoreConservativeThan(AbstractReformValue another);
        int GetRelativeConservatism(AbstractReformValue two);
        Procent howIsItGoodForPop(PopUnit pop);
        Procent LifeQualityImpact { get; }
    }
}