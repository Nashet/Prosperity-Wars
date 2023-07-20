using Leopotam.EcsLite;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation.ECS
{
	sealed class ProduceSystem : IEcsRunSystem
	{
		public void Run(IEcsSystems systems)
		{
			var filter = systems.GetWorld().Filter<ProducerComponent>().End();//todo cache it?

			var productionPool = systems.GetWorld().GetPool<ProducerComponent>();
			//var populationPool = systems.GetWorld().GetPool<WorkforceComponent>();

			foreach (var entity in filter)
			{
				ref var production = ref productionPool.Get(entity);

				int workforce; // should be variable
							   //if (populationPool.Has(entity))
							   //{
							   //ref var population = ref populationPool.Get(entity);
				workforce = 1111;// population.population;
				//}


				var modifier = 1.2f; // should be variable
				//farmers dont have input

				produce(production, workforce, modifier);
				//component.Country.producedTotalAdd(produced);
			}
		}

		private void produce(ProducerComponent component, int workforce, float modifier)
		{
			//var production = component.type.basicProduction.get() * workforce * modifier;
			//return production;
			var pop = component.pop;
			Storage producedAmount;
			var overpopulation = pop.Province.GetOverpopulation();

			//producedAmount = new Storage(pop.Type.getBasicProduction().Product, pop.Type.getBasicProduction().Multiply(pop.population.Get()).Divide(1000));

			//if (!overpopulation.isSmallerOrEqual(Procent.HundredProcent)) // all is OK
			//{
			//	producedAmount.Divide(overpopulation);
			//}

			if (overpopulation.isSmallerOrEqual(Procent.HundredProcent)) // all is OK
				producedAmount = new Storage(pop.Type.getBasicProduction().Product, pop.Type.getBasicProduction().Multiply(pop.population.Get()).Divide(1000));
			else
				producedAmount = new Storage(pop.Type.getBasicProduction().Product, pop.Type.getBasicProduction().Multiply(pop.population.Get()).Divide(1000).Divide(overpopulation));

			if (producedAmount.isNotZero())
			{
				pop.storage.add(producedAmount);
				pop.addProduct(producedAmount);
				pop.calcStatistics();
			}
		}

		//protected Procent getInputFactor(Procent multiplier)



		//Thats for differen systems:

		//todo thats for consumption system
		//if (!component.type.isResourceGathering())
		//	foreach (Storage next in getRealAllNeeds())
		//		getInputProductsReserve().Subtract(next, false);

		//getRealAllNeeds()??
		//changeProductionType()
		//SetStatisticToZero()
		//calcStatistic
		//getUpgradeNeeds()

		//setPriority - only needed for hiring with PlannedEconomy
		//GetSalary?


	}
}