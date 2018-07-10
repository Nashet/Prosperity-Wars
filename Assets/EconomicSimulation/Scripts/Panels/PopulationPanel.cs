using System;
using Nashet.UnityUIUtils;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class PopulationPanel : DragPanel, IFiltrable<PopUnit>
    {
        [SerializeField]
        private PopulationPanelTable table;

        //public readonly static Predicate<PopUnit> filterSelectedProvince = x => x.Province == Game.selectedProvince;
        private Predicate<PopUnit> filterSelectedProvince;

        //private Province m_showingProvince;

        private Province showingProvince;

        public Province SelectedProvince
        {
            get { return showingProvince; }
        }

        // Use this for initialization
        private void Start()
        {
            filterSelectedProvince = x => x.Province == showingProvince;
            MainCamera.populationPanel = this;
            //show(false);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -440f);
            Canvas.ForceUpdateCanvases();
            Hide();
        }

        public override void Refresh()
        {
            //if (showingProvince == null)
            //    SetAllPopsToShow();
            table.Refresh();
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

        //Expression<Func<PopUnit, bool>> isAdult = x => x.Type == PopType.Workers;

        //Expression<Func<PopUnit, bool>> isMale = x => x.Type == PopType.Farmers;
        //var isAdultMale = Expression.And(isAdult, isMale);

        private readonly Predicate<PopUnit> filterWorkers = (x => x.Type != PopType.Workers);

        public void OnFilterWorkersChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterWorkers);
            else
                AddFilter(filterWorkers);
            Refresh();
        }

        private readonly Predicate<PopUnit> filterFarmers = (x => x.Type != PopType.Farmers);

        public void OnFilterFarmersChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterFarmers);
            else
                AddFilter(filterFarmers);
            Refresh();
        }

        private readonly Predicate<PopUnit> filterArtisans = (x => x.Type != PopType.Artisans);

        public void OnFilterArtisansChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterArtisans);
            else
                AddFilter(filterArtisans);
            Refresh();
        }

        private readonly Predicate<PopUnit> filterTribesmen = (x => x.Type != PopType.Tribesmen);

        public void OnFilterTribesmenChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterTribesmen);
            else
                AddFilter(filterTribesmen);
            Refresh();
        }

        private readonly Predicate<PopUnit> filterCapitalists = x => x.Type != PopType.Capitalists;

        public void OnFilterCapitalistsChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterCapitalists);
            else
                AddFilter(filterCapitalists);
            Refresh();
        }

        private readonly Predicate<PopUnit> filterAristocrats = x => x.Type != PopType.Aristocrats;

        public void OnFilterAristocratsChange(bool @checked)
        {
            if (@checked)
                RemoveFilter(filterAristocrats);
            else
                AddFilter(filterAristocrats);
            Refresh();
        }

        private readonly Predicate<PopUnit> filterSoldiers = x => x.Type != PopType.Soldiers;

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

        public void ClearAllFiltres() // show all button
        {
            showingProvince = null;
            table.ClearAllFiltres();
            Refresh();
        }

        public void AddAllFiltres()// hide all button
        {
            //showingProvince = null;
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