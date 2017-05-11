using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class FinancePanel : DragPanel
{


    public GameObject financePanel;
    public Text expensesText, captionText, incomeText, bankText;
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
        sb.Append("\n Poor tax: ").Append(Game.player.getCountryWallet().getPoorTaxIncome());
        sb.Append("\n Rich tax: ").Append(Game.player.getCountryWallet().getRichTaxIncome());
        sb.Append("\n Gold mines: ").Append(Game.player.getCountryWallet().getGoldMinesIncome());
        sb.Append("\n Owned enterprises: ").Append(Game.player.getCountryWallet().getOwnedFactoriesIncome());
        sb.Append("\nTotal: ").Append(Game.player.wallet.moneyIncomethisTurn);

        sb.Append("\n\nBalance: ").Append(Game.player.getCountryWallet().getBalance());
        //sb.Append("\nScreen resolution: ").Append(Screen.currentResolution).Append(" Canvas size: ").Append(MainCamera.topPanel.transform.parent.GetComponentInParent<RectTransform>().rect);

        sb.Append("\n\nHave money: ").Append(Game.player.wallet.haveMoney).Append (" + some money in bank");
        incomeText.text = sb.ToString();



        sb.Clear();
        sb.Append("Expenses: ");
        sb.Append("\n Unemployment subsidies: ").Append(Game.player.getCountryWallet().getUnemploymentSubsidiesExpense());
        sb.Append("\n Enterprises subsidies: ").Append(Game.player.getCountryWallet().getfactorySubsidiesExpense());
        expensesText.text = sb.ToString();

        sb.Clear();
        sb.Append("Auto send extra money to bank - yes");
        sb.Append("\nBank reserves: ").Append(Game.player.bank).Append(" Bank loans: ").Append(Game.player.bank.getGivenLoans());
        sb.Append("\n\nTotal gold (in world): ").Append(Game.getAllMoneyInWorld());
        bankText.text = sb.ToString();
    }
    public void show()
    {
        financePanel.SetActive(true);

        panelRectTransform.SetAsLastSibling();
        refresh();
    }
    public void hide()
    {
        financePanel.SetActive(false);
    }
    public void onCloseClick()
    {
        hide();
    }


}
