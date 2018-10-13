
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Can be only 1 instance
    /// </summary>
    public class ModalWindow : Hideable, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected Text generalText;

        public static ModalWindow Instance { get; protected set; }
        protected bool isMouseInside;
        protected Animator animator;

        protected void Start()
        {
            Instance = this;
            //todo add check for multiple objects with that script
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Overwrites previous modal window if it was open
        /// </summary>    
        internal static void Show(string text)
        {
            Instance.generalText.text = text;
            Instance.Show();
        }

        public override void Show()
        {
            animator.Play("Opening");
            animator.SetBool("IsClosed", false);
            base.Show();            
        }

        public override void Hide()
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
                base.Hide();
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

        /// <summary>
        /// For tests only
        /// </summary>
        public void OnBigTextTest()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 90; i++)
            {
                sb.Append("Testing long text ");
            }

            generalText.text = sb.ToString();
        }
    }
}