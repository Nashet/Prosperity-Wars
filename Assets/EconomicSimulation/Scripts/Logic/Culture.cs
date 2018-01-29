using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class Culture: ISortableName
    {
        private readonly string name;
        private readonly List<Culture> allCultures = new List<Culture>();
        private readonly float nameWeight;
        public Culture(string name)
        {
            nameWeight = name.GetWeight();
            this.name = name;
            allCultures.Add(this);
        }

        public float GetNameWeight()
        {
            return nameWeight;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
