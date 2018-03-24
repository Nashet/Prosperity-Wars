using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Nashet.UnityUIUtils
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(ToolTipHandler))]
    public class HotKey : MonoBehaviour
    {        
        [SerializeField]
        private KeyCode key;
        [SerializeField]
        private bool addTextInTooltip = true;
        
        private Button button;
        //private ToolTipHandler tooltip;
        void Start()
        {
            button = GetComponent<Button>();
            if (addTextInTooltip)
            {
                var tooltip = GetComponent<ToolTipHandler>();
                tooltip.AddText("\nHotkey is " + key + " button");
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(key) && button.interactable)
                button.onClick.Invoke();
        }
    }
}