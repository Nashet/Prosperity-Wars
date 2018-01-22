using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Used as cell in UITableNew
    /// </summary>
    public class SimpleCell : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Button buttonComponent;

        [SerializeField]
        private Text nameLabel;

        [SerializeField]
        private Image iconImage;
        private IClickable objectToClick;
        private DragPanel parent;


        // Use this for initialization
        void Start()
        {
            buttonComponent.onClick.AddListener(HandleClick);
        }
        
        public void Setup(string text, IClickable link)
        {
            this.objectToClick = link;
            nameLabel.text = text;
            parent = GetComponentInParent<DragPanel>();
        }
        public void OnPointerDown(PointerEventData data)
        {
            parent.OnPointerDown(data);
        }
        private void HandleClick()
        {
            if (objectToClick == null)
                return;
            else
                objectToClick.OnClicked();            
        }        
    }
}