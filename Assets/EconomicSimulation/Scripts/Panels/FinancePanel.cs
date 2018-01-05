using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using Nashet.UnityUIUtils;
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
            hide();
        }
        public void refresh()
        {
            sb.Clear();
            sb.Append("Finances of ").Append(Game.Player);
            captionText.text = sb.ToString();

            sb.Clear();
            sb.Append("Income:");
            sb.Append("\n Poor tax (").Append(Game.Player.taxationForPoor.getValue()).Append("): ").Append(Game.Player.getPoorTaxIncome());
            sb.Append("\n Rich tax (").Append(Game.Player.taxationForRich.getValue()).Append("): ").Append(Game.Player.getRichTaxIncome());
            sb.Append("\n Gold mines: ").Append(Game.Player.getGoldMinesIncome());
            sb.Append("\n Owned enterprises: ").Append(Game.Player.getOwnedFactoriesIncome());
            sb.Append("\n Storage sells: ").Append(Game.Player.getCostOfAllSellsByGovernment());
            sb.Append("\n Rest: ").Append(Game.Player.getRestIncome());
            sb.Append("\nTotal: ").Append(Game.Player.moneyIncomethisTurn);

            sb.Append("\n\nBalance: ").Append(Game.Player.getBalance());
            sb.Append("\nHave money: ").Append(Game.Player.cash).Append(" + ").Append(Game.Player.deposits).Append(" on bank deposit");
            sb.Append("\nLoans taken: ").Append(Game.Player.loans);
            //sb.Append("\nGDP (current prices): ").Append(Game.Player.getGDP()).Append("; GDP per thousand men: ").Append(Game.Player.getGDPPer1000());
            incomeText.text = sb.ToString();
            //sb.Append("\nScreen resolution: ").Append(Screen.currentResolution).Append(" Canvas size: ").Append(MainCamera.topPanel.transform.parent.GetComponentInParent<RectTransform>().rect);

            sb.Clear();
            sb.Append("Expenses: ");
            sb.Append("\n Unemployment subsidies: ").Append(Game.Player.getUnemploymentSubsidiesExpense())
                .Append(" unemployment: ").Append(Game.Player.getUnemployment());
            sb.Append("\n Enterprises subsidies: ").Append(Game.Player.getFactorySubsidiesExpense());
            if (Game.Player.isInvented(Invention.ProfessionalArmy))
                sb.Append("\n Soldiers paychecks: ").Append(Game.Player.getSoldiersWageExpense());
            sb.Append("\n Storage buying: ").Append(Game.Player.getStorageBuyingExpense());
            sb.Append("\nTotal: ").Append(Game.Player.getAllExpenses());
            expensesText.text = sb.ToString();

            sb.Clear();

            sb.Append("\nNational bank: ").Append(Game.Player.getBank()).Append(" loans: ").Append(Game.Player.getBank().getGivenLoans());
            //sb.Append(Game.player.bank).Append(" deposits: ").Append(Game.player.bank.getGivenLoans());
            sb.Append("\nTotal gold (in world): ").Append(Game.getAllMoneyInWorld());
            sb.Append("\n*Government and others could automatically take money from deposits");
            bankText.text = sb.ToString();

            onLoanLimitChange();
            onDepositLimitChange();
            //AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
            autoPutInBankLimit.exponentialValue = Game.Player.autoPutInBankLimit;
            if (Game.Player.isInvented(Invention.Banking))
                bankPanel.interactable = true;
            else
            {
                bankPanel.interactable = false;
                autoSendMoneyToBank.isOn = false;
            }
            if (Game.Player.isInvented(Invention.ProfessionalArmy))
            {
                ssSoldiersWage.maxValue = ssSoldiersWage.setValueFunction(Game.market.getCost(PopType.Soldiers.getAllNeedsPer1000()).get() * 2f);
                ssSoldiersWage.exponentialValue = Game.Player.getSoldierWage(); // could be changed by AI
                refreshSoldierWageText();
                ssSoldiersWage.GetComponent<CanvasGroup>().alpha = 1f;
            }
            else
                ssSoldiersWage.GetComponent<CanvasGroup>().alpha = 0f;
        }
        public void show()
        {
            gameObject.SetActive(true);
            panelRectTransform.SetAsLastSibling();
            refresh();
        }

        public void findNoonesEterprises()
        {
            foreach (var item in Province.allProvinces)
            {
                foreach (var fact in item.allFactories)
                {
                    if (fact.getOwner() == null)
                        new Message("", "Null owner in " + item + " " + fact, "Got it");
                    else
                    if (fact.getOwner() is PopUnit)
                    {
                        var owner = fact.getOwner() as PopUnit;
                        if (!owner.isAlive())
                            new Message("", "Dead pop owner in " + item + " " + fact, "Got it"); ;
                    }
                    else
                    if (fact.getOwner() is Country)
                    {
                        var c = fact.getOwner() as Country;
                        if (!c.isAlive())
                            new Message("", "Dead country owner in " + item + " " + fact, "Got it"); ;
                    }
                }
            }
        }
        public void onTakeLoan()
        {
            Value loan = Game.Player.getBank().howMuchCanGive(Game.Player);
            if (loanLimit.value != 1f)
                loan.multiply(loanLimit.value);
            if (Game.Player.getBank().canGiveMoney(Game.Player, loan))
                Game.Player.getBank().giveMoney(Game.Player, loan);
            MainCamera.refreshAllActive();
        }
        public void onPutInDeposit()
        {
            if (loanLimit.value == 1f)
                Game.Player.getBank().takeMoney(Game.Player, new Value(Game.Player.cash));
            else
                Game.Player.getBank().takeMoney(Game.Player, Game.Player.cash.multiplyOutside(depositLimit.value));
            MainCamera.refreshAllActive();
        }
        public void onLoanLimitChange()
        {
            loanLimitText.text = Game.Player.getBank().howMuchCanGive(Game.Player).multiplyOutside(loanLimit.value).ToString();
        }

        public void onDepositLimitChange()
        {
            depositLimitText.text = Game.Player.cash.multiplyOutside(depositLimit.value).ToString();
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