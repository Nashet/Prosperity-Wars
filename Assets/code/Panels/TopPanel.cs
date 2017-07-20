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
        hide();        
    }
    public void hide()
    {
        gameObject.SetActive(false);
    }
    public void show()
    {
        gameObject.SetActive(true);
        //panelRectTransform.SetAsLastSibling();
        refresh();
    }
    public void refresh()
    {
        generalText.text = "Economic Simulation v0.13.0 Date: " + Game.date.ToShortDateString() + " Country: " + Game.Player.getName()
            + "\nMoney: " + Game.Player.cash
            + " Science points: " + Game.Player.sciencePoints.get().ToString("N0")
            + " Men: " + Game.Player.getMenPopulation()
            + " avg. loyalty: " + Game.Player.getAverageLoyalty();
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
        if (MainCamera.productionWindow.isActiveAndEnabled)
            if (MainCamera.productionWindow.getShowingProvince() == null)
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

        if (MainCamera.populationPanel.isActiveAndEnabled)
            if (MainCamera.populationPanel.showingProvince==null)
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
