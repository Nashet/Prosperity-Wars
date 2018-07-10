using UnityEngine;

namespace Nashet.UnityUIUtils
{
    public class HideStandAlone : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
#if !UNITY_STANDALONE
            gameObject.SetActive(false);
#endif
        }
    }
}