using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.UnityUIUtils
{
    // A very simple object pooling class
    public class SimpleObjectPool : MonoBehaviour
    {
        ///<summary>The prefab that this object pool returns instances of</summary>
        [SerializeField] // Don't make it static since there could be > 1 pools
        private GameObject pooledPrefab;

        // collection of currently inactive instances of the prefab
        private readonly Stack<GameObject> inactiveInstances = new Stack<GameObject>();

        //public GameObject GetObject(GameObject prefabAnother)
        //{
        //    GameObject spawnedGameObject;

        //    // if there is an inactive instance of the prefab ready to return, return that
        //    if (inactiveInstances.Count > 0)
        //    {
        //        // remove the instance from teh collection of inactive instances
        //        spawnedGameObject = inactiveInstances.Pop();
        //    }
        //    // otherwise, create a new instance
        //    else
        //    {
        //        spawnedGameObject = (GameObject)GameObject.Instantiate(prefabAnother);

        //        // add the PooledObject component to the prefab so we know it came from this pool
        //        PooledObject pooledObject = spawnedGameObject.AddComponent<PooledObject>();
        //        pooledObject.pool = this;
        //    }

        //    // put the instance in the root of the scene and enable it
        //    spawnedGameObject.transform.SetParent(null);
        //    spawnedGameObject.SetActive(true);

        //    // return a reference to the instance
        //    return spawnedGameObject;
        //}
        // Returns an instance of the prefab
        public GameObject GetObject()
        {
            GameObject spawnedGameObject;

            // if there is an inactive instance of the prefab ready to return, return that
            if (inactiveInstances.Count > 0)
            {
                // remove the instance from teh collection of inactive instances
                spawnedGameObject = inactiveInstances.Pop();
            }
            // otherwise, create a new instance
            else
            {
                spawnedGameObject = Instantiate(pooledPrefab);

                // add the PooledObject component to the prefab so we know it came from this pool
                PooledObject pooledObject = spawnedGameObject.AddComponent<PooledObject>();
                pooledObject.pool = this;
            }

            // put the instance in the root of the scene and enable it
            spawnedGameObject.transform.SetParent(null);
            spawnedGameObject.SetActive(true);

            // return a reference to the instance
            return spawnedGameObject;
        }

        // Return an instance of the prefab to the pool
        public void ReturnObject(GameObject toReturn)
        {
            PooledObject pooledObject = toReturn.GetComponent<PooledObject>();

            // if the instance came from this pool, return it to the pool
            if (pooledObject != null && pooledObject.pool == this)
            {
                ToolTipHandler s = toReturn.GetComponent<ToolTipHandler>();
                // s.GetComponent<ToolTipHandler>().tooltip = "";
                s.SetText("");
                s.SetTextDynamic(null);
                //if (s.tip != null)
                //    s.tip.HideTooltip();
                //s.tip = null;
                toReturn.GetComponentInChildren<Text>().text = "notext";

                // make the instance a child of this and disable it
                toReturn.transform.SetParent(transform);
                toReturn.SetActive(false);

                // add the instance to the collection of inactive instances
                inactiveInstances.Push(toReturn);
            }
            // otherwise, just destroy it
            else
            {
                Debug.LogWarning(toReturn.name + " was returned to a pool it wasn't spawned from! Destroying.");
                Destroy(toReturn);
            }
        }

        // a component that simply identifies the pool that a GameObject came from
        public class PooledObject : MonoBehaviour
        {
            public SimpleObjectPool pool;
        }
    }
}