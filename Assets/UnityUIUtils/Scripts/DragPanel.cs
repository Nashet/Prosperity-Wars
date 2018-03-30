﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    public interface IRefreshable
    {
        void Refresh();
    }

    public interface IHideable
    {
        void Hide();

        void Show();
    }

    public abstract class Hideable : MonoBehaviour, IHideable
    {
        // declare delegate (type)
        public delegate void HideEventHandler(Hideable eventData);

        //declare event of type delegate
        public event HideEventHandler Hidden;

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            var @event = Hidden;
            if (@event != null)// check for subscribers
                @event(this); //fires event for all subscribers
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Represent UI object that can be refreshed and hidden
    /// </summary>
    public abstract class Window : Hideable, IRefreshable
    {
        public abstract void Refresh();

        public override void Show()
        {
            base.Show();
            Refresh();
        }
    }

    /// <summary>
    /// Represents movable and hideable window
    /// </summary>
    public abstract class DragPanel : Window, IPointerDownHandler, IDragHandler
    {
        private Vector2 pointerOffset;
        private RectTransform canvasRectTransform;
        private RectTransform panelRectTransform;

        public void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRectTransform = canvas.transform as RectTransform;
                panelRectTransform = transform as RectTransform;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            panelRectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out pointerOffset);
        }

        public virtual void OnDrag(PointerEventData data)
        {
            if (panelRectTransform == null)
                return;

            //Vector2 pointerPostion = ClampToWindow(data);
            //Vector2 ert;
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                //canvasRectTransform, pointerPostion, data.pressEventCamera, out localPointerPosition
                canvasRectTransform, data.position, data.pressEventCamera, out localPointerPosition
            ))
            {
                //ert = localPointerPosition - pointerOffset;
                //panelRectTransform.localPosition = ert;
                GetComponent<RectTransform>().localPosition = localPointerPosition - pointerOffset;
                //GetComponent<RectTransform>().localPosition
            }
        }

        private Vector2 ClampToWindow(PointerEventData data)
        {
            Vector2 rawPointerPosition = data.position;

            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);

            float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

            Vector2 newPointerPosition = new Vector2(clampedX, clampedY);
            return newPointerPosition;
        }

        public override void Hide()
        {
            panelRectTransform.SetAsFirstSibling();
            base.Hide();
        }

        public override void Show()
        {
            base.Show();
            panelRectTransform.SetAsLastSibling();
        }
    }
}