using Nashet.Map.GameplayView;
using Nashet.MapMeshes;
using UnityEngine;

namespace Nashet.Map.GameplayControllers
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private CameraView cameraView;
		[SerializeField] private ProvinceSelectionController provinceSelectionController;

		public void Move(float v1, float v2)
		{
			cameraView.Move(v1, v2);
		}

		public void Zoom(float v)
		{
			cameraView.Zoom(v);
		}

		public void FocusOnPoint(Vector3 point) => cameraView.FocusOnPoint(point);
		public void FocusOnProvince(ProvinceMesh province, bool select)
		{
			FocusOnPoint(province.Position);
			if (select)
				provinceSelectionController.selectProvince(province.GameObject, province.ID);
		}
	}
}