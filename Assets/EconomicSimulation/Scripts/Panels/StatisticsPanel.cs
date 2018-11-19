using Nashet.UnityUIUtils;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class StatisticsPanel : DragPanel
    {
        [SerializeField] protected StatisticsPanelTable table;        

        // Use this for initialization
        protected  void Start()
        {
            MainCamera.StatisticPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(20f, -460f);            
           
            Canvas.ForceUpdateCanvases();            
            base.Hide();
        }

        public override void Refresh()
        {
            table.Refresh();
        }
    }
}