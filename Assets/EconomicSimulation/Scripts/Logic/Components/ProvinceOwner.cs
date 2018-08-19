using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nashet.EconomicSimulation
{   
    /// <summary>
    /// Represents ability to own provinces 
    /// </summary>
    public class ProvinceOwner : Component<Country>, IProvinceOwner, IPopulated
    {
        protected readonly List<Province> ownedProvinces = new List<Province>();
       

        public ProvinceOwner(Country owner):base (owner)
        {
            
        }
        //public IEnumerator<Province> GetEnumerator()
        //{
        //    foreach (var province in ownedProvinces)
        //        yield return province;
        //}

        public IEnumerable<Province> AllProvinces
        {
            get
            {
                foreach (var province in ownedProvinces)
                    yield return province;
            }
        }
        
        
        /// <summary>
        /// Has duplicates!
        /// </summary>
        public IEnumerable<Province> AllNeighborProvinces()
        {
            //var res = Enumerable.Empty<Province>();
            foreach (var province in ownedProvinces)
                foreach (var neighbor in province.AllNeighbors().Where(p => p.Country != owner))
                    yield return neighbor;

            //List<Province> result = new List<Province>();
            //foreach (var province in ownedProvinces)
            //    result.AddRange(
            //        province.getAllNeighbors().Where(p => p.Country != this && !result.Contains(p))
            //        );
            //return result;
        }
        /// <summary>
        /// Has duplicates!
        /// </summary>
        public IEnumerable<Country> AllNeighborCountries()
        {
            //var res = Enumerable.Empty<Province>();
            foreach (var province in ownedProvinces)
                foreach (var neighbor in province.AllNeighbors().Where(neigbor => neigbor.Country != owner))
                    yield return neighbor.Country;

            //List<Province> result = new List<Province>();
            //foreach (var province in ownedProvinces)
            //    result.AddRange(
            //        province.getAllNeighbors().Where(p => p.Country != this && !result.Contains(p))
            //        );
            //return result;
        }
        
        public IEnumerable<ISeller> AllSellers
        {
            get
            {
                foreach (var province in ownedProvinces)
                    foreach (var agent in province.AllProducers)
                        yield return agent;
                yield return owner;
            }
        }
        //public IEnumerable<PopUnit> GetAllPopulation(PopType type)
        //{
        //    foreach (var province in ownedProvinces)
        //    {
        //        foreach (var item in province.AllPops.Where(x => x.Type == type))
        //        {
        //            yield return item;
        //        }
        //    }
        //}

        public IEnumerable<PopUnit> AllPops
        {
            get
            {
                foreach (var province in ownedProvinces)
                    foreach (var pops in province.AllPops)
                        yield return pops;
            }
        }
        /// Doesn't include markets
        /// </summary>        
        public IEnumerable<Consumer> AllConsumers
        {
            get
            {
                foreach (var province in ownedProvinces)
                    foreach (var agent in province.AllConsumers)
                        yield return agent;
                yield return owner;
            }
        }
        public IEnumerable<Producer> AllProducers
        {
            get
            {
                foreach (var province in ownedProvinces)
                    foreach (var agent in province.AllProducers)
                        yield return agent;
            }
        }

        public IEnumerable<Factory> AllFactories
        {
            get
            {
                foreach (var province in ownedProvinces)
                {
                    foreach (var item in province.AllFactories)
                    {
                        yield return item;
                    }
                }
            }
        }
        /// <summary>
        /// Doesn't include markets
        /// </summary>        
        public IEnumerable<Agent> AllAgents
        {
            get
            {
                foreach (var province in ownedProvinces)
                    foreach (var agent in province.AllAgents)
                        yield return agent;
                if (owner.Bank != null)
                    yield return owner.Bank;
            }
        }
        /// <summary>
        /// Returns last escape type - demotion, migration or immigration
        /// </summary>
        public IEnumerable<KeyValuePair<IWayOfLifeChange, int>> AllPopsChanges
        {
            get
            {
                foreach (var item in AllPops)
                    foreach (var record in item.getAllPopulationChanges())
                        yield return record;
            }
        }
        public List<Country> AllCoresOnMyland()
        {
            var res = new List<Country>();
            foreach (var province in ownedProvinces)
            {
                foreach (var core in province.AllCores())
                {
                    if (!res.Contains(core))
                        res.Add(core);
                }
            }
            return res;
        }
        public bool HasCore(Country country)
        {
            return ownedProvinces.Any(x => x.isCoreFor(country));
        }
        public Procent getYesVotes(IReformValue reform, ref Procent procentPopulationSayedYes)
        {
            // calculate how much of population wants selected reform
            int totalPopulation = AllPops.Sum(x => x.population.Get());
            int votingPopulation = 0;
            int populationSayedYes = 0;
            int votersSayedYes = 0;
            Procent procentVotersSayedYes = new Procent(0);
            //Procent procentPopulationSayedYes = new Procent(0f);
            foreach (Province province in ownedProvinces)
                foreach (PopUnit pop in province.AllPops)
                {
                    if (pop.CanVoteInOwnCountry())
                    {
                        if (pop.getSayingYes(reform))
                        {
                            votersSayedYes += pop.population.Get();// * pop.getVotingPower();
                            populationSayedYes += pop.population.Get();// * pop.getVotingPower();
                        }
                        votingPopulation += pop.population.Get();// * pop.getVotingPower();
                    }
                    else
                    {
                        if (pop.getSayingYes(reform))
                            populationSayedYes += pop.population.Get();// * pop.getVotingPower();
                    }
                }
            if (totalPopulation != 0)
                procentPopulationSayedYes.Set((float)populationSayedYes / totalPopulation);
            else
                procentPopulationSayedYes.Set(0);

            if (votingPopulation == 0)
                procentVotersSayedYes.Set(0);
            else
                procentVotersSayedYes.Set((float)votersSayedYes / votingPopulation);
            return procentVotersSayedYes;
        }
        
        
        public Dictionary<PopType, int> getYesVotesByType(IReformValue reform, ref Dictionary<PopType, int> divisionPopulationResult)
        {  // division by pop types
            Dictionary<PopType, int> divisionVotersResult = new Dictionary<PopType, int>();
            foreach (PopType popType in PopType.getAllPopTypes())
            {
                divisionVotersResult.Add(popType, 0);
                divisionPopulationResult.Add(popType, 0);
                foreach (Province province in ownedProvinces)
                {
                    foreach (PopUnit pop in province.AllPops.Where(x => x.Type == popType))
                        if (pop.getSayingYes(reform))
                        {
                            divisionPopulationResult[popType] += pop.population.Get();// * pop.getVotingPower();
                            if (pop.CanVoteInOwnCountry())
                                divisionVotersResult[popType] += pop.population.Get();// * pop.getVotingPower();
                        }
                }
            }
            return divisionVotersResult;
        }
       
        /// <summary>
        /// Not finished, don't use it
        /// </summary>
        /// <param name="reform"></param>
        public Procent getYesVotes2(IReformValue reform, ref Procent procentPopulationSayedYes)
        {
            int totalPopulation = AllPops.Sum(x => x.population.Get());
            int votingPopulation = 0;
            int populationSayedYes = 0;
            int votersSayedYes = 0;
            Procent procentVotersSayedYes = new Procent(0f);
            Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
            Dictionary<PopType, int> divisionVotersResult = getYesVotesByType(reform, ref divisionPopulationResult);
            foreach (KeyValuePair<PopType, int> next in divisionVotersResult)
                votersSayedYes += next.Value;

            if (totalPopulation != 0)
                procentPopulationSayedYes.Set((float)populationSayedYes / totalPopulation);
            else
                procentPopulationSayedYes.Set(0);

            if (votingPopulation == 0)
                procentVotersSayedYes.Set(0);
            else
                procentVotersSayedYes.Set((float)votersSayedYes / votingPopulation);
            return procentVotersSayedYes;
        }
        private IEnumerable<IInvestable> GetAllInvestmentProjects()//Agent investor
        {
            foreach (var province in ownedProvinces)
                foreach (var item in province.AllInvestmentProjects())//investor
                {
                    yield return item;
                }
        }
        public Dictionary<IInvestable, Procent> GetAllInvestmentProjects2()
        {
            return GetAllInvestmentProjects().ToDictionary(y => y, x => x.GetMargin());
        }
        public List<Country> AllPotentialSeparatists()
        {
            var res = new List<Country>();
            foreach (var item in AllCoresOnMyland())
            {
                if (!item.IsAlive)
                    res.Add(item);
            }
            return res;
        }
        public void TakeProvince(Province province, bool addModifier)
        {
            Country oldCountry = province.Country;

            province.Country.Provinces.ownedProvinces.Remove(province);
            ownedProvinces.Add(province);
            province.OnSecedeTo(owner, addModifier);
            province.OnSecedeGraphic(owner);

            //kill country or move capital
            if (oldCountry.Provinces.Count == 0)
                oldCountry.OnKillCountry(owner);
            else if (province == oldCountry.Capital)
            {
                oldCountry.MoveCapitalTo(oldCountry.BestCapitalCandidate());
            }

            owner.government.OnReformEnactedInProvince(province);
        }
        public int Count
        {
            get
            {
                return ownedProvinces.Count;
            }
        }
        public int getFamilyPopulation()
        {
            //return  getMenPopulation() * Options.familySize;
            return AllPops.Sum(x => x.population.Get()) * Options.familySize;
        }
        public int getPopulationAmountByType(PopType popType)
        {
            int result = 0;
            foreach (Province province in ownedProvinces)
                result += province.AllPops.Where(x => x.Type == popType).Sum(x => x.population.Get());
            return result;
        }
    }
}
