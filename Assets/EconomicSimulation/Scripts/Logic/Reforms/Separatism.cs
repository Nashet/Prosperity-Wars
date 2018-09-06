using Nashet.Conditions;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class Separatism : AbstractReform
    {
        protected static readonly Procent willing = new Procent(3f);
        protected readonly Condition separatismAllowed;
        protected Goal typedValue { get; }
        public Country goal { get { return typedValue.separatismTarget; } }

        protected Separatism(Country country) : base(country.ShortName + " independence", "", country,
          0, null)//new ConditionsList(Condition.AlwaysYes))
        {
            //separatismAllowed = new Condition(x => isAvailable(x as Country), "Separatism target is valid", true);
            //allowed.add(separatismAllowed);
            //separatismTarget = country;

        }       

        protected static List<Goal> allSeparatists = new List<Goal>();
        public static Goal Get(Country country)
        {
            //possibleValues
            var found = allSeparatists.Find(x => x.separatismTarget == country);
            if (found == null)
            {
                var res = new Goal(country);
                allSeparatists.Add(res);
                return res;
            }
            else
                return found;
        }



        public class Goal : AbstractReformValue
        {
            public Country separatismTarget { get; protected set; }

            internal Goal(Country separatismTarget) : base(0, new DoubleConditionsList())
            {
                this.separatismTarget = separatismTarget;
            }

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                return willing;
            }
            public override string ToString()
            {
                return separatismTarget + " separatists";
            }
        }
    }
}