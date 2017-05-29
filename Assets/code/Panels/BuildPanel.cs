using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class BuildPanel : DragPanel
{   
    public ScrollRect table;
    public Text descriptionText;
    public Button buildButton;
    public FactoryType selectedFactoryType;
    StringBuilder sb = new StringBuilder();
    //Province province;
    // Use this for initialization
    void Start()
    {
        MainCamera.buildPanel = this;
        buildButton.interactable = false;
        hide();
    }
    
    public void show(bool bringOnTop)
    {
        gameObject.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();

    }
   


    public void onBuildClick()
    {
        //if (Game.player.economy.allowsFactoryBuildingByGovernment())
        {
            bool buildSomething = false;
            var resourceToBuild = selectedFactoryType.getBuildNeeds();
            if (Economy.isMarket.checkIftrue(Game.player))
            //if (Game.player.economy.status == Economy.StateCapitalism)
            //have money /resource
            {
                Value cost = Game.market.getCost(resourceToBuild);
                cost.add(Options.factoryMoneyReservPerLevel);
                if (Game.player.wallet.canPay(cost))
                {
                    var f = new Factory(Game.selectedProvince, Game.player, selectedFactoryType);
                    Game.player.wallet.pay(f.wallet, cost);
                    buildSomething = true;
                    MainCamera.factoryPanel.Show(f);
                }

            }
            else // non market
            {
                Storage needFood = resourceToBuild.findStorage(Product.Food);
                if (Game.player.storageSet.has(needFood))
                {
                    Factory fact = new Factory(Game.selectedProvince, Game.player, selectedFactoryType);

                    //wallet.pay(fact.wallet, new Value(100f));
                    Game.player.storageSet.subtract(needFood);
                    buildSomething = true;
                    MainCamera.factoryPanel.Show(fact);
                }
            }

            if (buildSomething == true)
            {
                // voteButton.interactable = false;
                MainCamera.topPanel.refresh();
                if (MainCamera.productionWindow.isActiveAndEnabled) MainCamera.productionWindow.refresh();
                selectedFactoryType = null;

                //Hide();
                //show();
                refresh();
            }
        }
    }
    public void refresh()
    {
        hide();
        if (selectedFactoryType != null)
        {
            sb.Clear();
            sb.Append("Build ").Append(selectedFactoryType);
            sb.Append("\n\nResources to build: ").Append(selectedFactoryType.getBuildNeeds()).Append(" cost: ").Append(selectedFactoryType.getBuildCost());            
            sb.Append("\nEveryday resource input: ").Append(selectedFactoryType.resourceInput);

            descriptionText.text = sb.ToString();
           
            buildButton.interactable = selectedFactoryType.conditionsBuild.isAllTrue(Game.player, out buildButton.GetComponentInChildren<ToolTipHandler>().tooltip);
            if (!Game.selectedProvince.CanBuildNewFactory(selectedFactoryType))
                buildButton.interactable = false;
            if (buildButton.interactable)
                buildButton.GetComponentInChildren<Text>().text = "Build " + selectedFactoryType;
        }
        else
        {
            buildButton.interactable = false;           
            {
                buildButton.GetComponentInChildren<Text>().text = "Select building";
                descriptionText.text = "Select building from left";
            }
            
        }        
        show(false);
    }    
}
