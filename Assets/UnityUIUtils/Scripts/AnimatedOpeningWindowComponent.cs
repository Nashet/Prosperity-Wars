using UnityEngine;
namespace Nashet.UnityUIUtils
{
    [RequireComponent(typeof(Animator))]
    public class AnimatedOpeningWindowComponent : MonoBehaviour, IHideable
    {
        protected Animator animator;

        public bool IsFinishedAnimation { get; protected set; }

        protected virtual void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void Show()
        {
            if (animator)
            {
                animator.Play("Opening");
            }
            IsFinishedAnimation = false;
        }

        public void Hide()
        {
            if (animator)
            {
                animator.SetTrigger("IsClosed");
            }
            else
                IsFinishedAnimation = true;
        }
        
        protected virtual void Update()
        {
            if (animator && animator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
            {
                IsFinishedAnimation = true;
            }
        }
    }
}