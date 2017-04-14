using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class BuildPanel : DragPanel
{
    public GameObject buildPanel;
    public ScrollRect table;
    public Text descriptionText;
    public Button buildButton;
    public FactoryType selectedFactoryType;
    //Province province;
    // Use this for initialization
    void Start()
    {
        MainCamera.buildPanel = this;
        buildButton.interactable = false;
        hide();
    }
    public void hide()
    {
        buildPanel.SetActive(false);
        //todo add button removal?      
    }
    public void show(bool bringOnTop)
    {
        buildPanel.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();

    }
    public void onCloseClick()
    {
        hide();

    }


    public void onBuildClick()
    {
        //if (Game.player.economy.allowsFactoryBuildingByGovernment())
        {
            bool buildSomething = false;
            var resourceToBuild = selectedFactoryType.getBuildNeeds();
            if (Game.player.economy.isMarket())
            //if (Game.player.economy.status == Economy.StateCapitalism)
            //have money /resourse
            {
                Value cost = Game.market.getCost(resourceToBuild);
                cost.add(Game.factoryMoneyReservPerLevel);
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
            descriptionText.text = "Build " + selectedFactoryType
                + "\n\nResouces to build: " + selectedFactoryType.getBuildNeeds()
                + "\nEveryday resource input: " + selectedFactoryType.resourceInput
                ;

            //if (Game.player.isInvented(InventionType.capitalism))
            //if (Game.player.economy.status != Economy.LaissezFaire)
            //{
            //    PrimitiveStorageSet resourceToBuild;

            //    resourceToBuild = selectedFactoryType.getBuildNeeds();

            //    // money / resource enough
            //    Storage needFood = resourceToBuild.findStorage(Product.Food);

            //    if (Game.player.economy.isMarket())
            //    {
            //        // todo refactor mirroring
            //        Value cost = Game.market.getCost(resourceToBuild);
            //        cost.add(Game.factoryMoneyReservPerLevel);
            //        if (Game.player.wallet.canPay(cost))
            //        {
            //            buildButton.interactable = true;
            //            buildButton.GetComponentInChildren<Text>().text = "Build " + selectedFactoryType;
            //        }
            //        else
            //        {
            //            buildButton.interactable = false;
            //            buildButton.GetComponentInChildren<Text>().text = "Not enough money";
            //        }
            //    }
            //    else //non market
            //    {
            //        if (Game.player.storageSet.has(needFood))
            //        {
            //            buildButton.interactable = true;
            //            buildButton.GetComponentInChildren<Text>().text = "Build " + selectedFactoryType;
            //        }
            //        else
            //        {
            //            buildButton.interactable = false;
            //            buildButton.GetComponentInChildren<Text>().text = "Not enough materails";
            //        }

            //    }
            //}
            //else
            //{
            //    buildButton.interactable = false;
            //    buildButton.GetComponentInChildren<Text>().text = "Can't built it with Laissez faire";
            //}
            //////
            //selectedFactoryType
            buildButton.interactable = selectedFactoryType.conditionsBuild.isAllTrue(Game.player, out buildButton.GetComponentInChildren<ToolTipHandler>().tooltip);
            if (!Game.selectedProvince.CanBuildNewFactory(selectedFactoryType))
                buildButton.interactable = false;
            if (buildButton.interactable)
                buildButton.GetComponentInChildren<Text>().text = "Build " + selectedFactoryType;

        }
        else
        {
            buildButton.interactable = false;
            //if (Game.player.economy.status == Economy.LaissezFaire)
            //{
            //    buildButton.GetComponentInChildren<Text>().text = "Can't built it with Laissez faire";
            //    descriptionText.text = "";
            //}
            //else
            {
                buildButton.GetComponentInChildren<Text>().text = "Select building";
                descriptionText.text = "Select building from left";
            }
            
        }

        
        show(false);
    }    
}
