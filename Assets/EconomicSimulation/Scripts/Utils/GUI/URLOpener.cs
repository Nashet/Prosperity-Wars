using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Allows opening links in new tabs with WebGL
/// </summary>
public class URLOpener : MonoBehaviour
{    
    public void OpenLinkJSPlugin(string url)
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
