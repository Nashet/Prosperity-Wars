using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    public class DragButton : MonoBehaviour, IPointerDownHandler
    {
        private DragPanel parent;

        public void OnPointerDown(PointerEventData data)
        {
            parent.OnPointerDown(data);
        }

        // Use this for initialization
        private void Start()
        {
            parent = GetComponentInParent<DragPanel>();
        }
    }
}