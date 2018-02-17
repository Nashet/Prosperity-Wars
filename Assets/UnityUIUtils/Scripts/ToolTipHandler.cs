using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
namespace Nashet.UnityUIUtils
{
    public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Func<string> dynamicText;

        /// <summary>Has to be public to allow direct write in Condition.isAllTRue()</summary>
        [TextArea]
        public string text;

        [SerializeField]
        private bool isDynamic;

        [SerializeField]
        private Hideable ownerWindow;

        //private TooltipBase tooltipHolder;
        private bool inside;

        /// <summary>
        /// Need that to use in descendant
        /// </summary>
        protected void Start()
        {
            if (ownerWindow != null)
                ownerWindow.Hidden += OnHiddenOwner;
        }
        
        void OnHiddenOwner(Hideable eventData)
        {
            // forces tooltip to hide
            OnPointerExit(null);
        }

        public void SetTextDynamic(Func<string> dynamicString)
        {
            this.dynamicText = dynamicString;
            isDynamic = true;
        }
        public void SetText(string data)
        {
            text = data;
            isDynamic = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
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
        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipBase.get() != null)
                TooltipBase.get().HideTooltip();
            inside = false;
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
    }
}