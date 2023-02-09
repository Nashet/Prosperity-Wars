using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class JoystickHandler : MonoBehaviour
    {
        [SerializeField] private float mapDragSpeed = 0.05f;

        private void Update()
        {
            HandleMapScroll();
        }

        private void HandleMapScroll()
        {
            var joy = LinksManager.Get.scrolJoystic;
            MainCamera.Get.Move(joy.Horizontal * mapDragSpeed, joy.Vertical * mapDragSpeed);

            return;
        }
    }
}
