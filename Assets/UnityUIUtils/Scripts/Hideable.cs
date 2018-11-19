using UnityEngine;

namespace Nashet.UnityUIUtils
{
    public class Hideable : MonoBehaviour, IHideable
    {
        // declare delegate (type)
        public delegate void HideEventHandler(Hideable eventData);

        //declare event of type delegate
        public event HideEventHandler Hidden;

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            var @event = Hidden;
            if (@event != null)// check for subscribers
                @event(this); //fires event for all subscribers
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
    }
}