using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Nashet.Utils;
using System;
using Nashet.ValueSpace;

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

        internal Color getColor()
        {
            return color;
        }
        /// <summary>
        /// Just a place holder, not used
        /// </summary>        
        public ReadOnlyValue getLifeQuality(PopUnit pop, PopType proposedType)
        {
            throw new NotImplementedException();
        }
    }
}
