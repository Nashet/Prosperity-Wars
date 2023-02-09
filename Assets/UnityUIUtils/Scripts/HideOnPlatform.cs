using UnityEngine;

namespace Nashet.UnityUIUtils
{
    public class HideOnPlatform : MonoBehaviour
    {
        [SerializeField] private RuntimePlatform platform = RuntimePlatform.WebGLPlayer;
        [SerializeField] private MonoBehaviour component;
        [SerializeField] private bool destroyObject;

        private void Awake()
        {
            if (Application.platform == platform)
            {
                if (component!= null && !destroyObject)
                    component.enabled = false;
                if (destroyObject)
                    gameObject.SetActive(false);
            }
        }
    }
}