using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class BuildPanel : DragPanel
    {
        [SerializeField]
        private BuildPanelTable table;

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
            Hide();
        }

        public override void Show()
        {
            selectedFactoryType = null; // changed province           
            base.Show();
        }
        public void onBuildClick()
        {

            bool buildSomething = false;

            if (Economy.isMarket.checkIftrue(Game.Player))
            //if (Game.player.economy.status == Economy.StateCapitalism)
            //have money /resource
            {
                //Value cost = Game.market.getCost(resourceToBuild);
                //cost.add(Options.factoryMoneyReservePerLevel);
                Value cost = selectedFactoryType.getInvestmentCost();
                if (Game.Player.canPay(cost))
                {
                    var factory = new Factory(Game.selectedProvince, Game.Player, selectedFactoryType, cost);
                    Game.Player.payWithoutRecord(factory, cost);
                    buildSomething = true;
                    MainCamera.factoryPanel.show(factory);
                }

            }
            else // non market
            {
                //todo remove grain connection
                var resourceToBuild = selectedFactoryType.GetBuildNeeds();
                Storage needFood = resourceToBuild.GetFirstSubstituteStorage(Product.Grain);
                if (Game.Player.countryStorageSet.has(needFood))
                {
                    Factory fact = new Factory(Game.selectedProvince, Game.Player, selectedFactoryType, Game.market.getCost(resourceToBuild));
                    //wallet.pay(fact.wallet, new Value(100f));
                    Game.Player.countryStorageSet.Subtract(needFood);
                    buildSomething = true;
                    MainCamera.factoryPanel.show(fact);
                }
            }

            if (buildSomething == true)
            {
                // voteButton.interactable = false;
                MainCamera.refreshAllActive();
                selectedFactoryType = null;
                //table.Refresh();
                Refresh();
            }
        }
        public void selectFactoryType(FactoryType newSelection)
        {
            selectedFactoryType = newSelection;
        }
        public override void Refresh()
        {
            if (Game.previoslySelectedProvince != Game.selectedProvince)
                selectFactoryType(null);

            table.Refresh();
            if (selectedFactoryType != null)
            {
                sb.Clear();
                sb.Append("Build ").Append(selectedFactoryType);
                sb.Append("\n\nResources to build: ").Append(selectedFactoryType.GetBuildNeeds());
                if (Game.Player.economy.getValue() != Economy.PlannedEconomy)
                {
                    var cost = selectedFactoryType.getInvestmentCost();
                    sb.Append(" cost: ").Append(cost);
                }
                sb.Append("\nEveryday resource input: ").Append(selectedFactoryType.resourceInput);

                descriptionText.text = sb.ToString();


                buildButton.interactable = selectedFactoryType.conditionsBuild.isAllTrue(Game.Player, Game.selectedProvince, out buildButton.GetComponent<ToolTipHandler>().text);
                if (!selectedFactoryType.canBuildNewFactory(Game.selectedProvince))
                    buildButton.interactable = false;
                if (buildButton.interactable)
                    buildButton.GetComponentInChildren<Text>().text = "Build " + selectedFactoryType;
            }
            else
            {
                buildButton.interactable = false;
                buildButton.GetComponentInChildren<Text>().text = "Select building";
                if (Game.selectedProvince == null)
                    descriptionText.text = "Select province where to build";
                else if (FactoryType.getAllInventedTypes(Game.Player, x => x.canBuildNewFactory(Game.selectedProvince)).Count() == 0)
                    descriptionText.text = "Nothing to build now";
                else
                    descriptionText.text = "Select building from left";
            }
        }
    }
}