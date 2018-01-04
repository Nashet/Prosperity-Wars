using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Nashet.EconomicSimulation
{
    public class Culture
    {
        string name;
        List<Culture> allCultures = new List<Culture>();
        public Culture(string iname)
        {
            name = iname;
            allCultures.Add(this);
        }
        public override string ToString()
        {
            return name;
        }
    }
}
