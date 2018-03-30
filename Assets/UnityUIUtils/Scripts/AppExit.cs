using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    public class AppExit : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_WEBGL
        Screen.fullScreen = false;
#else
            Application.Quit();
#endif
        }
    }
}