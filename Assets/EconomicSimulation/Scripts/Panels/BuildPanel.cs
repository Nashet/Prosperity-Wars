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
            Factory factory;
            if (Economy.isMarket.checkIfTrue(Game.Player))
            {
                Value cost = selectedFactoryType.GetBuildCost();
                if (Game.Player.canPay(cost))
                {
                    factory = Game.selectedProvince.BuildFactory(Game.Player, selectedFactoryType, cost);
                    Game.Player.payWithoutRecord(factory, cost);
                    buildSomething = true;
                    MainCamera.factoryPanel.show(factory);
                    if (Game.Player != factory.GetCountry())
                        factory.GetCountry().changeRelation(Game.Player, Options.RelationImpactOnGovernmentInvestment.get());
                }

            }
            else // non market
            {
                //todo remove grain connection
                var resourceToBuild = selectedFactoryType.GetBuildNeeds();
                Storage needFood = resourceToBuild.GetFirstSubstituteStorage(Product.Grain);
                if (Game.Player.countryStorageSet.has(needFood))
                {
                    factory = Game.selectedProvince.BuildFactory(Game.Player, selectedFactoryType, Game.market.getCost(resourceToBuild));
                    Game.Player.countryStorageSet.Subtract(needFood);
                    buildSomething = true;
                    MainCamera.factoryPanel.show(factory);
                    if (Game.Player != factory.GetCountry())
                        factory.GetCountry().changeRelation(Game.Player, Options.RelationImpactOnGovernmentInvestment.get());
                }
            }

            if (buildSomething == true)
            {
                selectedFactoryType = null;
                MainCamera.refreshAllActive();
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
                sb.Append("\n\nResources to build: ").Append(selectedFactoryType.GetBuildNeeds().getString(", "));
                sb.Append(".");
                if (Game.Player.economy.getValue() != Economy.PlannedEconomy)
                {
                    var cost = selectedFactoryType.GetBuildCost();
                    sb.Append(" cost: ").Append(cost);
                }
                sb.Append("\nEveryday resource input: ");
                if (selectedFactoryType.resourceInput == null)
                    sb.Append("none");
                else
                    sb.Append(selectedFactoryType.resourceInput);

                descriptionText.text = sb.ToString();


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
                else if (FactoryType.getAllInventedTypes(Game.Player).Where(x => x.canBuildNewFactory(Game.selectedProvince, Game.Player)).Count() == 0)
                    descriptionText.text = "Nothing to build now";
                else
                    descriptionText.text = "Select building from left";
            }
        }
    }
}