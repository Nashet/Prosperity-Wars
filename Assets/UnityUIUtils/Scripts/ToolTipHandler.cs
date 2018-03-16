using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using Nashet.Utils;
using System.Linq;

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
            //var foundParent = gameObject.AllParents().FirstOrDefault(x => x.HasComponent<Hideable>());
            //if (foundParent != null)
            //{
            //    var ownerWindow = foundParent.GetComponent<Hideable>();
            var ownerWindow = GetComponentInParent<Hideable>();
            if (ownerWindow != null)
                ownerWindow.Hidden += OnHiddenOwner;
            //}
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