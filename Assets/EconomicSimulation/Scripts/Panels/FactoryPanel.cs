using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class FactoryPanel : DragPanel//for dragging
    {
        [SerializeField]
        private Button upgradeButton, reopenButton, destroyButton, buyButton, sellButton, nationalizeButton;
        [SerializeField]
        private Toggle subidize, dontHireOnSubsidies;
        [SerializeField]
        private Slider priority;
        [SerializeField]
        private Text generaltext, efficiencyText, caption, onSaleText, ownership;


        private Factory factory;
        private reopenButtonStatus reopenButtonflag;
        // Use this for initialization
        void Start()
        {
            MainCamera.factoryPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(295f, -50f);
            Hide();
        }

        enum reopenButtonStatus { reopen, close };


        void setGUIElementsAccesability()
        {
            string dynText;
            upgradeButton.interactable = Factory.conditionsUpgrade.isAllTrue(factory, Game.Player, out dynText);
            upgradeButton.GetComponent<ToolTipHandler>().SetTextDynamic(() => dynText + "\n\nUpgrade makes enterprise bigger");

            subidize.interactable = Factory.conditionsSubsidize.isAllTrue(factory, Game.Player, out subidize.GetComponent<ToolTipHandler>().text);

            if (factory.IsOpen)
                reopenButtonflag = reopenButtonStatus.close;
            else
                reopenButtonflag = reopenButtonStatus.reopen;
            if (reopenButtonflag == reopenButtonStatus.close)
            {
                reopenButton.GetComponentInChildren<Text>().text = "Close enterprise";
                reopenButton.interactable = Factory.conditionsClose.isAllTrue(factory, Game.Player, out reopenButton.GetComponent<ToolTipHandler>().text);
            }
            else
            {
                reopenButton.interactable = Factory.conditionsReopen.isAllTrue(factory, Game.Player, out reopenButton.GetComponent<ToolTipHandler>().text);
                reopenButton.GetComponentInChildren<Text>().text = "Reopen";
            }

            destroyButton.interactable = Factory.conditionsDestroy.isAllTrue(factory, Game.Player, out destroyButton.GetComponent<ToolTipHandler>().text);


            nationalizeButton.interactable = Factory.conditionsNatinalize.isAllTrue(factory, Game.Player, out nationalizeButton.GetComponent<ToolTipHandler>().text);
            nationalizeButton.GetComponent<ToolTipHandler>().AddText("\nThat would make owners angry and would reduce your reputation");

            this.priority.interactable = Factory.conditionsChangePriority.isAllTrue(factory, Game.Player, out priority.GetComponent<ToolTipHandler>().text);
            priority.GetComponent<ToolTipHandler>().text += "\n\nHighest priority enterprises get workforce first";

            this.subidize.interactable = Factory.conditionsSubsidize.isAllTrue(factory, Game.Player, out subidize.GetComponent<ToolTipHandler>().text);
            this.dontHireOnSubsidies.interactable = Factory.conditionsDontHireOnSubsidies.isAllTrue(factory, Game.Player, out dontHireOnSubsidies.GetComponent<ToolTipHandler>().text);

            priority.value = factory.getPriority();
            subidize.isOn = factory.isSubsidized();
            dontHireOnSubsidies.isOn = factory.isDontHireOnSubsidies();
        }
        public override void Refresh()
        {
            if (factory != null)
            {
                setGUIElementsAccesability();
                
                caption.text = factory.ToString();
                var sb = new StringBuilder();
                sb = new StringBuilder();
                sb.Append("Workforce: ").Append(factory.getWorkForce());
                sb.Append("\nProduced: ").Append(factory.getGainGoodsThisTurn().ToString());
                sb.Append("\nUnsold: ").Append(factory.storage.ToString());
                sb.Append("\nBasic production: ").Append(factory.getType().basicProduction);
                sb.Append("\nSent to market: ").Append(factory.getSentToMarket());
                sb.Append("\nMoney income: ").Append(factory.moneyIncomethisTurn);
                sb.Append(" Cash: ").Append(factory.cash.ToString());

                sb.Append("\nProfit: ");
                if (Game.Player.economy.getValue() != Economy.PlannedEconomy)
                    sb.Append(factory.getProfit());
                else
                    sb.Append("unknown");
                sb.Append(" Dividends: ").Append(factory.GetDividends());
                if (factory.getType().hasInput())
                {
                    sb.Append("\nInput required: ");
                    foreach (Storage next in factory.getType().resourceInput)
                        sb.Append(next.get() * factory.GetWorkForceFulFilling().get()).Append(" ").Append(next.getProduct()).Append(";");
                }
                if (factory.getConsumed().Count() > 0)
                    sb.Append("\nConsumed: ").Append(factory.getConsumed().ToString()).Append(" Cost: ").Append(Game.market.getCost(factory.getConsumed()));
                //if (Game.devMode)
                //    sb.Append("\nConsumed LT: ").Append(factory.getConsumedLastTurn());

                if (factory.getInputProductsReserve().Count() > 0)
                    sb.Append("\nInput reserves: ").Append(factory.getInputProductsReserve());
                sb.Append("\nInput factor: ").Append(factory.getInputFactor());
                sb.Append("\nSalary (per 1000 men): ").Append(factory.getSalary()).Append(", Salary (total): ").Append(factory.getSalaryCost());


                if (factory.constructionNeeds.Count() > 0)
                    sb.Append("\nUpgrade needs: ").Append(factory.constructionNeeds);

                if (factory.getDaysInConstruction() > 0)
                    sb.Append("\nDays in construction: ").Append(factory.getDaysInConstruction());

                if (factory.getDaysUnprofitable() > 0)
                    sb.Append(" Days unprofitable: ").Append(factory.getDaysUnprofitable());

                if (factory.getDaysClosed() > 0)
                    sb.Append(" Days closed: ").Append(factory.getDaysClosed());

                if (factory.loans.get() > 0f)
                    sb.Append("\nLoan: ").Append(factory.loans.ToString());
                sb.Append("\nAssets value: ").Append(factory.ownership.GetAllAssetsValue());
                sb.Append(", Market value: ").Append(factory.ownership.GetMarketValue());


                sb.Append("\nTotal on sale: ").Append(factory.ownership.GetTotalOnSale());
                //if (Game.devMode)
                //    sb.Append("\nHowMuchHiredLastTurn ").Append(shownFactory.getHowMuchHiredLastTurn());
                generaltext.text = sb.ToString();
                //+"\nExpenses:"

                efficiencyText.text = "Efficiency: " + Factory.modifierEfficiency.getModifier(factory);
                efficiencyText.GetComponent<ToolTipHandler>().SetTextDynamic(()=> "Efficiency: " + Factory.modifierEfficiency.GetDescription(factory));
                

                var owners = factory.ownership.GetAllShares().OrderByDescending(x => x.Value.get()).ToList();//.getString(" ", "\n");
                ownership.text = "Biggest owner: " + owners[0].Key + " " + owners[0].Value + " (hover mouse for rest)";
                ownership.GetComponent<ToolTipHandler>().SetTextDynamic(() =>"Owners:\n"+ owners.getString(" ", "\n"));
                RefreshBuySellButtons();
            }
        }
        public void show(Factory fact)
        {
            factory = fact;
            Show();
        }
        public void removeFactory(Factory fact)
        {
            if (fact == factory)
            {
                factory = null;
                Hide();
            }
        }

        public void onSubsidizeValueChanged()
        {
            factory.setSubsidized(subidize.isOn);
            Refresh();
        }
        public void ondontHireOnSubsidiesValueChanged()
        {
            factory.setDontHireOnSubsidies(dontHireOnSubsidies.isOn);
        }
        public void onPriorityChanged()
        {
            factory.setPriority((int)Mathf.RoundToInt(priority.value));
        }
        public void onReopenClick()
        {
            if (reopenButtonflag == reopenButtonStatus.close)
                factory.close();
            else
                factory.open(Game.Player, true);
            Refresh();
            if (MainCamera.productionWindow.isActiveAndEnabled) MainCamera.productionWindow.Refresh();
            MainCamera.topPanel.Refresh();
            if (MainCamera.financePanel.isActiveAndEnabled) MainCamera.financePanel.Refresh();
        }
        private void RefreshBuySellButtons()
        {
            sellButton.interactable = Factory.conditionsSell.isAllTrue(Game.Player, factory, out sellButton.GetComponent<ToolTipHandler>().text);
            buyButton.interactable = Factory.conditionsBuy.isAllTrue(Game.Player, factory, out buyButton.GetComponent<ToolTipHandler>().text);


            buyButton.GetComponentInChildren<Text>().text = "Buy " + Options.PopBuyAssetsAtTime + " shares";

            var selling = factory.ownership.HowMuchSelling(Game.Player);
            if (selling.isZero())
                sellButton.GetComponentInChildren<Text>().text = "Sell shares";
            else
                sellButton.GetComponentInChildren<Text>().text = "Selling " + selling + " shares";
        }
        public void OnBuyClick()
        {
            factory.ownership.BuyStandardShare(Game.Player);
            MainCamera.refreshAllActive();
        }
        public void OnSellClick()
        {
            factory.ownership.SetToSell(Game.Player, Options.PopBuyAssetsAtTime);
            Refresh();
        }
        public void onUpgradeClick()
        {
            //if (shownFactory.getConditionsForFactoryUpgradeFast(Game.player))
            {
                factory.upgrade(Game.Player);                
                MainCamera.refreshAllActive();                
                if (Game.Player != factory.GetCountry())
                    factory.GetCountry().changeRelation(Game.Player, Options.RelationImpactOnGovernmentInvestment.get());
            }
        }
        public void onDestroyClick()
        {
            //if (shownFactory.whyCantDestroyFactory() == null)
            {
                factory.destroyImmediately();
                MainCamera.refreshAllActive();
            }
        }

        public void onNationalizeClick()
        {
            Game.Player.Nationilize(factory);
            MainCamera.refreshAllActive();
        }
    }
}