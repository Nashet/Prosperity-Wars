using Nashet.EconomicSimulation;
using UnityEngine;

namespace Nashet.UnityUIUtils
{
    internal class MessageSystem : MonoBehaviour
    {
        [SerializeField] protected GameObject messagePanelPrefab;
        private void Update()
        {
            // instantiate
            int counter = 0;
            while (Message.HasUnshownMessages())
            {
                var window = Instantiate(messagePanelPrefab, LinksManager.Get.CameraLayerCanvas.transform).GetComponent<MessagePanel>();
                window.Show(Message.PopAndDeleteMessage(), MainCamera.Get, counter);
                counter++;
            }

        }
    }
}