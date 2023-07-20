using Nashet.GameplayView;
using UnityEngine;

namespace Nashet.GameplayController
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private CameraView cameraView;

		internal void Move(float v1, float v2)
		{
			cameraView.Move(v1, v2);
		}

		internal void Zoom(float v)
		{
			cameraView.Zoom(v);
		}
	}
}