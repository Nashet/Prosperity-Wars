using Nashet.GameplayView;
using Nashet.MapMeshes;
using UnityEngine;

namespace Nashet.GameplayControllers
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private CameraView cameraView;
		[SerializeField] private ProvinceSelectionController provinceSelectionController;

		internal void Move(float v1, float v2)
		{
			cameraView.Move(v1, v2);
		}

		internal void Zoom(float v)
		{
			cameraView.Zoom(v);
		}

		internal void FocusOnPoint(Vector3 point) => cameraView.FocusOnPoint(point);
		public void FocusOnProvince(ProvinceMesh province, bool select)
		{
			FocusOnPoint(province.Position);
			if (select)
				provinceSelectionController.selectProvince(province.GameObject, province.ID);
		}
	}
}