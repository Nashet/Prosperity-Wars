using Nashet.UnityUIUtils;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class StatisticsPanel : DragPanel
    {
        [SerializeField] protected StatisticsPanelTable table;

        [SerializeField] protected Animator animator;

        // Use this for initialization
        protected void Start()
        {
            MainCamera.StatisticPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(20f, -460f);

            animator = GetComponent<Animator>();
            //show(false);
            Canvas.ForceUpdateCanvases();
            base.Hide();
        }

        public override void Refresh()
        {
            table.Refresh();
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

        protected void Update()
        {
            if (animator && animator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
            {
                base.Hide();
            }
        }
    }
}