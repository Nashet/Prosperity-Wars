using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;

namespace Nashet.Utils
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
        public static T Next<T>(this IEnumerable<T> collection, ref int number)
        {
            if (collection.IsEmpty())
                return default(T);
            var res = collection.ElementAtOrDefault(number);
            if (res == null)
            {
                number = 0;
                res = collection.ElementAtOrDefault(number);
            }
            number++;
            return res;
        }
        //public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Action<TKey, TValue> invokeMe)
        //{
        //    foreach (var keyValue in dictionary)
        //    {
        //        invokeMe(keyValue.Key, keyValue.Value);
        //    }
        //}
        //public static void AddMy(this Dictionary<object, int> dictionary, object what, int size)
        //{
        //    if (dictionary.ContainsKey(what))
        //        dictionary[what] += size;
        //    else
        //        dictionary.Add(what, size);
        //}
        public static IEnumerable<KeyValuePair<Tkey, Procent>> Group<T, Tkey>(this IEnumerable<T> collection, Func<T, Tkey> keySelector, Func<T, int> sumBy)
        {
            var totalPopulation = collection.Sum(x => sumBy(x));
            var query = collection.GroupBy(
        toBeKey => keySelector(toBeKey),
        (group, element) => new KeyValuePair<Tkey, Procent>
        (
             group,
             new Procent(element.Sum(everyElement => sumBy(everyElement)), totalPopulation)
        ));//.OrderByDescending(x => Mathf.Abs(x.Get()));

            return query;
        }

        public static void AddAndSum<T>(this Dictionary<T, int> dictionary, T what, int size)
        {
            if (dictionary.ContainsKey(what))
                dictionary[what] += size;
            else
                dictionary.Add(what, size);
        }

        public static void AddAndSum<T>(this Dictionary<T, decimal> dictionary, T what, decimal size)
        {
            if (dictionary.ContainsKey(what))
                dictionary[what] += size;
            else
                dictionary.Add(what, size);
        }

        public static void AddAndSum<T>(this Dictionary<T, Value> dictionary, T what, Value value)
        {
            if (dictionary.ContainsKey(what))
                dictionary[what].Add(value);
            else
                dictionary.Add(what, value);
        }

        public static void AddAndSum<T>(this Dictionary<T, Money> dictionary, T what, Money value)
        {
            if (dictionary.ContainsKey(what))
                dictionary[what].Add(value);
            else
                dictionary.Add(what, value);
        }

        public static void setMy<T>(this Dictionary<T, Value> dictionary, T what, Value value)
        {
            if (dictionary.ContainsKey(what))
                dictionary[what].Set(value);
            else
                dictionary.Add(what, value);
        }

        public static void AddIfNotNull<T>(this List<T> list, T what)
        {
            if (!what.Equals(default(T)))
                list.Add(what);
        }

        public static void move<T>(this List<T> source, T item, List<T> destination)
        {
            if (source.Remove(item)) // don't remove this
                destination.Add(item);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }

       

        /// <summary>
        /// New value
        /// </summary>
        public static Procent GetAverageProcent<T>(this IEnumerable<T> source, Func<T, Procent> selector) where T:PopUnit
        {
            Procent result = new Procent(0f);
            int calculatedPopulation = 0;
            foreach (var item in source)
            {
                result.AddPoportionally(calculatedPopulation, item.population.Get(), selector(item));
                calculatedPopulation += item.population.Get();
            }
            return result;
        }

        public static void PerformAction<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var item in source)
                action(item);
        }

        //public static void PerformAction<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Action<TSource> action)
        //{
        //    foreach (var item in source)
        //        if (predicate(item))
        //            action(item);
        //}

        /// <summary>
        /// Returns default() if there is source is empty
        /// </summary>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            //if (source == null) throw new ArgumentNullException("source");
            //if (selector == null) throw new ArgumentNullException("selector");
            //todo fix exception
            comparer = comparer ?? Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    //throw new InvalidOperationException("Sequence contains no elements");
                    return default(TSource);
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }

        /// <summary>
        /// Returns default() if there is source is empty
        /// </summary>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
               Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null);
        }

        //public static string ToString<TSource, TKey>(this IEnumerable<TSource> source,
        //       Func<TSource, TKey> selector)
        //{
        //    return source.MaxBy(selector, null).ToString();
        //}

        /// <summary>
        /// Returns the maximal element of the given sequence, based on
        /// the given projection and the specified comparer for projected values.     ///
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparer">Comparer to use to compare projected values</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/>
        /// or <paramref name="comparer"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            //if (source == null) throw new ArgumentNullException(nameof(source)); //todo fix exception
            //if (selector == null) throw new ArgumentNullException(nameof(selector));
            //todo fix exception
            comparer = comparer ?? Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    return default(TSource);
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }

        public static TSource MaxByRandom<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            if (source.IsEmpty())
                return default(TSource);
            var res = source.Max(selector);
            return source.Where(x => selector(x).Equals(res)).Random();
            //var res = source.MaxBy(selector);
            //return source.Where(x => selector(x).Equals(selector(res))).Random();
        }

        public static T Random<T>(this IList<T> collection)
        {
            if (collection.Count == 0)
                return default(T);
            else if (collection.Count == 1)
                return collection[0];
            else
            {
                int index = Rand.Get.Next(collection.Count);
                return collection[index];
            }
        }

        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable.IsEmpty())// || count == 0)
                return default(T);
            else
            {
                var count = enumerable.Count();
                int index = Rand.Get.Next(count);
                return enumerable.ElementAt(index);
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
        //private static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rand.Get.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        ///<summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }

        //public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        //{
        //    if (target == null)
        //        throw new ArgumentNullException("target");
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    foreach (var element in source)
        //        //if (target)
        //        target.Add(element);
        //}
        /// <summary>
        /// returns default(T) if fails
        /// </summary>
        //public static T Random<T>(this List<T> source)
        //{
        //    if (source == null || source.Count == 0)
        //        return default(T);
        //    return source[Rand.random2.Next(source.Count)];

        //}


        public static IEnumerable<T> FirstSameElements<T>(this IEnumerable<T> collection, Func<T, float> selector)
        {
            T previousElement = collection.FirstOrDefault();
            if (previousElement != null)
                foreach (var item in collection)
                    if (selector(item) == selector(previousElement))
                        yield return item;
        }

        /// <summary>
        ///returns an empty List<T> if didn't find anything
        /// </summary>
        //public static T Random<T>(this List<T> source, Predicate<T> predicate)
        //{
        //    return source.FindAll(predicate).Random();
        //    //return source.ElementAt(Rand.random2.Next(source.Count));
        //}

        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            Func<TKey, TValue, bool> predicate)
        {
            var keys = dic.Keys.Where(k => predicate(k, dic[k])).ToList();
            foreach (var key in keys)
            {
                dic.Remove(key);
            }
        }
    }
}