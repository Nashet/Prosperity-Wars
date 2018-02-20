using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class FinancePanel : DragPanel
    {
        [SerializeField]
        private Text expensesText, captionText, incomeText, bankText, loanLimitText, depositLimitText, AutoPutInBankText,
            totalText;

        [SerializeField]
        private Slider loanLimit, depositLimit;

        [SerializeField]
        private SliderExponential ssSoldiersWage, autoPutInBankLimit;

        [SerializeField]
        private Toggle autoSendMoneyToBank;

        [SerializeField]
        private CanvasGroup bankPanel;

        private readonly StringBuilder sb = new StringBuilder();
        // Use this for initialization
        void Start()
        {
            MainCamera.financePanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, 150f);
            //ssSoldiersWage.setExponential(x => 0.001f * Mathf.Pow(x, 1.5f), f => Mathf.Pow(f / 0.001f, 1f / 1.5f));
            ssSoldiersWage.setExponential(x => Mathf.Pow(x, 5f), f => Mathf.Pow(f, 1f / 5f));
            autoPutInBankLimit.setExponential(x => Mathf.Pow(x, 2f), f => Mathf.Pow(f, 1f / 2f));
            Hide();
        }
        public override void Refresh()
        {
            sb.Clear();
            sb.Append("Finances of ").Append(Game.Player);
            captionText.text = sb.ToString();

            sb.Clear();
            sb.Append("Income:");
            sb.Append("\n Income tax for Poor (").Append(Game.Player.taxationForPoor.getValue()).Append("): ").Append(Game.Player.IncomeTaxStaticticPoor);
            sb.Append("\n Income tax for Rich (").Append(Game.Player.taxationForRich.getValue()).Append("): ").Append(Game.Player.IncomeTaxStatisticRich);
            sb.Append("\n Income tax for Foreigners (").Append(Game.Player.taxationForRich.getTypedValue().tax).Append("): ").Append(Game.Player.IncomeTaxForeigner);
            sb.Append("\n Gold mines: ").Append(Game.Player.GoldMinesIncome);
            sb.Append("\n Dividends: ").Append(Game.Player.OwnedFactoriesIncome);
            sb.Append("\n Storage sells: ").Append(Game.Player.getCostOfAllSellsByGovernment());
            sb.Append("\n Rest: ").Append(Game.Player.RestIncome);
            sb.Append("\nTotal: ").Append(Game.Player.moneyIncomeThisTurn);

            sb.Append("\n\nBalance: ").Append(Game.Player.getBalance());
            sb.Append("\nHave money: ").Append(Game.Player.Cash).Append(" + ").Append(Game.Player.deposits).Append(" in bank");
            sb.Append("\nLoans taken: ").Append(Game.Player.loans);
            //sb.Append("\nGDP (current prices): ").Append(Game.Player.getGDP()).Append("; GDP per thousand men: ").Append(Game.Player.getGDPPer1000());
            incomeText.text = sb.ToString();
            //sb.Append("\nScreen resolution: ").Append(Screen.currentResolution).Append(" Canvas size: ").Append(MainCamera.topPanel.transform.parent.GetComponentInParent<RectTransform>().rect);

            sb.Clear();
            sb.Append("Expenses: ");
            sb.Append("\n Unemployment subsidies: ").Append(Game.Player.UnemploymentSubsidiesExpense)
                .Append(" unemployment: ").Append(Game.Player.GetAllPopulation().GetAverageProcent(x => x.getUnemployment()));
            sb.Append("\n Enterprises subsidies: ").Append(Game.Player.FactorySubsidiesExpense);
            if (Game.Player.Invented(Invention.ProfessionalArmy))
                sb.Append("\n Soldiers paychecks: ").Append(Game.Player.SoldiersWageExpense);
            sb.Append("\n Storage buying: ").Append(Game.Player.StorageBuyingExpense);
            sb.Append("\nTotal: ").Append(Game.Player.getExpenses());
            expensesText.text = sb.ToString();

            sb.Clear();

            sb.Append("\n").Append(Game.Player.Bank).Append(" - reserves: ").Append(Game.Player.Bank.Cash)
                .Append("; loans: ").Append(Game.Player.Bank.GetGivenCredits());
            //sb.Append(Game.player.bank).Append(" deposits: ").Append(Game.player.bank.getGivenLoans());
            sb.Append("\nTotal gold (in world): ").Append(World.GetAllMoney());
            sb.Append("\n*Government and others could automatically take money from deposits");
            bankText.text = sb.ToString();

            onLoanLimitChange();
            onDepositLimitChange();
            //AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
            autoPutInBankLimit.exponentialValue = Game.Player.autoPutInBankLimit;
            if (Game.Player.Invented(Invention.Banking))
                bankPanel.interactable = true;
            else
            {
                bankPanel.interactable = false;
                autoSendMoneyToBank.isOn = false;
            }
            if (Game.Player.Invented(Invention.ProfessionalArmy))
            {
                ssSoldiersWage.maxValue = ssSoldiersWage.ConvertToSliderFormat(Game.market.getCost(PopType.Soldiers.getAllNeedsPer1000Men()).get() * 2f);
                ssSoldiersWage.exponentialValue = Game.Player.getSoldierWage(); // could be changed by AI
                refreshSoldierWageText();
                //ssSoldiersWage.GetComponent<CanvasGroup>().alpha = 1f;
                //ssSoldiersWage.enabled = true;
                ssSoldiersWage.gameObject.SetActive(true);
            }
            else
                //ssSoldiersWage.GetComponent<CanvasGroup>().alpha = 0f;
                //ssSoldiersWage.enabled = false;
                ssSoldiersWage.gameObject.SetActive(false);
            if (Game.Player.economy.getValue() == Economy.PlannedEconomy)
            {
                ssSoldiersWage.enabled = false;
                ssSoldiersWage.GetComponent<ToolTipHandler>().SetText("With Planned Economy soldiers take products from country stockpile");
            }
            else
            {
                ssSoldiersWage.enabled = true;
                ssSoldiersWage.GetComponent<ToolTipHandler>().SetText("");
            }
        }
        //public void findNoonesEterprises()
        //{
        //    foreach (var item in Province.allProvinces)
        //    {
        //        foreach (var fact in item.allFactories)
        //        {
        //            if (fact.getOwner() == null)
        //                new Message("", "Null owner in " + item + " " + fact, "Got it");
        //            else
        //            if (fact.getOwner() is PopUnit)
        //            {
        //                var owner = fact.getOwner() as PopUnit;
        //                if (!owner.isAlive())
        //                    new Message("", "Dead pop owner in " + item + " " + fact, "Got it"); ;
        //            }
        //            else
        //            if (fact.getOwner() is Country)
        //            {
        //                var c = fact.getOwner() as Country;
        //                if (!c.isAlive())
        //                    new Message("", "Dead country owner in " + item + " " + fact, "Got it"); ;
        //            }
        //        }
        //    }
        //}
        public void onTakeLoan()
        {
            Value loan = Game.Player.Bank.HowBigCreditCanGive(Game.Player).Copy();
            if (loanLimit.value != 1f)
                loan.Multiply(loanLimit.value);
            Game.Player.Bank.GiveCredit(Game.Player, loan);
            MainCamera.refreshAllActive();
        }
        public void onPutInDeposit()
        {
            if (loanLimit.value == 1f)//.Copy()
                Game.Player.Bank.ReceiveMoney(Game.Player, Game.Player.Cash);// Copye some how related to bug with self paying
            else
                Game.Player.Bank.ReceiveMoney(Game.Player, Game.Player.Cash.Copy().Multiply(depositLimit.value));
            MainCamera.refreshAllActive();
        }
        public void onLoanLimitChange()
        {
            loanLimitText.text = Game.Player.Bank.HowBigCreditCanGive(Game.Player).Copy().Multiply(loanLimit.value).ToString();
        }

        public void onDepositLimitChange()
        {
            depositLimitText.text = Game.Player.Cash.Copy().Multiply(depositLimit.value).ToString();
        }
        private void refreshSoldierWageText()
        {
            sb.Clear();
            sb.Append("Soldiers wage: ").Append(string.Format("{0:N3}", ssSoldiersWage.exponentialValue)).Append(" men: ").Append(Game.Player.getPopulationAmountByType(PopType.Soldiers));
            ssSoldiersWage.GetComponentInChildren<Text>().text = sb.ToString();
        }
        public void onSoldierWageChange()
        {
            refreshSoldierWageText();
            Game.Player.setSoldierWage(ssSoldiersWage.exponentialValue);
        }
        public void onAutoPutInBankLimitChange()
        {
            Game.Player.autoPutInBankLimit = (int)autoPutInBankLimit.exponentialValue;
            AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
        }
        public void onAutoSendMoneyToBankToggleChange()
        {
            autoPutInBankLimit.interactable = autoSendMoneyToBank.isOn;
            if (!autoSendMoneyToBank.isOn)
            {
                Game.Player.autoPutInBankLimit = 0;
                //AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
                autoPutInBankLimit.exponentialValue = Game.Player.autoPutInBankLimit;
            }
        }
    }
}