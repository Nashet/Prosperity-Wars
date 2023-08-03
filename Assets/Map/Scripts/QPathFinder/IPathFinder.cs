using System;
using System.Collections.Generic;
using UnityEngine;

namespace QPathFinder
{
	public interface IPathFinder
	{
		void EnableNode(int nodeID, bool enable);
		void EnablePath(int pathID, bool enable);
		int FindNearestNode(Vector3 point);
		void FindShortestPathOfNodes(int fromNodeID, int toNodeID, Execution executionType, Action<List<Node>> callback, Predicate<IProvince> predicate = null);
	}
}