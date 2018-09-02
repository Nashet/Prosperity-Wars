using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.Utils
{
    public static class MonoBehaviourExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static IEnumerable<GameObject> AllParents(this GameObject that)
        {
            GameObject nextParent;
            if (that.transform.parent == null)
                nextParent = null;
            else
                nextParent = that.transform.parent.gameObject;
            while (nextParent != null)
            {
                yield return nextParent;
                if (nextParent.transform.parent == null)
                    nextParent = null;
                else
                    nextParent = nextParent.transform.parent.gameObject;
            }
        }

        public static bool HasComponent<T>(this MonoBehaviour that)
        {
            if (that.GetComponent<T>() == null)
                return false;
            else
                return true;
        }

        public static bool HasComponent<T>(this GameObject that)
        {
            if (that.GetComponent<T>() == null)
                return false;
            else
                return true;
        }

        public static bool HasComponentInParent<T>(this GameObject that)
        {
            if (that.transform.parent == null || that.transform.parent.GetComponent<T>() == null)
                return false;
            else
                return true;
        }

        public static bool HasComponentInParent<T>(this MonoBehaviour that)
        {
            if (that.transform.parent == null || that.transform.parent.GetComponent<T>() == null)
                return false;
            else
                return true;
        }

        public static bool HasComponentInParentParent<T>(this MonoBehaviour that)
        {
            if (that.transform.parent == null || that.transform.parent.parent == null || that.transform.parent.parent.GetComponent<T>() == null)
                return false;
            else
                return true;
        }
    }
}