

using UnityEngine;
namespace Nashet.UnityUIUtils
{
    public class MonoSingleton: MonoBehaviour
    {        
        public static MonoSingleton Instance { get; protected set; }

        // don't use it for MonoBehaviour
        protected MonoSingleton()
        {
            
        }

        protected virtual void Start()
        {
            //singleton pattern
            if (Instance == null)
                Instance = this;
            else
            {                
                Debug.Log(this + " singleton  already created. Exterminating..");
                Destroy(this);
            }
        }
    }
}