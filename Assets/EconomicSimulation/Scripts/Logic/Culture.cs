using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class Culture: Name
    {
        //private readonly string name;
        private readonly List<Culture> allCultures = new List<Culture>();
        private readonly float nameWeight;
        public Culture(string name):base (name)
        {
            
            //this.name = name;
            allCultures.Add(this);
        }

        

        public override string ToString()
        {
            return ShortName;
        }
    }
}
