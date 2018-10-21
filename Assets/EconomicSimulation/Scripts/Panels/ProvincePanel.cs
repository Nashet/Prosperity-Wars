using System;
using System.Linq;
using System.Text;
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

        //private Province selectedProvince;
        // Use this for initialization
        private void Start()
        {
            MainCamera.provincePanel = this;
            Hide();
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
            Country whomGrant = Game.selectedProvince.AllCores().Where(x => x != Game.Player && !x.IsAlive).Random();
            if (whomGrant == null)
                whomGrant = Game.selectedProvince.AllCores().Where(x => x != Game.Player).Random();

            whomGrant.onGrantedProvince(Game.selectedProvince);
            //MainCamera.refreshAllActive();
            UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
        }

        public void onCountryDiplomacyClick()
        {
            Game.Player.events.RiseClickedOn(new CountryEventArgs(Game.selectedProvince.Country));

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
            Game.selectedProvince.mobilize();
            //MainCamera.militaryPanel.show(null);
        }

        public void onEnterprisesClick()
        {
            if (MainCamera.productionWindow.isActiveAndEnabled)
                if (MainCamera.productionWindow.IsSelectedAnyProvince())
                {
                    if (MainCamera.productionWindow.IsSelectedProvince(Game.selectedProvince))
                        MainCamera.productionWindow.Hide();
                    else
                    {
                        MainCamera.productionWindow.SelectProvince(Game.selectedProvince);
                        MainCamera.productionWindow.Refresh();
                    }
                }
                else
                {
                    MainCamera.productionWindow.SelectProvince(Game.selectedProvince);
                    MainCamera.productionWindow.Refresh();
                }
            else
            {
                MainCamera.productionWindow.SelectProvince(Game.selectedProvince);
                MainCamera.productionWindow.Show();
            }
        }

        public void onPopulationDetailsClick()
        {
            if (MainCamera.populationPanel.isActiveAndEnabled)
                if (MainCamera.populationPanel.IsSelectedAnyProvince())
                {
                    //if (MainCamera.populationPanel.IsAppliedThatFilter(PopulationPanel.filterSelectedProvince))
                    if (MainCamera.populationPanel.IsSelectedProvince(Game.selectedProvince))
                        MainCamera.populationPanel.Hide();
                    else
                    {
                        //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                        MainCamera.populationPanel.SelectProvince(Game.selectedProvince);
                        MainCamera.populationPanel.Refresh();
                    }
                }
                else
                {
                    //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                    MainCamera.populationPanel.SelectProvince(Game.selectedProvince);
                    MainCamera.populationPanel.Refresh();
                }
            else
            {
                //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                MainCamera.populationPanel.SelectProvince(Game.selectedProvince);
                MainCamera.populationPanel.Show();
            }
        }

        public void onAttackThatClick()
        {

            //if (MainCamera.militaryPanel.isActiveAndEnabled)
            //    MainCamera.militaryPanel.Hide();
            //else
            //MainCamera.militaryPanel.Show();
            MainCamera.militaryPanel.show(Game.selectedProvince);
        }

        public override void Refresh()
        {
            var sb = new StringBuilder("Province name: ").Append(Game.selectedProvince);
            if (Game.devMode)
            {
                sb.Append("\nID: ").Append(Game.selectedProvince.ID);
                sb.Append("\nNeighbors: ").Append(Game.selectedProvince.AllNeighbors().ToString(", "));
            }
            sb.Append("\nPopulation (with families): ").Append(Game.selectedProvince.getFamilyPopulation());

            sb.Append("\nAverage loyalty: ").Append(Game.selectedProvince.AllPops.GetAverageProcent(x => x.loyalty));
            //sb.Append("\nMajor culture: ").Append(Game.selectedProvince.getMajorCulture());
            //sb.Append("\nGDP: ").Append(Game.selectedProvince.getGDP());
            sb.Append("\nResource: ");
            if (Game.selectedProvince.getResource() == null)
                sb.Append("none ");
            else
                sb.Append(Game.selectedProvince.getResource());
            //sb.Append("\nTerrain: ").Append(Game.selectedProvince.getTerrain());
            //sb.Append("\nRural overpopulation: ").Append(Game.selectedProvince.GetOverpopulation());
            sb.Append("\nCores: ").Append(Game.selectedProvince.getCoresDescription());
            

            sb.Append("\nCultures: ").Append(Game.selectedProvince.AllPops.Group(x => x.culture, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 2));

            sb.Append("\nClasses: ").Append(Game.selectedProvince.AllPops.Group(x => x.Type, y => y.population.Get())
                .OrderByDescending(x => x.Value.get()).ToString(", ", 0));

            if (Game.selectedProvince.getModifiers().Count > 0)
                sb.Append("\nModifiers: ").Append(ToStringExtensions.ToString(Game.selectedProvince.getModifiers()));

            Text text = btnOwner.GetComponentInChildren<Text>();
            text.text = "Owner: " + Game.selectedProvince.Country;

            btnBuild.interactable = ProductionType.allowsForeignInvestments.checkIftrue(Game.Player, Game.selectedProvince, out btnBuild.GetComponent<ToolTipHandler>().text);
            btnBuild.GetComponent<ToolTipHandler>().AddText("\nHotkey is " + "B" + " button");

            btMobilize.interactable = Province.doesCountryOwn.checkIftrue(Game.Player, Game.selectedProvince, out btMobilize.GetComponent<ToolTipHandler>().text);
            btMobilize.GetComponent<ToolTipHandler>().AddText("\nHotkey is " + "M" + " button");

            //if (Game.devMode)
            //    sb.Append("\nColor: ").Append(province.getColorID());
            btAttackThat.interactable = Diplomacy.canAttack.isAllTrue(Game.selectedProvince, Game.Player, out btAttackThat.GetComponent<ToolTipHandler>().text);
            btAttackThat.GetComponent<ToolTipHandler>().AddText("\nHotkey is " + "T" + " button");
            btGrandIndependence.interactable = Province.canGetIndependence.isAllTrue(Game.selectedProvince, Game.Player, out btGrandIndependence.GetComponent<ToolTipHandler>().text);
            generaltext.text = sb.ToString();
        }

        public override void Hide()
        {
            base.Hide();
            MainCamera.selectProvince(-1);
        }
    }
}