using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nashet.EconomicSimulation
{
    public class Politics : Component<Country>, IStatisticable
    {
        protected readonly List<AbstractReform> reforms = new List<AbstractReform>();
        protected readonly List<Movement> movements = new List<Movement>();

        //protected readonly Money defaultedSocialObligations = new Money(0m);
        //protected readonly Money lastTurnDefaultedSocialObligations = new Money(0m);
        //public MoneyView LastTurnDefaultedSocialObligations { get { return lastTurnDefaultedSocialObligations; } }

        public Politics(Country owner) : base(owner)
        {
        }

        /// <summary>
        /// temporally
        /// </summary>
        //public void RegisterDefaultedSocialObligations(MoneyView size)
        //{
        //    defaultedSocialObligations.Add(size);
        //}

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
            reforms.Sort((x, y) => x.ShowOrder - y.ShowOrder);
        }

        public void SetStatisticToZero()
        {
            //lastTurnDefaultedSocialObligations.Set(defaultedSocialObligations);
            //defaultedSocialObligations.SetZero();
        }

        /// <summary>
        /// Gets reform which can take given value
        /// </summary>
        public AbstractReform GetReform(IReformValue abstractReformValue)
        {
            foreach (var item in reforms)
            {
                if (item.AllPossibleValues.Any(x => x == abstractReformValue))
                    return item;
            }
            return null;
        }

    }
}
