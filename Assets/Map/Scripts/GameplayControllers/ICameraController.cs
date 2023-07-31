using Nashet.MapMeshes;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public interface ICameraController
	{
		event OnZoomHappened ZoomHappened;
		event OnCameraMoved CameraMoved;
		event OnFocusOnPoint FocusOnPointHappened;
		event OnInitialized Initialized;

		void FocusOnPoint(Vector3 point);
		void FocusOnProvince(ProvinceMesh province, bool selectProvince);
		void Initialize(Rect mapBorders);
		void Move(float x, float y);
		void Zoom(float zMove);
	}
}