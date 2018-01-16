using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class ProductionWindow : DragPanel, IFiltrable<Factory>
    {
        [SerializeField]
        private ProductionWindowTable table;
        [SerializeField]
        private List<Factory> factoriesToShow;

        //private Province showingProvince;
        void Start()
        {
            MainCamera.productionWindow = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.bottomPanel.GetComponent<RectTransform>().rect.height - 2f);
            Canvas.ForceUpdateCanvases();
            Hide();
        }
        //public void setFactoriesToShow(List<Factory> list)
        //{
        //    factoriesToShow = list;
        //}
        //public Province getShowingProvince()
        //{
        //    return showingProvince;
        //}
        //public void show(Province inn)
        //{
        //    showingProvince = inn;
        //    if (showingProvince != null)
        //    {
        //        //MainCamera.productionWindow.setFactoriesToShow(showingProvince.allFactories);
        //        table.AddFilter(x=>x.getProvince() == showingProvince);
        //    }
        //    Show();
        //}
        //internal void SetAllFactoriesToShow()
        //{
        //    factoriesToShow = new List<Factory>();            
        //    foreach (Province province in Game.Player.ownedProvinces)
        //        foreach (Factory factory in province.allFactories)
        //            factoriesToShow.Add(factory);            
        //}
        public void onShowAllClick()
        {
           // SetAllFactoriesToShow();
            //show(null, true);
            table.ClearAllFiltres();
            table.Refresh();
        }
        public override void Refresh()
        {
            //if (showingProvince == null)
            //{
            //    SetAllFactoriesToShow();
            //}
           
            table.Refresh();
        }

        public void removeFactory(Factory fact)
        {
            if (factoriesToShow != null && factoriesToShow.Contains(fact))
            {
                factoriesToShow.Remove(fact);
                if (this.isActiveAndEnabled)
                    Refresh();
            }
        }
        public bool IsSetAnyFilter()
        {
            return table.IsSetAnyFilter();
        }

        public bool IsSetThatFilter(Predicate<Factory> filter)
        {
            return table.IsSetThatFilter(filter);
        }
        public void AddFilter(Predicate<Factory> filter)
        {
            table.AddFilter(filter);
        }

        public void RemoveFilter(Predicate<Factory> filter)
        {
            table.RemoveFilter(filter);
        }

        public void ClearAllFiltres()
        {
            table.ClearAllFiltres();
        }
    }
}