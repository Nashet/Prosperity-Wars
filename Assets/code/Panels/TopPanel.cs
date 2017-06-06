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
        btnPlay.onClick.AddListener(() => onbtnPlayClick(btnPlay));
        btnStep.onClick.AddListener(() => onbtnStepClick(btnPlay));
        btnPlay.image.color = Color.grey;
        MainCamera.topPanel = this;
    }

   
    public void refresh()
    {

        generalText.text = "Economic Simulation v0.11.0 Date: " + Game.date + " Country: " + Game.Player.getName()
            + "\nMoney: " + Game.Player.cash
            + " Science points: " + Game.Player.sciencePoints
            + " Men: " + Game.Player.getMenPopulation();
        //+ " Storage: " + Game.player.storageSet.ToString();
    }
    public void onTradeClick()
    {
        if (MainCamera.tradeWindow.isActiveAndEnabled)
            MainCamera.tradeWindow.hide();
        else
            MainCamera.tradeWindow.show(true);
    }
    public void onExitClick()
    {
        Application.Quit();
    }
    public void onMilitaryClick()
    {
        if (MainCamera.militaryPanel.isActiveAndEnabled)
            MainCamera.militaryPanel.hide();
        else
            MainCamera.militaryPanel.show(null);
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
        if (Game.isRunningSimulation())
        {
            Game.pauseSimulation();
            button.image.color = Color.grey;
            Text text = button.GetComponentInChildren<Text>();
            text.text = "Pause";
        }
        else
            Game.makeOneStepSimulation();
    }
    void onbtnPlayClick(Button button)
    {
        switchHaveToRunSimulation(button);
    }
    public void switchHaveToRunSimulation(Button button)
    {
        if (Game.isRunningSimulation())
            Game.pauseSimulation();
        else
            Game.continueSimulation();

        if (Game.isRunningSimulation())
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
