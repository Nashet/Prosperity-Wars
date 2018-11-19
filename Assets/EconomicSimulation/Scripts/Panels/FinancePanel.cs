using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
        private void Start()
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
            sb.Append("Income: " + Game.Player.Register.Income).Append(Game.Player.Register.GetIncomeText());

            sb.Append("\n\nBalance: ").Append(Money.DecimalToString(Game.Player.Register.Balance));
            sb.Append("\nHave money: ").Append(Game.Player.Cash).Append(" + ").Append(Game.Player.deposits).Append(" in bank");
            sb.Append("\nLoans taken: ").Append(Game.Player.loans);
            //sb.Append("\nGDP (current prices): ").Append(Game.Player.getGDP()).Append("; GDP per thousand men: ").Append(Game.Player.getGDPPer1000());

            if (Game.Player.FailedPayments.Income.isNotZero())
                sb.Append("\n\nFailed to pay: ").Append(Game.Player.FailedPayments.Income).Append(Game.Player.FailedPayments.GetIncomeText());


            incomeText.text = sb.ToString();

            //sb.Append("\nScreen resolution: ").Append(Screen.currentResolution).Append(" Canvas size: ").Append(MainCamera.topPanel.transform.parent.GetComponentInParent<RectTransform>().rect);

            //sb.Clear();
            //sb.Append("Expenses: ");

            //sb.Append("\n Unemployment subsidies: ").Append(Game.Player.UnemploymentSubsidiesExpense)
            //    .Append(" seeking a job: ").Append(Game.Player.Provinces.AllPops.GetAverageProcent(x => x.GetSeekingJob()));

            //if (Game.Player.UBI != UBI.None)
            //    sb.Append("\n Unconditional basic income: ").Append(Game.Player.UBISubsidiesExpense);

            //if (Game.Player.PovertyAid != PovertyAid.None)
            //    sb.Append("\n Poverty Aid: ").Append(Game.Player.PovertyAidExpense);

            //sb.Append("\n Enterprises subsidies: ").Append(Game.Player.FactorySubsidiesExpense);

            //if (Game.Player.Science.IsInvented(Invention.ProfessionalArmy))
            //    sb.Append("\n Soldiers paychecks: ").Append(Game.Player.SoldiersWageExpense);

            //sb.Append("\n Storage buying: ").Append(Game.Player.StorageBuyingExpense);

            //sb.Append("\nTotal: ").Append(Game.Player.GetRegisteredExpenses());
            //expensesText.text = sb.ToString();
            expensesText.text = Game.Player.Register.GetExpensesText();

            // bank part
            sb.Clear();

            sb.Append("\n").Append(Game.Player.Bank).Append(" - reserves: ").Append(Game.Player.Bank.Cash)
                .Append("; loans: ").Append(Game.Player.Bank.GetGivenCredits());
            //sb.Append(Game.player.bank).Append(" deposits: ").Append(Game.player.bank.getGivenLoans());
            sb.Append("\nTotal gold (in the world): ").Append(World.GetAllMoney());
            sb.Append("\n*Government and others could automatically take money from deposits, 1 gold = 1000 gold bites");
            bankText.text = sb.ToString();

            onLoanLimitChange();
            onDepositLimitChange();
            //AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
            autoPutInBankLimit.exponentialValue = (float)Game.Player.autoPutInBankLimit.Get();
            if (Game.Player.Science.IsInvented(Invention.Banking))
                bankPanel.interactable = true;
            else
            {
                bankPanel.interactable = false;
                autoSendMoneyToBank.isOn = false;
            }
            if (Game.Player.Science.IsInvented(Invention.ProfessionalArmy))
            {
                ssSoldiersWage.maxValue = ssSoldiersWage.ConvertToSliderFormat((float)(Game.Player.market.getCost(PopType.Soldiers.getAllNeedsPer1000Men()).Get() * 2m));
                ssSoldiersWage.exponentialValue = (float)Game.Player.getSoldierWage().Get(); // could be changed by AI
                refreshSoldierWageText();
                //ssSoldiersWage.GetComponent<CanvasGroup>().alpha = 1f;
                //ssSoldiersWage.enabled = true;
                ssSoldiersWage.gameObject.SetActive(true);
            }
            else
                //ssSoldiersWage.GetComponent<CanvasGroup>().alpha = 0f;
                //ssSoldiersWage.enabled = false;
                ssSoldiersWage.gameObject.SetActive(false);
            if (Game.Player.economy == Economy.PlannedEconomy)
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
        //                if (!owner.IsAlive)
        //                    new Message("", "Dead pop owner in " + item + " " + fact, "Got it"); ;
        //            }
        //            else
        //            if (fact.getOwner() is Country)
        //            {
        //                var c = fact.getOwner() as Country;
        //                if (!c.IsAlive)
        //                    new Message("", "Dead country owner in " + item + " " + fact, "Got it"); ;
        //            }
        //        }
        //    }
        //}
        public void onTakeLoan()
        {
            Money loan = Game.Player.Bank.HowBigCreditCanGive(Game.Player).Copy();
            if (loanLimit.value != 1f)
                loan.Multiply((decimal)loanLimit.value);
            Game.Player.Bank.GiveCredit(Game.Player, loan);
            //MainCamera.refreshAllActive();
            UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
        }

        public void onPutInDeposit()
        {
            if (loanLimit.value == 1f)//.Copy()
                Game.Player.Bank.ReceiveMoney(Game.Player, Game.Player.Cash);
            else
                Game.Player.Bank.ReceiveMoney(Game.Player, Game.Player.Cash.Copy().Multiply((decimal)depositLimit.value));
            //MainCamera.refreshAllActive();
            UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
        }

        public void onLoanLimitChange()
        {
            loanLimitText.text = Game.Player.Bank.HowBigCreditCanGive(Game.Player).Copy().Multiply((decimal)loanLimit.value).ToString();
        }

        public void onDepositLimitChange()
        {
            depositLimitText.text = Game.Player.Cash.Copy().Multiply((decimal)depositLimit.value).ToString();
        }

        private void refreshSoldierWageText()
        {
            sb.Clear();
            sb.Append("Soldiers wage: ").Append(string.Format("{0:N3}", ssSoldiersWage.exponentialValue)).Append(" men: ").Append(Game.Player.Provinces.getPopulationAmountByType(PopType.Soldiers));
            ssSoldiersWage.GetComponentInChildren<Text>().text = sb.ToString();
        }

        public void onSoldierWageChange()
        {
            refreshSoldierWageText();
            Game.Player.setSoldierWage(new MoneyView((decimal)ssSoldiersWage.exponentialValue));
        }

        public void onAutoPutInBankLimitChange()
        {
            Game.Player.autoPutInBankLimit.Set(new MoneyView((decimal)autoPutInBankLimit.exponentialValue));
            AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
        }

        public void onAutoSendMoneyToBankToggleChange()
        {
            autoPutInBankLimit.interactable = autoSendMoneyToBank.isOn;
            if (!autoSendMoneyToBank.isOn)
            {
                Game.Player.autoPutInBankLimit.SetZero();
                //AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
                autoPutInBankLimit.exponentialValue = (float)Game.Player.autoPutInBankLimit.Get();
            }
        }
    }
}