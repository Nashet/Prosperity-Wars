using Nashet.MapMeshes;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public delegate void OnZoomHappened(float zMove);
	public delegate void OnCameraMoved(float x, float y);
	public delegate void OnFocusOnPoint(Vector3 point);
	public delegate void OnInitialized(Rect mapBorders);

	public class CameraController : MonoBehaviour, ICameraController
	{
		[SerializeField] private ProvinceSelectionController provinceSelectionController;

		public event OnZoomHappened ZoomHappened;
		public event OnCameraMoved CameraMoved;
		public event OnFocusOnPoint FocusOnPointHappened;
		public event OnInitialized Initialized;

		public void Move(float x, float y)
		{
			CameraMoved?.Invoke(x, y);
		}

		public void Zoom(float zMove)
		{
			ZoomHappened?.Invoke(zMove);
		}

		public void FocusOnPoint(Vector3 point) => FocusOnPointHappened?.Invoke(point);
		public void FocusOnProvince(ProvinceMesh province, bool selectProvince)
		{
			FocusOnPoint(province.Position);
			if (selectProvince)
				provinceSelectionController.selectProvince(province.GameObject, province.ID);
		}

		public void Initialize(Rect mapBorders)
		{
			Initialized?.Invoke(mapBorders);
		}
	}
}