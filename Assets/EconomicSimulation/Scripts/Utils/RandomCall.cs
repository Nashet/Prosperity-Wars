using System;
using Nashet.ValueSpace;

namespace Nashet.Utils
{
    public class Rand : Random
    {
        //public static readonly UnityEngine.Random random = new UnityEngine.Random();
        public static readonly Random Get = new Random();

        //public static bool Chance(int chance)
        //{
        //    return random2.Next(chance) == 0;
        //}
        public static float getFloat(float minValue, float maxValue)
        {
            //float m = (maxValue - minValue) ;

            return (float)(Get.NextDouble()) * (maxValue - minValue) + minValue;
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
            return Get.Next(Procent.Precision) < chance.RawUIntValue;
        }

        public static bool Call(Action action, int chance)
        {
            if (UnityEngine.Random.Range(0, chance) == 0)
            {
                action();
                return true;
            }
            else
                return false;
        }

        public static bool Call(Action action, ReadOnlyValue chance)
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