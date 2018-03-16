using UnityEngine;
using System;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Linq;

namespace Nashet.Utils
{
    public class Rand : System.Random
    {
        //public static readonly UnityEngine.Random random = new UnityEngine.Random();
        public static readonly System.Random random2 = new System.Random();
        //public static bool Chance(int chance)
        //{
        //    return random2.Next(chance) == 0;
        //}
        public static float getFloat(float minValue, float maxValue)
        {
            //float m = (maxValue - minValue) ;

            return (float)(random2.NextDouble()) * (maxValue - minValue) + minValue;
        }
        /// <summary>
        /// Higher procent - higher chance
        /// </summary>            
        public static bool Chance(ReadOnlyValue chance)
        {
            //if (chance.isZero())
            //    return false;
            //else
            //excluding Procent.Precision
            return random2.Next(Procent.Precision) < chance.RawUIntValue;
        }
        internal static bool Call(Action action, int chance)
        {
            if (UnityEngine.Random.Range(0, chance) == 0)
            {
                action();
                return true;
            }
            else
                return false;
        }
        internal static bool Call(Action action, ReadOnlyValue chance)
        {
            if (Chance(chance))
            {
                action();
                return true;
            }
            else
                return false;
        }
    }
}