using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Nashet.EconomicSimulation
{
    public class Culture: ISortable
    {
        private readonly string name;
        private readonly List<Culture> allCultures = new List<Culture>();
        public Culture(string iname)
        {
            name = iname;
            allCultures.Add(this);
        }

        public float getSortRank()
        {
            return GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }
    }
}
