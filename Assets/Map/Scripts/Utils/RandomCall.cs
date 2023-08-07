using System;

namespace Nashet.Map.Utils
{
    public class Rand : Random
    {
        //public static readonly UnityEngine.Random random = new UnityEngine.Random();
        public static readonly Random Get = new Random();

        /// <summary>
        /// Meaning N chance to 1 to return true
        /// </summary>
        public static bool Chance(int chance)
        {
            return Get.Next(chance - 1) == 0;
        }

        public static float getFloat(float minValue, float maxValue)
        {
            //float m = (maxValue - minValue) ;

            return (float)(Get.NextDouble()) * (maxValue - minValue) + minValue;
        }

		/// <summary>
		/// Meaning N chance to 1 to execute action
		/// </summary>
		public static bool Call(Action action, int chance)
        {
            if (UnityEngine.Random.Range(0, chance - 1) == 0)
            {
                action();
                return true;
            }
            else
                return false;
        }
	}
}