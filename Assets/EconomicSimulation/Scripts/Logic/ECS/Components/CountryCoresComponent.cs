using System.Collections.Generic;

namespace Nashet.EconomicSimulation.ECS
{
	public struct CountryCoresComponent
	{
		public HashSet<Country> cores;
		public Province province;//todo temporal solution, fixit
	}
}