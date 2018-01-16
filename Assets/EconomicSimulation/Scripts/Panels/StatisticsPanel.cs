using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class StatisticsPanel : DragPanel
    {
        [SerializeField]
        private StatisticsPanelTable table;
        // Use this for initialization
        void Start()
        {
            MainCamera.StatisticPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, -100f);
            //show(false);
            Canvas.ForceUpdateCanvases();
            Hide();
        }        
       
        //public void onShowAllClick()
        //{
        //    //hide();
        //    SetAllPopsToShow();
        //    //showAll = true;
        //    showingProvince = null;
        //    show(true);
        //}
        public override void Refresh()
        {
            table.Refresh();
        }
    }
}
