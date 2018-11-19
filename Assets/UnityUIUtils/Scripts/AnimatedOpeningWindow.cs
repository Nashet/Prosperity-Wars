using UnityEngine;
namespace Nashet.UnityUIUtils
{
    public abstract class AnimatedOpeningWindow : DragPanel
    {
        protected Animator animator;

        protected virtual void Start()
        {            
            base.Hide();
            animator = GetComponent<Animator>();
        }

        public override void Show()
        {
            if (animator)
            {
                animator.Play("Opening");
            }
            base.Show();
        }

        public override void Hide()
        {
            if (animator)
            {
                animator.SetTrigger("IsClosed");
            }
        }

        protected virtual void Update()
        {
            if (animator && animator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
            {
                base.Hide();
            }
        }
    }
}