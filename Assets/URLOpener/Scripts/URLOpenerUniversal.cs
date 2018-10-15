using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nashet.URLOpener
{
    /// <summary>
    /// Allows opening URL from webGL
    /// </summary>
    public class URLOpenerUniversal : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private string url;

        private readonly UnityEvent OnPress = new UnityEvent();

        private void Start()
        {
            OnPress.AddListener(() => { OpenLinkJSPlugin(url); });
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPress.Invoke();
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