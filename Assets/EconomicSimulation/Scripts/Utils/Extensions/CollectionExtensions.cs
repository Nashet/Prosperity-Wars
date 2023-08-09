using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
namespace Nashet.Utils
{
	public static class CollectionExtensions
    {
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

		/// <summary>
		/// New value
		/// </summary>
		public static Procent GetAverageProcent<T>(this IEnumerable<T> source, Func<T, Procent> selector) where T : PopUnit
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
	}
}