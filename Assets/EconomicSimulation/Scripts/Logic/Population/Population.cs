using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nashet.EconomicSimulation
{
    public class Population
    {
        public static readonly float PopulationMultiplier = 1000f;
        private int population;
        private readonly PopUnit pop;

        public Population(int population, PopUnit pop)
        {            
            this.pop = pop;
            Change(population);
        }

        public int Get()
        {
            return population;
        }

        public void Delete()
        {
            population = 0;
        }

        /// <summary>
        /// Checks - outside
        /// </summary>
        /// <param name="change"></param>
        public void Change(int change)
        {
            //population += change;
            ////throw new NotImplementedException();
            ////because pool aren't implemented yet
            ////Pool.ReleaseObject(this);
            var newPopulation = population + change;
            if (newPopulation > 0)
            {
                population = newPopulation;

                lifeNeeds.Clear();
                foreach (var item in pop.Type.getLifeNeedsPer1000Men())                
                    lifeNeeds.Add(item.Copy().Multiply(population / PopulationMultiplier));

                everydayNeeds.Clear();
                foreach (var item in pop.Type.getEveryDayNeedsPer1000Men())
                    everydayNeeds.Add(item.Copy().Multiply(population / PopulationMultiplier));
                
                luxuryNeeds.Clear();
                foreach (var item in pop.Type.getLuxuryNeedsPer1000Men())
                    luxuryNeeds.Add(item.Copy().Multiply(population / PopulationMultiplier));
            }
            else
                pop.Kill();
        }

        private readonly List<Storage> lifeNeeds = new List<Storage>();
        private readonly List<Storage> everydayNeeds = new List<Storage>();
        private readonly List<Storage> luxuryNeeds = new List<Storage>();

        //return pop.Type.getLifeNeedsPer1000Men().Multiply(new Value(population / 1000f));
        public IEnumerable<Storage> getRealLifeNeeds()
        {
            foreach (var item in lifeNeeds)
            {
                yield return item;
            }
        }

        public IEnumerable<Storage> getRealEveryDayNeeds()
        {
            foreach (var item in everydayNeeds)
            {
                yield return item;
            }
        }

        public IEnumerable<Storage> getRealLuxuryNeeds()
        {
            foreach (var item in luxuryNeeds)
            {
                yield return item;
            }
        }

        public IEnumerable<Storage> getRealAllNeeds()
        {
            //return pop.Type.getAllNeedsPer1000Men().Multiply(new Value(population / 1000f));
            foreach (var item in lifeNeeds)
                yield return item;
            foreach (var item in everydayNeeds)
                yield return item;
            foreach (var item in luxuryNeeds)
                yield return item;
        }
    }
}
