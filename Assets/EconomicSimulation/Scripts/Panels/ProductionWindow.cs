using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class ProductionWindow : DragPanel
    {
        [SerializeField]
        private UITableNew table;
        private Province showingProvince;
        void Start()
        {
            MainCamera.productionWindow = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.bottomPanel.GetComponent<RectTransform>().rect.height - 2f);
            Canvas.ForceUpdateCanvases();
            Hide();
        }
        public Province getShowingProvince()
        {
            return showingProvince;
        }
        public void show(Province inn, bool bringOnTop)
        {
            showingProvince = inn;
            if (showingProvince != null)
            {
                Game.factoriesToShowInProductionPanel = showingProvince.allFactories;
            }
            Show();                       
        }
        internal void SetAllFactoriesToShow()
        {
            List<Factory> er = new List<Factory>();
            //Game.popListToShow.Clear();
            foreach (Province province in Game.Player.ownedProvinces)
                foreach (Factory factory in province.allFactories)
                    // Game.popListToShow.Add(popUnit);
                    er.Add(factory);
            Game.factoriesToShowInProductionPanel = er;
        }
        public void onShowAllClick()
        {
            SetAllFactoriesToShow();
            show(null, true);
        }
        public override void Refresh()
        {

            if (showingProvince == null)
            {
                SetAllFactoriesToShow();
            }
            //foreach (var item in tables)
            //    item.refreshContent();
            table.Refresh();
        }

        public void removeFactory(Factory fact)
        {
            if (Game.factoriesToShowInProductionPanel != null && Game.factoriesToShowInProductionPanel.Contains(fact))
            {
                Game.factoriesToShowInProductionPanel.Remove(fact);
                if (MainCamera.productionWindow.isActiveAndEnabled)
                    Refresh();
            }
        }
    }
}