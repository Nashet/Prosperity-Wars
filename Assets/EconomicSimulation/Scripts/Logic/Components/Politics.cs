using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nashet.EconomicSimulation
{
    public class Politics : Component<Country>
    {
        protected readonly List<AbstractReform> reforms = new List<AbstractReform>();
        protected readonly List<Movement> movements = new List<Movement>();

        public Politics(Country owner) : base(owner)
        {
        }
        public IEnumerable<Movement> AllMovements
        {
            get
            {
                foreach (var movement in movements)
                    yield return movement;
            }
        }

        public IEnumerable<AbstractReform> AllReforms
        {
            get
            {
                foreach (var reform in reforms)
                    yield return reform;
            }
        }

        internal void Simulate()
        {
            //movements
            movements.RemoveAll(x => x.isEmpty());
            foreach (var item in movements.ToArray())
                item.Simulate();
        }

        internal void RegisterMovement(Movement movement)
        {
            movements.Add(movement);
        }

        internal void RemoveMovement(Movement movement)
        {
            movements.Remove(movement);
        }
        internal void RegisterReform(AbstractReform abstractReform)
        {
            reforms.Add(abstractReform);
        }
    }
}
