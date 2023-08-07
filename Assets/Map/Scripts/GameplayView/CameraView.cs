using Nashet.Map.GameplayControllers;
using UnityEngine;

namespace Nashet.Map.GameplayView
{
	public class CameraView : MonoBehaviour
	{
		[SerializeField] private float xzCameraSpeed = 2f;

		[SerializeField] private float yCameraSpeed = 55f;

		[SerializeField] private Rect mapBorders;

		[SerializeField] private bool allowed;
		[SerializeField] private CameraController cameraController;
		[SerializeField] private float minimalHeight = -40;
		[SerializeField] private float maxHeight = -500;
		[SerializeField] private float focusHeight;
		

		private void Awake()
		{
			focusHeight = transform.position.z;
			cameraController.CameraMoved += Move;
			cameraController.ZoomHappened += Zoom;
			cameraController.Initialized += Set;
			cameraController.FocusOnPointHappened += FocusOnPoint;
		}

		private void Set(Rect mapBorders)
		{
			this.mapBorders = mapBorders;
			allowed = true;
		}

		private void Zoom(float zMove)
		{
			var position = transform.position;
			zMove = zMove * yCameraSpeed;
			if (position.z + zMove > minimalHeight
				|| position.z + zMove < maxHeight)
				zMove = 0f;
			transform.Translate(0f, 0f, zMove, Space.World);
		}

		private void Move(float xMove, float yMove)
		{
			if (!allowed)
				return; // map isnt done yet

			var position = transform.position;


			if (xMove * xzCameraSpeed + position.x < mapBorders.x
				|| xMove * xzCameraSpeed + position.x > mapBorders.width)
				xMove = 0;

			if (yMove * xzCameraSpeed + position.y < mapBorders.y
				|| yMove * xzCameraSpeed + position.y > mapBorders.height)
				yMove = 0;

			transform.Translate(xMove * xzCameraSpeed, yMove * xzCameraSpeed, 0f, Space.World);
		}

		private void FocusOnPoint(Vector3 point)
		{
			gameObject.transform.position = new Vector3(point.x, point.y, focusHeight);
		}
	}
}