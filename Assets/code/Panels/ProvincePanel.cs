using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;

public class ProvincePanel : MonoBehaviour
{
    public Text generaltext;
    public Button btnOwner, btnBuild, btAttackThat, btMobilize, btGrandIndependence;

    // Use this for initialization    
    void Start()
    {
        MainCamera.provincePanel = this;
        hide();
    }
    public void hide()    // it's cause it did not inherit my panel class
    {
        gameObject.SetActive(false);
    }
    public void show()
    {
        gameObject.SetActive(true);
    }
    public void onCloseClick()
    {
        hide();
    }
    public void onBuildClick()
    {
        //MainCamera.buildPanel.show(true);
        if (MainCamera.buildPanel.isActiveAndEnabled)
            MainCamera.buildPanel.hide();
        else
            MainCamera.buildPanel.show(true);
    }
    public void onGrantIndependenceClick()
    {
        Country whomGrant = Game.selectedProvince.getRandomCore(x => x != Game.Player && !x.isAlive());
        if (whomGrant == null)
            whomGrant = Game.selectedProvince.getRandomCore(x => x != Game.Player);

        whomGrant.onGrantedProvince(Game.selectedProvince);
    }
    public void onCountryDiplomacyClick()
    {
        if (MainCamera.diplomacyPanel.isActiveAndEnabled)
        {
            if (MainCamera.diplomacyPanel.getSelectedCountry() == Game.selectedProvince.getCountry())

                MainCamera.diplomacyPanel.hide();
            else
                MainCamera.diplomacyPanel.show(Game.selectedProvince.getCountry());
        }
        else
            MainCamera.diplomacyPanel.show(Game.selectedProvince.getCountry());
    }
    public void onMobilizeClick()
    {
        Game.selectedProvince.mobilize();
        MainCamera.militaryPanel.show(null);
    }
    public void onPopulationDetailsClick()
    {
        if (MainCamera.populationPanel.isActiveAndEnabled)
            if (MainCamera.populationPanel.showingProvince == null)
            {
                MainCamera.populationPanel.hide();
                Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
                MainCamera.populationPanel.showingProvince = Game.selectedProvince;
                //MainCamera.populationPanel.showAll = false;
                MainCamera.populationPanel.show(true);
            }
            else
            {
                if (MainCamera.populationPanel.showingProvince == Game.selectedProvince)
                    MainCamera.populationPanel.hide();
                else
                {
                    MainCamera.populationPanel.hide();
                    Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
                    MainCamera.populationPanel.showingProvince = Game.selectedProvince;
                    //MainCamera.populationPanel.showAll = false;
                    MainCamera.populationPanel.show(true);
                }
            }
        else
        {
            Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
            MainCamera.populationPanel.showingProvince = Game.selectedProvince;
            //MainCamera.populationPanel.showAll = false;
            MainCamera.populationPanel.show(true);
        }

    }
    public void onAttackThatClick()
    {
        MainCamera.militaryPanel.show(Game.selectedProvince);
    }
    public void onEnterprisesClick()
    {
        if (MainCamera.productionWindow.isActiveAndEnabled)
            if (MainCamera.productionWindow.getShowingProvince() == null)
            {
                MainCamera.productionWindow.hide();
                Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
                //MainCamera.productionWindow.getShowingProvince() = Game.selectedProvince;
                MainCamera.productionWindow.show(Game.selectedProvince, true);
            }
            else
            {
                if (MainCamera.productionWindow.getShowingProvince() == Game.selectedProvince)
                    MainCamera.productionWindow.hide();
                else
                {
                    MainCamera.productionWindow.hide();
                    Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories; ;
                    //MainCamera.productionWindow.showingProvince = Game.selectedProvince;
                    MainCamera.productionWindow.show(Game.selectedProvince, true);
                }
            }
        else
        {
            Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
            //MainCamera.productionWindow.showingProvince = Game.selectedProvince;
            MainCamera.productionWindow.show(Game.selectedProvince, true);
        }
    }
    public void refresh(Province province)
    {
        var sb = new StringBuilder("Province name: ").Append(province);
        sb.Append("\nID: ").Append(province.getID());
        sb.Append("\nPopulation (with families): ").Append(province.getFamilyPopulation());
        sb.Append("\nAverage loyalty: ").Append(province.getAverageLoyalty());
        sb.Append("\nMajor culture: ").Append(province.getMajorCulture());
        sb.Append("\nGDP: ").Append(province.getGDP());
        sb.Append("\nResource: ");
        if (province.getResource() == null)
            sb.Append("none ");
        else
            sb.Append(province.getResource());
        sb.Append("\nTerrain: ").Append(province.getTerrain());
        sb.Append("\nRural overpopulation: ").Append(province.getOverpopulation());
        sb.Append("\nCores: ").Append(province.getCoresDescription());
        if (province.getModifiers().Count > 0)
            sb.Append("\nModifiers: ").Append(GetStringExtensions.getString(province.getModifiers()));


        //+ "\nNeighbors " + province.getNeigborsList()
        ;
        Text text = btnOwner.GetComponentInChildren<Text>();
        text.text = "Owner: " + province.getCountry();

        if (province.getCountry() == Game.Player)
        {
            btnBuild.GetComponentInChildren<ToolTipHandler>().tooltip = "";
            btnBuild.interactable = true;
            btMobilize.GetComponentInChildren<ToolTipHandler>().tooltip = "";
            btMobilize.interactable = true;
        }
        else
        {
            btnBuild.GetComponentInChildren<ToolTipHandler>().tooltip = "That isn't your province, right?";
            btnBuild.interactable = false;
            btMobilize.GetComponentInChildren<ToolTipHandler>().tooltip = "That isn't your province, right?";
            btMobilize.interactable = false;
        }

        if (Game.devMode)
            sb.Append("\nColor: ").Append(province.getColorID());
        btAttackThat.interactable = Country.canAttack.isAllTrue(province, Game.Player, out btAttackThat.GetComponentInChildren<ToolTipHandler>().tooltip);
        btGrandIndependence.interactable = Province.canGetIndependence.isAllTrue(province, Game.Player, out btGrandIndependence.GetComponentInChildren<ToolTipHandler>().tooltip);
        generaltext.text = sb.ToString();
    }
    public void onddMapModesChange(int newMapMode)
    {
        if (Game.getMapMode() != newMapMode)
            Game.redrawMapAccordingToMapMode(newMapMode);

    }
}
