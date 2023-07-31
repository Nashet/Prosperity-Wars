using System.Collections.Generic;

namespace QPathFinder
{
	public interface IGraphData
	{
		List<Node> nodes { get; }
		List<Path> paths { get; }

		void ReGenerateIDs();
	}
}