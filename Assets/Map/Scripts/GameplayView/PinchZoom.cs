using Nashet.Map.GameplayControllers;
using UnityEngine;

namespace Nashet.Map.GameplayView
{
	public class PinchZoom : MonoBehaviour
	{
		[SerializeField] private float zoomSpeed = 0.01f;
		[SerializeField] private CameraController cameraController;
		private Vector2 prevDist = new Vector2(0, 0);
		private Vector2 curDist = new Vector2(0, 0);

		private void Update()
		{
			CheckForMultiTouch();
		}

		private void CheckForMultiTouch()
		{
			// These lines of code will take the distance between two touches and zoom in - zoom out at middle point between them
			if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
			{
				curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
				prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
				float touchDelta = curDist.magnitude - prevDist.magnitude;

				// Zoom out
				if (touchDelta > 0)
				{
					cameraController.Zoom(zoomSpeed * -1f);
				}

				//Zoom in
				else if (touchDelta < 0)
				{
					cameraController.Zoom(zoomSpeed);
				}
			}
		}
	}
}