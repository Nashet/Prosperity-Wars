using Leopotam.EcsLite;
using System;

namespace Nashet.EconomicSimulation.ECS
{
	public static class ECSUtils { 
		public static T GetSingleComponent<T>(this EcsWorld world) where T : struct
		{
			var pool = world.GetPool<T>();
			var filter = world.Filter<T>().End();

			int counter = 0;
			T map = default;

			foreach (int entity in filter)
			{
				ref T component = ref pool.Get(entity);
				map = component;
				counter++;
			}

			if (counter != 1)
			{
				throw new ArgumentOutOfRangeException($"Wrong amount of components: {counter}");
			}

			return map;
		}

		public static int Add<T>(this int entity, EcsPool<T> pool) where T : struct
		{
			pool.Add(entity);
			return entity;
		}

		public static ref T AddnSet<T>(this int entity, EcsPool<T> pool) where T : struct
		{
			pool.Add(entity);
			ref var component = ref pool.Get(entity);
			return ref component;
		}
		public static ref T UnpackComponent<T>(this EcsPackedEntity packed, EcsWorld world, EcsPool<T> pool) where T : struct
		{
			packed.Unpack(world, out int entity);			
			return ref pool.Get(entity);
		}
	}
}
