using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nashet.UnityUIUtils
{
    public class AppExit : MonoBehaviour
    {

        public void onExitClick()
        {
#if UNITY_WEBGL
        Screen.fullScreen = false;
#else
            Application.Quit();
#endif
        }
    }
}
