using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public interface IPopulated
    {        
        IEnumerable<Producer> AllProducers { get; }
        IEnumerable<Consumer> AllConsumers { get; }
        IEnumerable<ISeller> AllSellers { get; }
        IEnumerable<Agent> AllAgents { get; }
        IEnumerable<PopUnit> AllPops{ get; }
        //IEnumerable<Workers> AllWorkers { get; }
        IEnumerable<Factory> AllFactories { get; }
        IEnumerable<KeyValuePair<IWayOfLifeChange, int>> AllPopsChanges { get; }
    }
}