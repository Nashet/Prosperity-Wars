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
        //public readonly static Predicate<PopUnit> filterSelectedProvince = x => x.getProvince() == Game.selectedProvince;
        private Predicate<Factory> filterSelectedProvince;
        private readonly static Predicate<Factory> filterOnlyExisting = (x => !x.isToRemove());
        private Province showingProvince;

        void Start()
        {
            filterSelectedProvince = x => x.getProvince() == showingProvince;
            MainCamera.productionWindow = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.bottomPanel.GetComponent<RectTransform>().rect.height - 2f);
            Canvas.ForceUpdateCanvases();
            //ClearAllFiltres();
            table.AddFilter(x => !x.isToRemove());
            Hide();

        }
       
        public bool IsSelectedAnyProvince()
        {
            return showingProvince != null;
        }
        public bool IsSelectedProvince(Province province)
        {
            return showingProvince == province;
        }
        public void SelectProvince(Province province)
        {
            showingProvince = province;
            if (showingProvince == null)
                RemoveFilter(filterSelectedProvince);
            else
                AddFilter(filterSelectedProvince);
        }
        public override void Refresh()
        {           
            table.Refresh();
        }
       
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
        public void ClearAllFiltres()// show all button
        {
            showingProvince = null;
            table.ClearAllFiltres();
            table.AddFilter(filterOnlyExisting);
            Refresh();
        }

        public void AddAllFiltres()// hide all button
        {
            //showingProvince = null;
            table.AddAllFiltres();
            RemoveFilter(filterSelectedProvince);
            Refresh();
        }
    }
}