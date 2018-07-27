using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nashet.EconomicSimulation
{
    public static class  Diplomacy
    {
        private static readonly Dictionary<Country, Country> wars = new Dictionary<Country, Country>();
        public static void DeclareWar(Country attacker, Country defender)
        {
            if (!IsInWar(attacker, defender))
                wars.Add(attacker, defender);
        }
        public static bool IsInWar(Country countryOne, Country countryTwo)
        {
            if (wars.ContainsKey(countryOne))
                return true;
            else
                return wars.ContainsKey(countryTwo);
        }
    }
}
