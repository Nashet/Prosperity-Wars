using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnityUIUtils
{
    public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IHideable
    {
        [SerializeField]
        private Func<string> dynamicText;

        /// <summary>Has to be public to allow direct write in Condition.isAllTRue()</summary>
        [TextArea]
        public string text;

        [SerializeField]
        private bool isDynamic;

        //[SerializeField]
        //private Hideable ownerWindow;

        private bool inside;

        /// <summary>
        /// Need that to use in descendant
        /// </summary>
        protected void Start()
        {            
            var ownerWindow = GetComponentInParent<Hideable>();
            if (ownerWindow != null)
                ownerWindow.Hidden += OnHiddenOwner;         
        }

        private void OnHiddenOwner(Hideable eventData)
        {
            // forces tooltip to hide
            OnPointerExit(null);
        }

        public void SetTextDynamic(Func<string> dynamicString)
        {
            dynamicText = dynamicString;
            isDynamic = true;
        }

        public void SetText(string data)
        {
            text = data;
            isDynamic = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Hide();
        }

        public bool IsInside()
        {
            return inside;
        }

        private void Update()
        {
            if (IsInside() && isDynamic)
                OnPointerEnter(null); // forces tooltip to update
        }

        internal string GetText()
        {
            return text;
        }

        internal void AddText(string add)
        {
            text += add;
        }

        public void Hide()
        {
            if (TooltipBase.get() != null)
                TooltipBase.get().HideTooltip();
            inside = false;
        }

        public void Show()
        {
            if (text != "" || dynamicText != null)
            {
                if (dynamicText == null)
                    TooltipBase.get().SetTooltip(text);
                else
                    TooltipBase.get().SetTooltip(dynamicText());
                inside = true;
            }
        }
    }
}