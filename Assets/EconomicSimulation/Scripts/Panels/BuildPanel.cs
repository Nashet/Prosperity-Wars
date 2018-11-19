using System;
using System.Linq;
using System.Text;
using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;
using UnityEngine.UI;

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

        private ProductionType selectedFactoryType;
        private StringBuilder sb = new StringBuilder();

        //Province province;
        // Use this for initialization
        private void Start()
        {
            MainCamera.buildPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -340f);
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
            Factory factory;
            if (Economy.isMarket.checkIfTrue(Game.Player))
            {
                MoneyView cost = selectedFactoryType.GetBuildCost(Game.Player.market);
                if (Game.Player.CanPay(cost))
                {
                    factory = Game.selectedProvince.BuildFactory(Game.Player, selectedFactoryType, cost);
                    Game.Player.PayWithoutRecord(factory, cost, Register.Account.Construction);
                    buildSomething = true;
                    MainCamera.factoryPanel.show(factory);
                    if (Game.Player != factory.Country)
                        factory.Country.Diplomacy.ChangeRelation(Game.Player, Options.RelationImpactOnGovernmentInvestment.get());
                }
            }
            else // non market
            {
                //todo remove grain connection
                var resourceToBuild = selectedFactoryType.GetBuildNeeds();
                Storage needFood = resourceToBuild.GetFirstSubstituteStorage(Product.Grain);
                if (Game.Player.countryStorageSet.has(needFood))
                {
                    factory = Game.selectedProvince.BuildFactory(Game.Player, selectedFactoryType, Game.Player.market.getCost(resourceToBuild));
                    Game.Player.countryStorageSet.Subtract(needFood);
                    buildSomething = true;
                    MainCamera.factoryPanel.show(factory);
                    if (Game.Player != factory.Country)
                        factory.Country.Diplomacy.ChangeRelation(Game.Player, Options.RelationImpactOnGovernmentInvestment.get());
                }
            }

            if (buildSomething)
            {
                selectedFactoryType = null;
                UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
                //MainCamera.refreshAllActive();
            }
        }

        public void selectFactoryType(ProductionType newSelection)
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
                sb.Append("\n\nResources to build: ").Append(selectedFactoryType.GetBuildNeeds().ToString(", "));
                sb.Append(".");
                if (Game.Player.economy != Economy.PlannedEconomy)
                {
                    var cost = selectedFactoryType.GetBuildCost(Game.Player.market);
                    sb.Append(" cost: ").Append(cost);
                }
                sb.Append("\nEveryday resource input: ");
                if (selectedFactoryType.resourceInput == null)
                    sb.Append("none");
                else
                    sb.Append(selectedFactoryType.resourceInput);

                descriptionText.text = sb.ToString();

                // fix that duplicate:
                buildButton.interactable = selectedFactoryType.conditionsBuildThis.isAllTrue(Game.Player, Game.selectedProvince, out buildButton.GetComponent<ToolTipHandler>().text);
                if (!selectedFactoryType.canBuildNewFactory(Game.selectedProvince, Game.Player))
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
                else if (ProductionType.getAllInventedFactories(Game.Player).Where(x => x.canBuildNewFactory(Game.selectedProvince, Game.Player)).Count() == 0)
                    descriptionText.text = "Nothing to build now";
                else
                    descriptionText.text = "Select building from left";
            }
        }
    }
}