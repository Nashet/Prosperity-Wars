using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation.Reforms
{
    public interface IReformValue
    {
        int ID { get; }
        bool IsAllowed(object firstObject, object secondObject, out string description);
        bool IsAllowed(object firstObject, object secondObject);
        float getVotingPower(PopUnit forWhom);
        /// <summary>
        /// Could be wrong for some reforms! Assumes that reforms go in conservative-liberal order
        /// </summary> 
        bool IsMoreConservativeThan(IReformValue another);
        int GetRelativeConservatism(IReformValue two);
        Procent howIsItGoodForPop(PopUnit pop);
        Procent LifeQualityImpact { get; }
    }
}