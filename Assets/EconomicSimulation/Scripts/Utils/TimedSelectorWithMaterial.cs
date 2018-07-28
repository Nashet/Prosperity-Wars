using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nashet.Utils
{
    /// <summary>
    /// As component it gives ability to select & deselect some GameObject with additional material
    /// </summary>
    public class TimedSelectorWithMaterial : UISelector
    {
        [Tooltip("In seconds, zero mean no time limit")]
        [SerializeField] protected float selectionTime;

        /// <summary>
        /// Is forbidden since it's MonoBehaviour
        /// </summary>        
        protected TimedSelectorWithMaterial() : base()
        {

        }

        public override void Deselect(GameObject someObject)
        {
            if (someObject != null)
            {
                var renderer = someObject.GetComponent<MeshRenderer>();
                if (renderer == null)
                //if there is no render in selected object, find one in childes
                {
                    var children = someObject.GetComponentsInChildren<MeshRenderer>();
                    foreach (var item in children)
                    {
                        RemoveMaterial(item);
                    }
                }
                else
                {
                    RemoveMaterial(renderer);
                }
            }
        }

        public override void Select(GameObject someObject)
        {
            if (someObject != null)
            {
                var renderer = someObject.GetComponent<MeshRenderer>();
                if (renderer == null)
                //if there is no render in selected object, find one in childes
                {
                    var children = someObject.GetComponentsInChildren<MeshRenderer>();
                    foreach (var item in children)
                    {
                        AddMaterial(item);
                    }
                }
                else
                {
                    AddMaterial(renderer);
                }
                if (selectionTime != 0f)
                    StartCoroutine(DelayedDeselection(someObject));
            }
        }


        protected void RemoveMaterial(MeshRenderer renderer)
        {
            Material[] newArray = new Material[1];
            newArray[0] = renderer.material;
            renderer.materials = newArray;
        }
        protected void AddMaterial(MeshRenderer renderer)
        {
            Material[] rt = new Material[2];
            rt[0] = renderer.material;
            rt[1] = selectionMaterial;
            renderer.materials = rt;
        }

        /// <summary>
        /// Use this instead
        /// </summary>        
        new public static TimedSelectorWithMaterial AddTo(GameObject toWhom, Material selectionMaterial, float selectionTime)
        {
            var added = toWhom.AddComponent<TimedSelectorWithMaterial>();
            added.selectionMaterial = selectionMaterial;
            added.selectionTime = selectionTime;
            return added;
        }
        protected IEnumerator DelayedDeselection(GameObject someObject)
        {
            yield return new WaitForSeconds(selectionTime);
            Deselect(someObject);
        }
    }
}
