using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nashet.UnityUIUtils
{
    public class HideStandAlone : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
#if !UNITY_STANDALONE
            gameObject.SetActive(false);
#endif
        }
       
    }
}