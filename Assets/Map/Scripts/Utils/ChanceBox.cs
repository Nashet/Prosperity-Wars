using System.Collections.Generic;
using System.Linq;

namespace Nashet.Map.Utils
{
	public class ChanceBox<T> : IChanceBox<T>
	{
		private class Mean
		{
			//KeyValuePair<string,int>
			public T element;

			public float weight;

			public Mean(T obj, float inchance)
			{
				element = obj;
				weight = inchance;
			}

			public override string ToString()
			{
				return element + " " + weight;
			}
		}

		//SortedDictionary
		//SortedDictionary<T, float> list = new SortedDictionary<T, float>();
		//todo make it dictionary
		private List<Mean> list = new List<Mean>();

		public void Add(T obj, float chance)
		{
			list.Add(new Mean(obj, chance));
		}

		public void Initiate()
		{
			float totalWeight = 0f;

			//list = list.OrderByDescending(o => o.weight).ToList();
			list = list.OrderBy(o => o.weight).ToList();
			int count = list.Count;
			foreach (var next in list)
			{
				// next.weight += count;
				totalWeight += next.weight;
				count--;
			}

			foreach (Mean next in list)
			{
				next.weight = next.weight / totalWeight;
				//next.weight = next.weight / list.Count ;
			}
			for (int i = 1; i < list.Count; i++)
			{
				list[i].weight += list[i - 1].weight;
			}
		}

		/// <summary>Gives random T according element weight  /// </summary>
		public T GetRandom()
		{
			float randomNumber = Rand.getFloat(0f, 1f);
			foreach (Mean next in list)
				if (randomNumber <= next.weight)
					return next.element;
			return default(T);
		}
	}
}