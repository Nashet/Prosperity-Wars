using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

namespace Nashet.UnityUIUtils
{
    public class Message
    {
        static private readonly Stack<Message> Queue = new Stack<Message>();
        private readonly string caption, text, closeText;
        static public bool HasUnshownMessages()
        {
            return Queue.Count > 0;
        }
        static public Message PopAndDeleteMessage()
        {
            return Queue.Pop();
        }

        public Message(string caption, string message, string closeText)
        {
            this.caption = caption;
            this.text = message;
            this.closeText = closeText;
            Queue.Push(this);
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
    public class MessagePanel : DragPanel
    {
        ///<summary>Stores position of top-level message window. Used to correctly place next message window</summary>
        static Vector3 lastDragPosition;

        [SerializeField]
        private Text caption, message, closeText;

        [SerializeField]
        private static GameObject messagePanelPrefab; //FixedJoint it

        private static int howMuchPausedWindowsOpen = 0;
        private StringBuilder sb = new StringBuilder();
        // Use this for initialization
        void Start()
        {
            Vector3 position = Vector3.zero;
            position.Set(lastDragPosition.x - 10f, lastDragPosition.y - 10f, 0);
            transform.localPosition = position;
            lastDragPosition = transform.localPosition;

            //var image = GetComponent<Image>();
            //image.color = GUIChanger.BackgroundColor;
            //image.material = null;
            //image = GetComponentInChildren<Image>();
            //image.color = GUIChanger.ButtonsColor;
            GUIChanger.Apply(this.gameObject);
        }

        override public void OnDrag(PointerEventData data) // need it to place windows in stair-order
        {
            base.OnDrag(data);
            lastDragPosition = transform.localPosition;
        }

        public override void Refresh()
        {
            //
        }

        public void show(Message mess)
        {
            howMuchPausedWindowsOpen++;
            caption.text = mess.GetCaption();
            message.text = mess.GetText();
            closeText.text = mess.GetClosetext();
            Show();            
        }
        static public void showMessageBox(Canvas canvas)
        {
            if (messagePanelPrefab == null)
                messagePanelPrefab = Resources.Load("Prefabs\\MessagePanel", typeof(GameObject)) as GameObject;
            Message mes = Message.PopAndDeleteMessage();
            GameObject newObject = (GameObject)GameObject.Instantiate(messagePanelPrefab);
            newObject.transform.SetParent(canvas.transform, true);

            MessagePanel mesPanel = newObject.GetComponent<MessagePanel>();
            mesPanel.Awake();
            mesPanel.show(mes);
        }
        override public void Hide()
        {
            base.Hide();
            howMuchPausedWindowsOpen--;
            Destroy(gameObject);
        }

        internal static bool IsOpenAny()
        {
            return howMuchPausedWindowsOpen > 0;
        }
    }
}
