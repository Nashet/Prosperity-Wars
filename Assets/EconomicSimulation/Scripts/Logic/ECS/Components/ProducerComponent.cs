using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation.ECS
{
	//public enum ProductionStatus { open, closed, upgrading }
	public struct ProducerComponent
	{
		public ProductionType type;
		//public Product product;
		public decimal gainGoodsThisTurn;//for statistics?
		public decimal storage;
		public StorageSet inputProductsReserve;// = new StorageSet();//todo maybe put in consuption component
		public PopUnit pop;
		//public bool isWorking;

											   

		// base.produce(new Value(artisan.population.Get() * PopUnit.modEfficiency.getModifier(artisan) * Options.ArtisansProductionModifier * getInputFactor().get() / 1000f));

		

	}
}