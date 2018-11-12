using System.Collections.Generic;
using UnityEngine;

namespace Nashet.UnityUIUtils
{
    public class Message
    {
        private static readonly Stack<Message> Queue = new Stack<Message>();
        private static bool showDefeatingAttackersMessages = true;

        private readonly string caption, text, closeText;
        private Vector2 focus;
        private bool hasFocus;

        public bool HasFocus
        {
            get { return hasFocus; }
        }

        public static bool ShowDefeatingAttackersMessages
        {
            get { return showDefeatingAttackersMessages; }
        }

        public static void NewMessage(string caption, string message, string closeText, bool isDefeatingAttackersMessage, Vector2 focus)
        {
            if (!isDefeatingAttackersMessage || showDefeatingAttackersMessages)
                new Message(caption, message, closeText, focus);
        }

        public static void NewMessage(string caption, string message, string closeText, bool isDefeatingAttackersMessage)
        {
            if (!isDefeatingAttackersMessage || showDefeatingAttackersMessages)
                new Message(caption, message, closeText);
        }

        protected Message(string caption, string message, string closeText)
        {
            this.caption = caption;
            text = message;
            this.closeText = closeText;
            Queue.Push(this);
        }

        protected Message(string caption, string message, string closeText, Vector2 focus) : this(caption, message, closeText)
        {
            hasFocus = true;
            this.focus = focus;
        }

        public static bool HasUnshownMessages()
        {
            return Queue.Count > 0;
        }

        public static Message PopAndDeleteMessage()
        {
            return Queue.Pop();
        }

        public static void SetShowDefeatingAttackersMessages(bool value)
        {
            showDefeatingAttackersMessages = value;
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