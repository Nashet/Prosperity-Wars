using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices;

namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Allows opening URL from webGL
    /// </summary>
    public class PressHandler : MonoBehaviour, IPointerDownHandler
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
            openWindow(url);
#else
            Application.OpenURL(url);
#endif
        }

        [DllImport("__Internal")]
        private static extern void openWindow(string url);

    }
}