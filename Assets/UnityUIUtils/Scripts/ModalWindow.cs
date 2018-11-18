using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Can be only 1 instance
    /// </summary>
    [RequireComponent(typeof(Hideable))]
    public class ModalWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected Text generalText;

        public static ModalWindow Instance { get; protected set; }
        protected bool isMouseInside;
        protected Animator animator;

        protected IHideable hideable;

        protected void Start()
        {
            //singleton pattern
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.Log(this + " singleton  already created. Exterminating..");
                Destroy(this);
            }
            hideable = GetComponent<IHideable>();            
            Hide();
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Overwrites previous modal window if it was open
        /// </summary>    
        internal void Show(string text)
        {
            generalText.text = text;
            Show();
        }

        protected void Show()
        {
            animator.Play("Opening");
            animator.SetBool("IsClosed", false);
            hideable.Show();
        }

        protected void Hide()
        {
            animator.SetBool("IsClosed", true);
        }

        // Update is called once per frame
        protected void Update()
        {
            // close if clicked outside
            if (Input.GetMouseButtonUp(0) && !isMouseInside)
                Hide();

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
            {
                hideable.Hide();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseInside = false;
        }        
    }
}