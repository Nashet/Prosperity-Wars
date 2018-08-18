using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public interface IReformValue
    {        
        bool IsAllowed(object firstObject, object secondObject, out string description);
        bool IsAllowed(object firstObject, object secondObject);
        float getVotingPower(PopUnit forWhom);
        bool IsMoreConservative(AbstractReformValue another);
        Procent howIsItGoodForPop(PopUnit pop);
        Procent LifeQualityImpact { get; }
    }

}