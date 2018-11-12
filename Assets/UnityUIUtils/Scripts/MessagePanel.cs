using Nashet.EconomicSimulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{

    public class MessagePanel : DragPanel
    {
        ///<summary>Stores position of top-level message window. Used to correctly place next message window</summary>
        private static Vector3 lastDragPosition;

        [SerializeField]
        private Text caption, message, closeText;

        [SerializeField]
        private Toggle showDefeatingAttackerMessage;

        [SerializeField]
        private static GameObject messagePanelPrefab; //FixedJoint it

        private MainCamera mainCamera;

        private static int howMuchPausedWindowsOpen;
        private Message messageSource;

        public static void showMessageBox(Canvas canvas, MainCamera mainCamera)
        {
            if (messagePanelPrefab == null)
                messagePanelPrefab = Resources.Load("Prefabs\\MessagePanel", typeof(GameObject)) as GameObject;
            Message message = Message.PopAndDeleteMessage();
            GameObject newObject = Instantiate(messagePanelPrefab, canvas.transform);
            //newObject.transform.SetParent(canvas.transform, true);

            MessagePanel mesPanel = newObject.GetComponent<MessagePanel>();
            mesPanel.Awake();
            mesPanel.show(message, mainCamera);
        }

        // Use this for initialization
        private void Start()
        {
            Vector3 position = Vector3.zero;
            position.Set(lastDragPosition.x - 10f, lastDragPosition.y - 10f, 0);
            transform.localPosition = position;
            lastDragPosition = transform.localPosition;
            GUIChanger.Apply(gameObject);
            showDefeatingAttackerMessage.isOn = Message.ShowDefeatingAttackersMessages;
            var rect = GetComponent<RectTransform>();
            rect.transform.position = new Vector3((Screen.width - rect.sizeDelta.x) / 2, (Screen.height - rect.sizeDelta.y) / 2, rect.position.z);
        }

        public override void OnDrag(PointerEventData data) // need it to place windows in stair-order
        {
            base.OnDrag(data);
            lastDragPosition = transform.localPosition;
        }

        public override void Refresh()
        {
            //
        }

        public void OnShowMessagesChanged(bool value)
        {
            Message.SetShowDefeatingAttackersMessages(value);
        }

        public void OnFocusClicked()
        {
            if (messageSource.HasFocus)
                mainCamera.FocusOnPoint(messageSource.getFocus());
        }

        private void show(Message mess, MainCamera mainCamera)
        {
            this.mainCamera = mainCamera;
            howMuchPausedWindowsOpen++;
            caption.text = mess.GetCaption();
            message.text = mess.GetText();
            closeText.text = mess.GetClosetext();
            messageSource = mess;
            Show();
        }

        public override void Hide()
        {
            base.Hide();
            howMuchPausedWindowsOpen--;
            Destroy(gameObject);
        }

        public static bool IsOpenAny()
        {
            return howMuchPausedWindowsOpen > 0;
        }
    }
}