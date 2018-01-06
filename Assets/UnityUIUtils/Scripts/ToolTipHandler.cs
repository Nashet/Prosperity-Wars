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

        //[SerializeField]
        public string text;
        //public string Text
        //{
        //    get { return m_text; }
        //    set { m_text = value; }
        //}
        [SerializeField]
        private bool isDynamic;

        // now auto update all dynamic tooltips
        //[SerializeField]
        //private bool updateEachTick;

        protected TooltipBase tooltipHolder;
        private bool inside;

        protected void Start()
        {
            tooltipHolder = TooltipBase.get();
        }
        public void setDynamicString(Func<string> dynamicString)
        {
            this.dynamicText = dynamicString;
            isDynamic = true;
        }
        public void setText(string data)
        {
            text = data;
            isDynamic = false;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (text != "" || dynamicText != null)
            {
                if (dynamicText == null)
                    tooltipHolder.SetTooltip(text);
                else
                    tooltipHolder.SetTooltip(dynamicText());
                inside = true;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipHolder != null)
                tooltipHolder.HideTooltip();
            inside = false;
        }
        public bool isInside()
        {
            return inside;
        }
        private void Update()
        {
            if (isInside() && isDynamic)
                OnPointerEnter(null);
        }

        internal string getText()
        {
            return text;
        }
    }
}