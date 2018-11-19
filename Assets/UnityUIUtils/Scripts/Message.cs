using System.Collections.Generic;
using UnityEngine;

namespace Nashet.UnityUIUtils
{
    public class Message
    {      
        protected readonly string caption, text, closeText;
        protected Vector2 focus;
        protected bool hasFocus;

        public bool HasFocus
        {
            get { return hasFocus; }
        }        

        internal Message(string caption, string message, string closeText)
        {
            this.caption = caption;
            text = message;
            this.closeText = closeText;            
        }

        internal Message(string caption, string message, string closeText, Vector2 focus) : this(caption, message, closeText)
        {
            hasFocus = true;
            this.focus = focus;
        }

        /// <summary>
        /// Before use check if focus exists
        /// </summary>
        public Vector2 getFocus()
        {
            return focus;
        }

        public string GetCaption()
        {
            return caption;
        }

        public string GetText()
        {
            return text;
        }

        public string GetClosetext()
        {
            return closeText;
        }
    }
}