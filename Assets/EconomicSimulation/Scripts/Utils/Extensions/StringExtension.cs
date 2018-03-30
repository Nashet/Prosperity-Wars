using UnityEngine;

namespace Nashet.Utils
{
    public static class StringExtension
    {
        /// <summary>
        /// Tested only for English language!
        /// </summary>
        //public static int GetWeight(this string source)
        //{
        //    int res = 0;
        //    for (int i = 0; i < source.Length && i < 3; i++)
        //        if (source[i] != ' ')
        //        {
        //            // aware of int overflowing
        //            res += (int)Mathf.Pow(3 - i, 6) * (char.ToUpper(source[i]) - 64);
        //        }
        //    return res;
        //}
        public static float GetWeight(this string source)
        {
            float res = 0;
            for (int i = 0; i < source.Length && i < 3; i++)
                if (source[i] != ' ')
                    // aware of overflowing
                    res += Mathf.Pow(3 - i, 10) * (char.ToUpper(source[i]) - 64);
            return res;
        }
    }
}