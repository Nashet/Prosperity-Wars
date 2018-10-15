using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nashet.URLOpener
{
    /// <summary>
    /// Allows opening URL
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class URLOpenerButton : MonoBehaviour
    {
        [SerializeField]
        private string url;

        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() => { OpenLinkJSPlugin(url); });
        }
        private static void OpenLinkJSPlugin(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OpenWindow(url);
#else
            Application.OpenURL(url);
#endif
        }

        [DllImport("__Internal")]
        private static extern void OpenWindow(string url);
    }
}