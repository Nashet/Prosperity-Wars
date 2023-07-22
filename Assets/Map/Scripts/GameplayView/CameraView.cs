using UnityEngine;

namespace Nashet.Map.GameplayView
{
	public class CameraView : MonoBehaviour
	{
		[SerializeField]
		private float xzCameraSpeed = 2f;

		[SerializeField]
		private float yCameraSpeed = 55f;

		[SerializeField]
		private Rect mapBorders;

		[SerializeField]
		private bool allowed;
		private float focusHeight;

		private void Start()
		{
			focusHeight = transform.position.z;
		}

		public void Zoom(float zMove)
		{
			var position = transform.position;
			zMove = zMove * yCameraSpeed;
			if (position.z + zMove > -40f
				|| position.z + zMove < -500f)
				zMove = 0f;
			transform.Translate(0f, 0f, zMove, Space.World);
		}

		public void Set(Rect mapBorders)
		{
			this.mapBorders = mapBorders;
			allowed = true;
		}

		//public void FocusOnProvince(Province province, bool select)
		//{
		//	gameObject.transform.position = new Vector3(province.provinceMesh.Position.x, province.provinceMesh.Position.y, focusHeight);
		//	if (select)
		//		selectProvince(province.ID);
		//}

		public void Move(float xMove, float yMove)
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

		private void FixedUpdate()
		{
			if (!allowed)
				return; // map isnt done yet

			Zoom(Input.GetAxis("Mouse ScrollWheel"));
		}

		public void FocusOnPoint(Vector3 point)
		{
			gameObject.transform.position = new Vector3(point.x, point.y, focusHeight);
		}
	}
}