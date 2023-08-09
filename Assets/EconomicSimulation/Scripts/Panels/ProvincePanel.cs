using System;
using System.Linq;
using System.Text;
using Nashet.GameplayControllers;
using Nashet.Map.GameplayControllers;
using Nashet.Map.Utils;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{

	public class ProvincePanel : Window
    {
        [SerializeField]
        private Text generaltext;

        [SerializeField]
        private Button btnOwner, btnBuild, btAttackThat, btMobilize, btGrandIndependence;

		[SerializeField]
		private ProvinceSelectionHelper provinceSelectionHelper;

		[SerializeField]
		private ProvinceSelectionController ProvinceSelectionController;

		// Use this for initialization
		private void Start()
        {
            MainCamera.provincePanel = this;
            Hide();
			provinceSelectionHelper.ProvinceSelected += ProvinceSelectedHandler;
		}

		private void OnDestroy()
		{
			provinceSelectionHelper.ProvinceSelected -= ProvinceSelectedHandler;
		}

		private void ProvinceSelectedHandler(Province province)
		{
            if (province == null)
            {
                HideInternal();
            }
            else
            {
                Show();
            }
		}

		public void onBuildClick()
        {
            //MainCamera.buildPanel.show(true);
            if (MainCamera.buildPanel.isActiveAndEnabled)
                MainCamera.buildPanel.Hide();
            else
                MainCamera.buildPanel.Show();
        }

        public void onGrantIndependenceClick()
        {
            Country whomGrant = provinceSelectionHelper.selectedProvince.AllCores().Where(x => x != Game.Player && !x.IsAlive).Random();
            if (whomGrant == null)
                whomGrant = provinceSelectionHelper.selectedProvince.AllCores().Where(x => x != Game.Player).Random();

            whomGrant.onGrantedProvince(provinceSelectionHelper.selectedProvince);
            //MainCamera.refreshAllActive();
            UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
        }

        public void onCountryDiplomacyClick()
        {
            Game.Player.events.RiseClickedOn(new CountryEventArgs(provinceSelectionHelper.selectedProvince.Country));

            //if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            //{
            //    if (MainCamera.diplomacyPanel.getSelectedCountry() == Game.selectedProvince.Country)

            //        MainCamera.diplomacyPanel.Hide();
            //    else
            //        MainCamera.diplomacyPanel.show(Game.selectedProvince.Country);
            //}
            //else
            //    MainCamera.diplomacyPanel.show(Game.selectedProvince.Country);
        }

        public void onMobilizeClick()
        {
			provinceSelectionHelper.selectedProvince.mobilize();
            //MainCamera.militaryPanel.show(null);
        }

        public void onEnterprisesClick()
        {
            if (MainCamera.productionWindow.isActiveAndEnabled)
                if (MainCamera.productionWindow.IsSelectedAnyProvince())
                {
                    if (MainCamera.productionWindow.IsSelectedProvince(provinceSelectionHelper.selectedProvince))
                        MainCamera.productionWindow.Hide();
                    else
                    {
                        MainCamera.productionWindow.SelectProvince(provinceSelectionHelper.selectedProvince);
                        MainCamera.productionWindow.Refresh();
                    }
                }
                else
                {
                    MainCamera.productionWindow.SelectProvince(provinceSelectionHelper.selectedProvince);
                    MainCamera.productionWindow.Refresh();
                }
            else
            {
                MainCamera.productionWindow.SelectProvince(provinceSelectionHelper.selectedProvince);
                MainCamera.productionWindow.Show();
            }
        }

        public void onPopulationDetailsClick()
        {
            if (MainCamera.populationPanel.isActiveAndEnabled)
                if (MainCamera.populationPanel.IsSelectedAnyProvince())
                {
                    //if (MainCamera.populationPanel.IsAppliedThatFilter(PopulationPanel.filterSelectedProvince))
                    if (MainCamera.populationPanel.IsSelectedProvince(provinceSelectionHelper.selectedProvince))
                        MainCamera.populationPanel.Hide();
                    else
                    {
                        //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                        MainCamera.populationPanel.SelectProvince(provinceSelectionHelper.selectedProvince);
                        MainCamera.populationPanel.Refresh();
                    }
                }
                else
                {
                    //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                    MainCamera.populationPanel.SelectProvince(provinceSelectionHelper.selectedProvince);
                    MainCamera.populationPanel.Refresh();
                }
            else
            {
                //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                MainCamera.populationPanel.SelectProvince(provinceSelectionHelper.selectedProvince);
                MainCamera.populationPanel.Show();
            }
        }

        public void onAttackThatClick()
        {

            //if (MainCamera.militaryPanel.isActiveAndEnabled)
            //    MainCamera.militaryPanel.Hide();
            //else
            //MainCamera.militaryPanel.Show();
            MainCamera.militaryPanel.show(provinceSelectionHelper.selectedProvince);
        }

        public override void Refresh()
        {
            var sb = new StringBuilder("Province name: ").Append(provinceSelectionHelper.selectedProvince);
            if (Game.devMode)
            {
                sb.Append("\nID: ").Append(provinceSelectionHelper.selectedProvince);
                sb.Append("\nNeighbors: ").Append(provinceSelectionHelper.selectedProvince.AllNeighbors().ToString(", "));
                sb.Append($", isCoastal {provinceSelectionHelper.selectedProvince.IsCoastal}, terrain - {provinceSelectionHelper.selectedProvince.Terrain} ");
			}
            sb.Append("\nPopulation (with families): ").Append(provinceSelectionHelper.selectedProvince.getFamilyPopulation());

            sb.Append("\nAverage loyalty: ").Append(provinceSelectionHelper.selectedProvince.AllPops.GetAverageProcent(x => x.loyalty));
            //sb.Append("\nMajor culture: ").Append(Game.selectedProvince.getMajorCulture());
            //sb.Append("\nGDP: ").Append(Game.selectedProvince.getGDP());
            sb.Append("\nResource: ");
            if (provinceSelectionHelper.selectedProvince.getResource() == null)
                sb.Append("none ");
            else
                sb.Append(provinceSelectionHelper.selectedProvince.getResource());
            //sb.Append("\nTerrain: ").Append(Game.selectedProvince.getTerrain());
            //sb.Append("\nRural overpopulation: ").Append(Game.selectedProvince.GetOverpopulation());
            sb.Append("\nCores: ").Append(provinceSelectionHelper.selectedProvince.getCoresDescription());
            

            sb.Append("\nCultures: ").Append(provinceSelectionHelper.selectedProvince.AllPops.Group(x => x.culture, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 2));

            sb.Append("\nClasses: ").Append(provinceSelectionHelper.selectedProvince.AllPops.Group(x => x.Type, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 0));

            if (provinceSelectionHelper.selectedProvince.getModifiers().Count > 0)
                sb.Append("\nModifiers: ").Append(ToStringExtensions.ToString(provinceSelectionHelper.selectedProvince.getModifiers()));

            Text text = btnOwner.GetComponentInChildren<Text>();
            text.text = "Owner: " + provinceSelectionHelper.selectedProvince.Country;

            btnBuild.interactable = ProductionType.allowsForeignInvestments.checkIftrue(Game.Player, provinceSelectionHelper.selectedProvince, out btnBuild.GetComponent<ToolTipHandler>().text);
            btnBuild.GetComponent<ToolTipHandler>().AddText("\nHotkey is B button");

            btMobilize.interactable = Province.doesCountryOwn.checkIftrue(Game.Player, provinceSelectionHelper.selectedProvince, out btMobilize.GetComponent<ToolTipHandler>().text);
            btMobilize.GetComponent<ToolTipHandler>().AddText("\nHotkey is M button");

            //if (Game.devMode)
            //    sb.Append("\nColor: ").Append(province.getColorID());
            btAttackThat.interactable = Diplomacy.canAttack.isAllTrue(provinceSelectionHelper.selectedProvince, Game.Player, out btAttackThat.GetComponent<ToolTipHandler>().text);
            btAttackThat.GetComponent<ToolTipHandler>().AddText("\nHotkey is T button");
            btGrandIndependence.interactable = Province.canGetIndependence.isAllTrue(provinceSelectionHelper.selectedProvince, Game.Player, out btGrandIndependence.GetComponent<ToolTipHandler>().text);
            generaltext.text = sb.ToString();
        }

		private void HideInternal()
		{
			base.Hide();
		}
		public override void Hide()
        {
            base.Hide();
			ProvinceSelectionController.selectProvince(null, null);
        }
    }
}