using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.Utils
{
    /// <summary>
    /// As component it gives ability to select & deselect some UI element with a selectionMaterial
    /// </summary>
    public class UISelector : MonoBehaviour, ISelector
    {
        [Header("Gives ability to select & deselect some Object with material")]
        [SerializeField] protected Material selectionMaterial;
        [SerializeField] protected Material defaultMaterial;

        /// <summary>
        /// Is forbidden since it's MonoBehaviour
        /// </summary>        
        protected UISelector() : base()
        {

        }

        /// <summary>
        /// Use this instead
        /// </summary>        
        public static UISelector AddTo(GameObject toWhom, Material selectionMaterial, Material defaultMaterial)
        {
            var added = toWhom.AddComponent<UISelector>();
            added.selectionMaterial = selectionMaterial;
            added.defaultMaterial = defaultMaterial;
            return added;
        }


        public virtual void Deselect(GameObject someObject)
        {
            var image = someObject.GetComponent<Image>();
            if (image == null)
            //if there is no render in selected object, find one in childes
            {
                var children = someObject.GetComponentsInChildren<Image>();
                foreach (var item in children)
                {
                    item.material = defaultMaterial;
                }
            }
            else
            {
                image.material = defaultMaterial;
            }
        }

        public virtual void Select(GameObject someObject)
        {
            var image = someObject.GetComponent<Image>();
            if (image == null)
            //if there is no render in selected object, find one in childes
            {
                var children = someObject.GetComponentsInChildren<Image>();
                foreach (var item in children)
                {
                    item.material = selectionMaterial;
                }
            }
            else
            {
                image.material = selectionMaterial;
            }
        }
    }
}
