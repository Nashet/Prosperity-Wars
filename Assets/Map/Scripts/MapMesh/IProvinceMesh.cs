using UnityEngine;

namespace Nashet.MapMeshes
{
	public interface IProvinceMesh
	{
		GameObject GameObject { get; }
		int ID { get; }
		Vector3 Position { get; }

		void SetBorderMaterial(int id, Material material);
		void SetColor(Color color);
	}
}