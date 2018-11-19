using Nashet.EconomicSimulation;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Handles message panels
    /// </summary>
    internal class MessageSystem : MonoBehaviour
    {
        [SerializeField] protected GameObject messagePanelPrefab;

        internal static MessageSystem Instance { get; private set; }

        protected readonly Stack<Message> Queue = new Stack<Message>();
        protected bool showDefeatingAttackersMessages = true;

        protected void Start()
        {
            //singleton pattern
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.Log(this + " singleton  already created. Exterminating..");
                Destroy(this);
            }

        }

        public bool HasUnshownMessages()
        {
            return Queue.Count > 0;
        }

        public Message PopAndDeleteMessage()
        {
            return Queue.Pop();
        }

        public void SetShowDefeatingAttackersMessages(bool value)
        {
            showDefeatingAttackersMessages = value;
        }

        public bool ShowDefeatingAttackersMessages
        {
            get { return showDefeatingAttackersMessages; }
        }

        public void NewMessage(string caption, string message, string closeText, bool isDefeatingAttackersMessage, Vector2 focus)
        {
            if (!isDefeatingAttackersMessage || showDefeatingAttackersMessages)
                Queue.Push(new Message(caption, message, closeText, focus));
        }

        public void NewMessage(string caption, string message, string closeText, bool isDefeatingAttackersMessage)
        {
            if (!isDefeatingAttackersMessage || showDefeatingAttackersMessages)
                Queue.Push(new Message(caption, message, closeText));
        }

        protected void Update()
        {
            // instantiate
            int counter = 0;
            while (HasUnshownMessages())
            {
                var window = Instantiate(messagePanelPrefab, LinksManager.Get.CameraLayerCanvas.transform).GetComponent<MessagePanel>();
                window.Show(PopAndDeleteMessage(), MainCamera.Get, counter);
                counter++;
            }

        }
    }
}