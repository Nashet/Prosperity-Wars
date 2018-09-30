using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.Utils
{
    public class ColorSelector : ISelector
    {
        protected Color selectionColor;//, defaultColor;
        protected Image image;

        public ColorSelector(Color color)
        {
            selectionColor = color;
        }
        public void Deselect(GameObject someObject)
        {
            if (image == null)
                image=someObject.GetComponent<Image>();
            //image.color = defaultColor;
            image.color = GUIChanger.ButtonsColor;
        }

        public void Select(GameObject someObject)
        {
            if (image == null)
                image = someObject.GetComponent<Image>();
            //defaultColor = image.color;
            image.color = selectionColor;
        }
    }
}
