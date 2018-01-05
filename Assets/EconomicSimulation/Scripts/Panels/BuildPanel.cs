using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    
    public class BuildPanel : DragPanel
    {
        [SerializeField]
        private ScrollRect table;
        [SerializeField]
        private Text descriptionText;
        [SerializeField]
        private Button buildButton;

        private FactoryType selectedFactoryType;
        StringBuilder sb = new StringBuilder();
        //Province province;
        // Use this for initialization
        void Start()
        {
            MainCamera.buildPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -100f);
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

                if (Economy.isMarket.checkIftrue(Game.Player))
                //if (Game.player.economy.status == Economy.StateCapitalism)
                //have money /resource
                {
                    //Value cost = Game.market.getCost(resourceToBuild);
                    //cost.add(Options.factoryMoneyReservePerLevel);
                    Value cost = selectedFactoryType.getMinimalMoneyToBuild();
                    if (Game.Player.canPay(cost))
                    {
                        var factory = new Factory(Game.selectedProvince, Game.Player, selectedFactoryType);
                        Game.Player.payWithoutRecord(factory, cost);
                        buildSomething = true;
                        MainCamera.factoryPanel.Show(factory);
                    }

                }
                else // non market
                {
                    //todo remove grain connection
                    var resourceToBuild = selectedFactoryType.getBuildNeeds();
                    Storage needFood = resourceToBuild.getFirstStorage(Product.Grain);
                    if (Game.Player.countryStorageSet.has(needFood))
                    {
                        Factory fact = new Factory(Game.selectedProvince, Game.Player, selectedFactoryType);
                        //wallet.pay(fact.wallet, new Value(100f));
                        Game.Player.countryStorageSet.subtract(needFood);
                        buildSomething = true;
                        MainCamera.factoryPanel.Show(fact);
                    }
                }

                if (buildSomething == true)
                {
                    // voteButton.interactable = false;
                    MainCamera.refreshAllActive();
                    selectedFactoryType = null;

                    //Hide();
                    //show();
                    //refresh();
                }
            }
        }
        public void refresh(FactoryType newSelection)
        {
            if (newSelection != null)
                selectedFactoryType = newSelection;
            hide();
            if (selectedFactoryType != null)
            {
                sb.Clear();
                sb.Append("Build ").Append(selectedFactoryType);
                sb.Append("\n\nResources to build: ").Append(selectedFactoryType.getBuildNeeds());
                if (Game.Player.economy.getValue() != Economy.PlannedEconomy)
                {
                    var cost = selectedFactoryType.getMinimalMoneyToBuild();
                    sb.Append(" cost: ").Append(cost);
                }
                sb.Append("\nEveryday resource input: ").Append(selectedFactoryType.resourceInput);

                descriptionText.text = sb.ToString();

                buildButton.interactable = selectedFactoryType.conditionsBuild.isAllTrue(Game.Player, out buildButton.GetComponentInChildren<ToolTipHandler>().text);
                if (!selectedFactoryType.canBuildNewFactory(Game.selectedProvince))
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
}