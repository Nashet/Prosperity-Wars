using UnityEngine;
using System;

namespace Nashet.Utils
{
    public static class Rand
    {        
        private static readonly UnityEngine.Random random = new UnityEngine.Random();
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