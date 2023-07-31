using UnityEngine;

namespace QPathFinder
{
	public interface INode
	{
		float combinedHeuristic { get; }
		bool IsOpen { get; }
		Vector3 Position { get; }
		IProvince Province { get; }

		void SetAsOpen(bool open);
		void SetPosition(Vector3 pos);
	}
}