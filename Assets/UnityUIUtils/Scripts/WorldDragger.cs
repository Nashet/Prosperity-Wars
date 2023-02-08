using Nashet.EconomicSimulation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    public class WorldDragger : MonoBehaviour, IDragHandler
    {
        [SerializeField] private float scrollSpeed = 10f;

        private RectTransform canvasRectTransform;
        private Vector2 oldPosition;

        private void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            canvasRectTransform = canvas.transform as RectTransform;
            // panelRectTransform = transform as RectTransform;           
        }

        public void OnDrag(PointerEventData data)
        {
            HandleMapScroll(data);
        }

        private void HandleMapScroll(PointerEventData data)
        {
            var cameraScript = Camera.main.GetComponent<MainCamera>();
            Debug.LogError(data.position);
            var change = oldPosition - data.position;
            var movement = new Vector3(change.x * scrollSpeed, 0, change.y * scrollSpeed);
            cameraScript.Move(movement);
            oldPosition = data.position;
        }
    }
}