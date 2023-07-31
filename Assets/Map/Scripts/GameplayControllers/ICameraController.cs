using Nashet.MapMeshes;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public interface ICameraController
	{
		void FocusOnPoint(Vector3 point);
		void FocusOnProvince(ProvinceMesh province, bool selectProvince);
		void Move(float x, float y);
		void Zoom(float zMove);
	}
}