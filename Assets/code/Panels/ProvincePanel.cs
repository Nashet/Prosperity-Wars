using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;

public class ProvincePanel : MonoBehaviour
{
    public Text generaltext;
    public Button btnOwner, btnBuild, btAttackThat, btMobilize;
    // Use this for initialization    
    void Start()
    {
        MainCamera.provincePanel = this;
        hide();
    }
    public void hide()
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
        sb.Append("\nMiddle loyalty: ").Append(province.getMiddleLoyalty());
        sb.Append("\nMajor culture: ").Append(province.getMajorCulture());
        sb.Append("\nTax income: ").Append(province.getIncomeTax());
        sb.Append("\nResource: ").Append(province.getResource());
        sb.Append("\nTerrain: ").Append(province.getTerrain());
        sb.Append("\nRural overpopulation: ").Append(province.getOverpopulation());
        sb.Append("\nCores: ").Append(province.getCoresDescription());
        if (province.getModifiers().Count > 0)
            sb.Append("\nModifiers: ").Append(CollectionExtensions.getString(province.getModifiers()));


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

        if (Game.devMode) sb.Append("\nColor: ").Append( province.getColorID());

        //btAttackThat.interactable = Game.Player.canAttack(province);
        btAttackThat.interactable = Country.canAttack.isAllTrue(Game.Player, province, out btAttackThat.GetComponentInChildren<ToolTipHandler>().tooltip);
        //if (!btAttackThat.interactable)
        //    btAttackThat.GetComponentInChildren<ToolTipHandler>().tooltip = "Can attack only neighbors";
        //else btAttackThat.GetComponentInChildren<ToolTipHandler>().tooltip = "";
        generaltext.text = sb.ToString();
    }
    public void onddMapModesChange(int newMapMode)
    {
        if (Game.getMapMode() != newMapMode)
            Game.redrawMapAccordingToMapMode(newMapMode);

    }
}
