using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class PopulationPanel : DragPanel
    {
        [SerializeField]
        private UITableNew table;
        private Province m_showingProvince;

        public Province ShowingProvince
        {
            get { return m_showingProvince; }
            set { m_showingProvince = value; }
        }

        // Use this for initialization
        void Start()
        {
            MainCamera.populationPanel = this;
            //show(false);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.topPanel.GetComponent<RectTransform>().rect.height * -1f);
            Canvas.ForceUpdateCanvases();
            Hide();
        }
        internal void SetAllPopsToShow()
        {
            if (Game.Player != null)
            {
                List<PopUnit> er = new List<PopUnit>();
                //Game.popListToShow.Clear();
                foreach (Province province in Game.Player.ownedProvinces)
                    foreach (PopUnit popUnit in province.allPopUnits)
                        // Game.popListToShow.Add(popUnit);
                        er.Add(popUnit);
                Game.popsToShowInPopulationPanel = er;
            }
        }

        public void onShowAllClick()
        {
            //hide();
            SetAllPopsToShow();
            //showAll = true;
            ShowingProvince = null;
            Show();
        }
        public override void Refresh()
        {
            if (ShowingProvince == null)
                SetAllPopsToShow();
            //foreach (var item in tables)
            //    item.refreshContent();     
            table.Refresh();
        }
    }
}