using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq.Expressions;

namespace Nashet.EconomicSimulation
{
    public class ProductionWindow : DragPanel, IFiltrable<Factory>
    {
        [SerializeField]
        private ProductionWindowTable table;
        public readonly static Predicate<Factory> filterSelectedProvince = (x => x.getProvince() == Game.selectedProvince);
        private readonly static Predicate<Factory> filterOnlyExisting = (x => !x.isToRemove());

       

        //private Province showingProvince;
        void Start()
        {
            MainCamera.productionWindow = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.bottomPanel.GetComponent<RectTransform>().rect.height - 2f);
            Canvas.ForceUpdateCanvases();
            //ClearAllFiltres();
            table.AddFilter(x => !x.isToRemove());
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

        public override void Refresh()
        {
            //if (showingProvince == null)
            //{
            //    SetAllFactoriesToShow();
            //}

            table.Refresh();
        }

        //public void removeFactory(Factory fact)
        //{
        //    if (factoriesToShow != null && factoriesToShow.Contains(fact))
        //    {
        //        factoriesToShow.Remove(fact);
        //        if (this.isActiveAndEnabled)
        //            Refresh();
        //    }
        //}
        private readonly Predicate<Factory> filterGovernmentOwned = (x => x.getOwner() != x.getCountry());
        public void OnGovernmentOwnedFilterChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterGovernmentOwned);
            else
                AddFilter(filterGovernmentOwned);

            Refresh();
        }
        private readonly Predicate<Factory> filterPrivateOwned = (x => x.getOwner() == x.getCountry());
        public void OnPrivateOwnedFilterChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterPrivateOwned);
            else
                AddFilter(filterPrivateOwned);

            Refresh();
        }
        private readonly Predicate<Factory> filterSubsidized = (x => !x.isSubsidized());
        public void OnFilterSubsidizedChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterSubsidized);
            else
                AddFilter(filterSubsidized);

            Refresh();
        }
        public bool IsSetAnyFilter()
        {
            return table.IsSetAnyFilter();
        }
        public bool IsAppliedThatFilter(Predicate<Factory> filter)
        {
            return table.IsAppliedThatFilter(filter);
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
            table.AddFilter(filterOnlyExisting);
            Refresh();
        }

        public void AddAllFiltres()// show all button
        {
            table.AddAllFiltres();
            RemoveFilter(filterSelectedProvince);
            Refresh();
        }
    }
}