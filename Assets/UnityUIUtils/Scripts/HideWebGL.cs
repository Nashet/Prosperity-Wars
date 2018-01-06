using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nashet.UnityUIUtils
{
    public class HideWebGL : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
#if UNITY_WEBGL
        gameObject.SetActive(false);
#endif
        }
    }
}