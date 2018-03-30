using UnityEngine;

namespace Nashet.UnityUIUtils
{
    public class HideWebGL : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
#if UNITY_WEBGL
        gameObject.SetActive(false);
#endif
        }
    }
}