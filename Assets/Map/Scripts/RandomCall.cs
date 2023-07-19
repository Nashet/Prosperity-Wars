using System;

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
	}
}