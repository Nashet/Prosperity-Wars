using Leopotam.EcsLite;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation.ECS
{
	sealed class CountryCoresSystem : IEcsRunSystem
	{
		public void Run(IEcsSystems systems)
		{
			var filter = systems.GetWorld().Filter<CountryCoresComponent>().End();

			var pool = systems.GetWorld().GetPool<CountryCoresComponent>();

			foreach (var entity in filter)
			{
				if (Rand.Get.Next(Options.ProvinceChanceToGetCore) != 1)//todo maybe add entity skipping instead of that
					continue;

				ref var component = ref pool.Get(entity);
				var Country = component.province.Country;
				{
					if (!component.cores.Contains(Country)
						&& component.province.AllNeighbors().Any(x => x.isCoreFor(Country))
						&& component.province.getMajorCulture() == Country.Culture)
					{
						component.cores.Add(Country);
					}
				}
			}
		}
	}
}