using System;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Culture : Name, IWayOfLifeChange
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

        public Color getColor()
        {
            return color;
        }

        /// <summary>
        /// Just a place holder, not used
        /// </summary>
        public ReadOnlyValue getLifeQuality(PopUnit pop)
        {
            throw new NotImplementedException();
        }
    }
}