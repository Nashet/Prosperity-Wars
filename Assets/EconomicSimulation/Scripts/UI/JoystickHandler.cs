using Nashet.EconomicSimulation;
using UnityEngine;

namespace Nashet.GameplayControllers
{
    public class JoystickHandler : MonoBehaviour
    {
        [SerializeField] private float mapDragSpeed = 0.05f;
		[SerializeField] private CameraController cameraController;

		private void Update()
        {
            HandleMapScroll();
        }

        private void HandleMapScroll()
        {
            var joy = LinksManager.Get.scrolJoystic;
			cameraController.Move(joy.Horizontal * mapDragSpeed, joy.Vertical * mapDragSpeed);

            return;
        }
    }
}
