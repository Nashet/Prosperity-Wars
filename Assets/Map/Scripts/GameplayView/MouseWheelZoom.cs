using Nashet.Map.GameplayControllers;
using UnityEngine;

namespace Nashet.Map.GameplayView
{
	public class MouseWheelZoom : MonoBehaviour
	{
		[SerializeField] CameraController controller;

		private void Update()
		{
			var wheel = Input.GetAxis("Mouse ScrollWheel");

			if (wheel != 0f)
			{
				controller.Zoom(wheel);
			}
		}
	}
}