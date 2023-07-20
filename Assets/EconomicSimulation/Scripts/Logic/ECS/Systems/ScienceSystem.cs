using Leopotam.EcsLite;

namespace Nashet.EconomicSimulation.ECS
{
	sealed class ScienceSystem : IEcsRunSystem
	{
		public void Run(IEcsSystems systems)
		{
			var scienceFilter = systems.GetWorld().Filter<ScienceComponent>().End();

			var sciencePool = systems.GetWorld().GetPool<ScienceComponent>();

			foreach (var entity in scienceFilter)
			{

				ref var science = ref sciencePool.Get(entity);
				if (!science.country.IsAlive)// || science.country == World.UncolonizedLand
				{
					// todo. Should remove component if country is dead
					continue;
				}
				var points = Options.defaultSciencePointMultiplier * Science.modSciencePoints.getModifier(science.country);
				if (Game.devMode)
				{
					points *= 1000f;
				}
				science.Points += points;
			}
		}
	}
}