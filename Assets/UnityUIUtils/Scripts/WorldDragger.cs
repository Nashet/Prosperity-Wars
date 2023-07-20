using Nashet.GameplayController;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    public class WorldDragger : MonoBehaviour, IDragHandler
    {
        [SerializeField] private float scrollSpeed = 10f;
		[SerializeField] private CameraController cameraController;

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
            var change = oldPosition - data.position;
			cameraController.Move(change.x * scrollSpeed, change.y * scrollSpeed);
            oldPosition = data.position;
        }
    }
}