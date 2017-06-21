using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

    // Update is called once per frame
    void Update()
    {

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
            if (MainCamera.productionWindow.showingProvince == null)
            {
                MainCamera.productionWindow.hide();
                Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
                MainCamera.productionWindow.showingProvince = Game.selectedProvince;
                MainCamera.productionWindow.show(Game.selectedProvince, true);
            }
            else
            {
                if (MainCamera.productionWindow.showingProvince == Game.selectedProvince)
                    MainCamera.productionWindow.hide();
                else
                {
                    MainCamera.productionWindow.hide();
                    Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories; ;
                    MainCamera.productionWindow.showingProvince = Game.selectedProvince;
                    MainCamera.productionWindow.show(Game.selectedProvince, true);
                }
            }
        else
        {
            Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
            MainCamera.productionWindow.showingProvince = Game.selectedProvince;
            MainCamera.productionWindow.show(Game.selectedProvince, true);
        }
    }
    public void refresh(Province province)
    {
        generaltext.text = "Province name: " + province
            + "\nID: " + province.getID()
            + "\nPopulation (with families): " + province.getFamilyPopulation()
            + "\nMiddle loyalty: " + province.getMiddleLoyalty()
            + "\nMajor culture: " + province.getMajorCulture()
            + "\nTax income: " + province.getIncomeTax()
            + "\nResource: " + province.getResource()
            + "\nRural overpopulation: " + province.getOverpopulation()
            + "\nCores: " + province.getCoresDescription();

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

        if (Game.devMode) generaltext.text += "\nColor: " + province.getColorID();

        btAttackThat.interactable = Game.Player.canAttack(province);
        if (!btAttackThat.interactable)
            btAttackThat.GetComponentInChildren<ToolTipHandler>().tooltip = "Can attack only neighbors";
        else btAttackThat.GetComponentInChildren<ToolTipHandler>().tooltip = "";

    }
    public void onddMapModesChange(int newMapMode)
    {
        if (Game.getMapMode() != newMapMode)
            Game.redrawMapAccordingToMapMode(newMapMode);

    }
}
