
using System.Collections.Generic;
using Nashet.ValueSpace;
using Nashet.Conditions;

namespace Nashet.EconomicSimulation
{
    public class Separatism : AbstractReformValue
    {
        private static readonly List<Separatism> allSeparatists = new List<Separatism>();
        private static readonly Procent willing = new Procent(3f);
        private readonly Condition separatismAllowed;

        private readonly Country separatismTarget;

        private Separatism(Country country) : base(country.getName() + " independence", "", 0,
            new ConditionsListForDoubleObjects())//new ConditionsList(Condition.AlwaysYes))
        {
            separatismAllowed = new Condition(x => isAvailable(x as Country), "Separatism target is valid", true);
            allowed.add(separatismAllowed);
            separatismTarget = country;
            allSeparatists.Add(this);
        }

        internal static Separatism find(Country country)
        {
            var found = allSeparatists.Find(x => x.separatismTarget == country);
            if (found == null)
                return new Separatism(country);
            else
                return found;
        }
        protected override Procent howIsItGoodForPop(PopUnit pop)
        {
            //return Procent.HundredProcent;
            return willing;
        }

        internal override bool isAvailable(Country country)
        {
            return !separatismTarget.isAlive();
        }

        internal Country getCountry()
        {
            return separatismTarget;
        }
    }
}