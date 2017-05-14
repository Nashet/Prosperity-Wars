using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TopPanel : MonoBehaviour
{
    public Button btnPlay, btnStep, btnTrade;
    public Text generalText;
    // Use this for initialization
    void Start()
    {
        //generaltext = transform.FindChild("GeneralText").gameObject.GetComponent<Text>();
        btnPlay.onClick.AddListener(() => onbtnPlayClick(btnPlay));
        btnStep.onClick.AddListener(() => onbtnStepClick(btnPlay));
        btnPlay.image.color = Color.grey;
        MainCamera.topPanel = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void refresh()
    {

        generalText.text = "Economic Simulation v10 Date: " + Game.date + " Country: " + Game.player.name
            + "\nMoney: " + Game.player.wallet.haveMoney
            + " Science points: " + Game.player.sciencePoints
            + " Men: " + Game.player.getMenPopulation()
            + " Storage: " + Game.player.storageSet.ToString();
    }
    public void onTradeClick()
    {
        if (MainCamera.tradeWindow.isActiveAndEnabled)
            MainCamera.tradeWindow.hide();
        else
            MainCamera.tradeWindow.show(true);
    }
    public void onDiplomacyClick()
    {
        if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            MainCamera.diplomacyPanel.hide();
        else
            MainCamera.diplomacyPanel.show();
    }
    public void onInventionsClick()
    {

        if (MainCamera.inventionsPanel.isActiveAndEnabled)
            MainCamera.inventionsPanel.hide();
        else
            MainCamera.inventionsPanel.show(true);
    }
    public void onEnterprisesClick()
    {
        //MainCamera.productionWindow.show(null, true);
        //MainCamera.productionWindow.onShowAllClick();

        //if (MainCamera.productionWindow.isActiveAndEnabled)
        //    MainCamera.productionWindow.hide();
        //else
        //{
        //    MainCamera.productionWindow.show(null, true);
        //    MainCamera.productionWindow.onShowAllClick();
        //}
        if (MainCamera.productionWindow.isActiveAndEnabled)
            if (MainCamera.productionWindow.showingProvince == null)
                MainCamera.productionWindow.hide();
            else
            {
                MainCamera.productionWindow.show(null, true);
                MainCamera.productionWindow.onShowAllClick();
            }
        else
        {
            MainCamera.productionWindow.show(null, true);
            MainCamera.productionWindow.onShowAllClick();
        }
    }
    public void onPopulationClick()
    {
        //MainCamera.populationPanel.onShowAllClick();
        ////MainCamera.populationPanel.show();

        if (MainCamera.populationPanel.isActiveAndEnabled)
            if (MainCamera.populationPanel.showAll)
                MainCamera.populationPanel.hide();
            else
                MainCamera.populationPanel.onShowAllClick();
        else
            MainCamera.populationPanel.onShowAllClick();
    }
    public void onPoliticsClick()
    {
        if (MainCamera.politicsPanel.isActiveAndEnabled)
            MainCamera.politicsPanel.hide();
        else
            MainCamera.politicsPanel.show(true);
    }
    public void onFinanceClick()
    {
        if (MainCamera.financePanel.isActiveAndEnabled)
            MainCamera.financePanel.hide();
        else
            MainCamera.financePanel.show();
    }
    void onbtnStepClick(Button button)
    {
        if (Game.haveToRunSimulation)
        {
            Game.haveToRunSimulation = false;
            button.image.color = Color.grey;
            Text text = button.GetComponentInChildren<Text>();
            text.text = "Pause";
        }
        else
            Game.haveToStepSimulation = true;
    }
    void onbtnPlayClick(Button button)
    {
        Game.haveToRunSimulation = !Game.haveToRunSimulation;
        if (Game.haveToRunSimulation)
        {
            button.image.color = Color.white;
            Text text = button.GetComponentInChildren<Text>();
            text.text = "Playing";
        }
        else
        {
            button.image.color = Color.grey;
            Text text = button.GetComponentInChildren<Text>();
            text.text = "Pause";
        }
    }
}
