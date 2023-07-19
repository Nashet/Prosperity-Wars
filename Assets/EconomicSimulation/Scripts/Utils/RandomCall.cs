using System;
using Nashet.ValueSpace;

namespace Nashet.Utils
{
    public class RandomCall : Random
    {
        //public static readonly UnityEngine.Random random = new UnityEngine.Random();
        public static readonly Random Get = new Random();        

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