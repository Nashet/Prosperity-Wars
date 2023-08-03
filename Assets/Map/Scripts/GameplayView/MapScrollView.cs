using Nashet.Map.GameplayControllers;
using UnityEngine;

namespace Nashet.Map.GameplayView
{
	public class MapScrollView : MonoBehaviour
	{
		[SerializeField] CameraController controller;

		private void Update()
		{
			float verticalInput = Input.GetAxis("Vertical");
			float horizontalInput = Input.GetAxis("Horizontal");
			if (verticalInput != 0f || horizontalInput != 0f)
			{
				controller.Move(horizontalInput, verticalInput);
			}
		}
	}
}