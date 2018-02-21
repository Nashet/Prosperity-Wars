using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Nashet.Utils;
using System;

namespace Nashet.EconomicSimulation
{
    public class Culture : Name
    {
        private readonly Color color;
        public Culture(string name, Color color) : base(name)
        {
            this.color = color;
        }


        public override string ToString()
        {
            return ShortName;
        }

        internal Color getColor()
        {
            return color;
        }
    }
}
