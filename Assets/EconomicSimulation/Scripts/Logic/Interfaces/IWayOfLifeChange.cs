using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public interface IWayOfLifeChange
    {
        //bool HasJobsFor(PopType popType, Province province);
        ReadOnlyValue getLifeQuality(PopUnit pop);

        //string getWayOfLifeString(PopUnit pop);
    }
}