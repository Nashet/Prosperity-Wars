using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class FinancePanel : DragPanel
{
    public Text expensesText, captionText, incomeText, bankText, loanLimitText, depositLimitText, AutoPutInBankText,
        totalText;
    public Slider loanLimit, depositLimit, autoPutInBankLimit;
    public Toggle autoSendMoneyToBank;
    public CanvasGroup loanPanel, depositPanel;
    StringBuilder sb = new StringBuilder();
    // Use this for initialization
    void Start()
    {
        MainCamera.financePanel = this;
        hide();
    }

    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh()
    {
        sb.Clear();
        sb.Append("Finances of ").Append(Game.player);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Income:");
        sb.Append("\n Poor tax (").Append(Game.player.taxationForPoor.getValue()).Append("): ").Append(Game.player.getPoorTaxIncome());
        sb.Append("\n Rich tax (").Append(Game.player.taxationForRich.getValue()).Append("): ").Append(Game.player.getRichTaxIncome());
        sb.Append("\n Gold mines: ").Append(Game.player.getGoldMinesIncome());
        sb.Append("\n Owned enterprises: ").Append(Game.player.getOwnedFactoriesIncome());
        sb.Append("\nTotal: ").Append(Game.player.moneyIncomethisTurn);

        sb.Append("\n\nBalance: ").Append(Game.player.getBalance());
        sb.Append("\nHave money: ").Append(Game.player.cash).Append(" + ").Append(Game.player.deposits).Append(" on bank deposit");
        sb.Append("\nGDP (current prices): ").Append(Game.player.getGDP()).Append("; GDP per thousand men: ").Append(Game.player.getGDPPer1000());
        incomeText.text = sb.ToString();
        //sb.Append("\nScreen resolution: ").Append(Screen.currentResolution).Append(" Canvas size: ").Append(MainCamera.topPanel.transform.parent.GetComponentInParent<RectTransform>().rect);

        //sb.Clear();
        //sb.Append("Balance: ").Append(Game.player.getCountryWallet().getBalance());
        //sb.Append("\nHave money: ").Append(Game.player.wallet.haveMoney).Append(" + ").Append(Game.player.deposits).Append(" on bank deposit");
        ////sb.Append("\nGDP (current prices): ").Append(Game.player.getGDP()).Append("; GDP per thousand men: ").Append(Game.player.getGDPPer1000());
        //totalText.text = sb.ToString();

        sb.Clear();
        sb.Append("Expenses: ");
        sb.Append("\n Unemployment subsidies: ").Append(Game.player.getUnemploymentSubsidiesExpense())
            .Append(" unemployment: ").Append(Game.player.getUnemployment());
        sb.Append("\n Enterprises subsidies: ").Append(Game.player.getfactorySubsidiesExpense());
        sb.Append("\n Storage buying: ").Append(Game.player.getStorageBuyingExpense());
        sb.Append("\nTotal: ").Append(Game.player.getAllExpenses());
        expensesText.text = sb.ToString();

        sb.Clear();
        sb.Append("Loans taken: ").Append(Game.player.loans);
        sb.Append("\nNational bank: ").Append(Game.player.bank).Append(" loans: ").Append(Game.player.bank.getGivenLoans());
        //sb.Append(Game.player.bank).Append(" deposits: ").Append(Game.player.bank.getGivenLoans());
        sb.Append("\nTotal gold (in world): ").Append(Game.getAllMoneyInWorld());
        bankText.text = sb.ToString();

        onLoanLimitChange();
        onDepositLimitChange();
        AutoPutInBankText.text = Game.player.autoPutInBankLimit.ToString();
        // loanPanel.interactable = Country.condCanTakeLoan.isAllTrue(Game.player, out loanPanel.GetComponentInChildren<ToolTipHandler>().tooltip);
        //depositPanel.interactable = Country.condCanPutOnDeposit.isAllTrue(Game.player, out depositPanel.GetComponentInChildren<ToolTipHandler>().tooltip);
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
                if (fact.factoryOwner == null)
                    new Message("", "Null owner in " + item + " " + fact, "Got it");
                else
                if (fact.factoryOwner is PopUnit)
                {
                    var c = fact.factoryOwner as PopUnit;
                    if (c.getPopulation() == 0)
                        new Message("", "Dead pop owner in " + item + " " + fact, "Got it"); ;
                }
                else
                if (fact.factoryOwner is Country)
                {
                    var c = fact.factoryOwner as Country;
                    if (!c.isExist())
                        new Message("", "Dead country owner in " + item + " " + fact, "Got it"); ;
                }
            }
        }
    }
    public void onTakeLoan()
    {
        Game.player.bank.giveMoney(Game.player, new Value(Game.player.bank.getReservs() * loanLimit.value));
        refresh();
    }
    public void onPutInDeposit()
    {
        Game.player.bank.takeMoney(Game.player, new Value(Game.player.cash.get() * depositLimit.value));
        refresh();
    }
    public void onLoanLimitChange()
    {
        loanLimitText.text = (Game.player.bank.getReservs() * loanLimit.value).ToString();
    }

    public void onDepositLimitChange()
    {
        depositLimitText.text = (Game.player.cash.get() * depositLimit.value).ToString();
    }
    public void onAutoPutInBankLimitChange()
    {
        Game.player.autoPutInBankLimit = (int)autoPutInBankLimit.value;
        AutoPutInBankText.text = Game.player.autoPutInBankLimit.ToString();
    }
    public void onAutoSendMoneyToBankToggleChange()
    {
        autoPutInBankLimit.interactable = autoSendMoneyToBank.isOn;
        if (!autoSendMoneyToBank.isOn)
        {
            Game.player.autoPutInBankLimit = 0;
            autoPutInBankLimit.value = 0f;
            AutoPutInBankText.text = Game.player.autoPutInBankLimit.ToString();
        }
        
    }
}
