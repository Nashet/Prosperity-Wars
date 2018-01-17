using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Nashet.UnityUIUtils;
using System;
using System.Linq.Expressions;

namespace Nashet.EconomicSimulation
{
    public class PopulationPanel : DragPanel, IFiltrable<PopUnit>
    {
        [SerializeField]
        private PopulationPanelTable table;

        public readonly static Predicate<PopUnit> filterSelectedProvince = new Predicate<PopUnit>(x => x.getProvince() == Game.selectedProvince);

        //private Province m_showingProvince;

        //public Province showingProvince
        //{
        //    get { return m_showingProvince; }
        //    set { m_showingProvince = value; }
        //}

        // Use this for initialization

        void Start()
        {            
            MainCamera.populationPanel = this;
            //show(false);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.topPanel.GetComponent<RectTransform>().rect.height * -1f);
            Canvas.ForceUpdateCanvases();
            Hide();
        }
        //internal void SetAllPopsToShow()
        //{
        //    if (Game.Player != null)
        //    {
        //        List<PopUnit> er = new List<PopUnit>();
        //        //Game.popListToShow.Clear();
        //        foreach (Province province in Game.Player.ownedProvinces)
        //            foreach (PopUnit popUnit in province.allPopUnits)
        //                // Game.popListToShow.Add(popUnit);
        //                er.Add(popUnit);
        //        Game.popsToShowInPopulationPanel = er;
        //    }
        //}

       
        public override void Refresh()
        {
            //if (showingProvince == null)
            //    SetAllPopsToShow();               
            table.Refresh();
            
        }
        //Expression<Func<PopUnit, bool>> isAdult = x => x.popType == PopType.Workers;

        
        //Expression<Func<PopUnit, bool>> isMale = x => x.popType == PopType.Farmers;
        //var isAdultMale = Expression.And(isAdult, isMale);


        private Predicate<PopUnit> filterWorkers = new Predicate<PopUnit>(x => x.popType != PopType.Workers);
        public void OnFilterWorkersChange(bool @checked)
        {            
            if (@checked)
                RemoveFilter(filterWorkers);
            else
                AddFilter(filterWorkers);
            Refresh();
        }
        private Predicate<PopUnit> filterFarmers = new Predicate<PopUnit>(x => x.popType != PopType.Farmers);
        public void OnFilterFarmersChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterFarmers);
            else
                AddFilter(filterFarmers);
            Refresh();
        }
        //private static bool _filterArtisans(PopUnit x)
        //{ return x.popType != PopType.Artisans; }
        //private Predicate<PopUnit> filterArtisans = _filterArtisans;
        private Predicate<PopUnit> filterArtisans = new Predicate<PopUnit>(x => x.popType != PopType.Artisans);        
        public void OnFilterArtisansChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterArtisans);
            else
                AddFilter(filterArtisans);
            Refresh();
        }
        private Predicate<PopUnit> filterTribesmen = new Predicate<PopUnit>(x => x.popType != PopType.Tribesmen);
        public void OnFilterTribesmenChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterTribesmen);
            else
                AddFilter(filterTribesmen);
            Refresh();
        }

        private Predicate<PopUnit> filterCapitalists = new Predicate<PopUnit>(x => x.popType != PopType.Capitalists);
        public void OnFilterCapitalistsChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterCapitalists);
            else
                AddFilter(filterCapitalists);
            Refresh();
        }
        private Predicate<PopUnit> filterAristocrats = new Predicate<PopUnit>(x => x.popType != PopType.Aristocrats);
        public void OnFilterAristocratsChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterAristocrats);
            else
                AddFilter(filterAristocrats);
            Refresh();
        }
        private Predicate<PopUnit> filterSoldiers = new Predicate<PopUnit>(x => x.popType != PopType.Soldiers);
        public void OnFilterSoldiersChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterSoldiers);
            else
                AddFilter(filterSoldiers);
            Refresh();
        }
        public void AddFilter(Predicate<PopUnit> filter)
        {
            (table).AddFilter(filter);
        }

        public void RemoveFilter(Predicate<PopUnit> filter)
        {
            (table).RemoveFilter(filter);
        }

        public void ClearAllFiltres()
        {
            table.ClearAllFiltres();            
            Refresh();
        }
        public void AddAllFiltres()// show all button
        {
            table.AddAllFiltres();
            RemoveFilter(filterSelectedProvince);            
            Refresh();
        }
        public bool IsSetAnyFilter()
        {
            return ((IFiltrable<PopUnit>)table).IsSetAnyFilter();
        }

        public bool IsAppliedThatFilter(Predicate<PopUnit> filter)
        {
            return ((IFiltrable<PopUnit>)table).IsAppliedThatFilter(filter);
        }

       
    }
}