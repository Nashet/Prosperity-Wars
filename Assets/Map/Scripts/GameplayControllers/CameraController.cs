using Nashet.Map.GameplayView;
using Nashet.MapMeshes;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public class CameraController : MonoBehaviour, ICameraController
	{
		[SerializeField] private CameraView cameraView;
		[SerializeField] private ProvinceSelectionController provinceSelectionController;

		public void Move(float x, float y)
		{
			cameraView.Move(x, y);
		}

		public void Zoom(float zMove)
		{
			cameraView.Zoom(zMove);
		}

		public void FocusOnPoint(Vector3 point) => cameraView.FocusOnPoint(point);
		public void FocusOnProvince(ProvinceMesh province, bool selectProvince)
		{
			FocusOnPoint(province.Position);
			if (selectProvince)
				provinceSelectionController.selectProvince(province.GameObject, province.ID);
		}
	}
}