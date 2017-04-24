using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class FinancePanel : DragPanel
{


    public GameObject financePanel;
    public Text expensesText, captionText, incomeText;
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
        sb.Append("Financef of ").Append(Game.player);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Income:");
        sb.Append("\n Poor tax:").Append(Game.player.wallet.poorTaxIncome);
        sb.Append("\n Rich tax:").Append(Game.player.wallet.richTaxIncome);
        sb.Append("\n Gold mines:").Append(Game.player.wallet.goldMinesIncome);
        sb.Append("\n Owned interprises:").Append(Game.player.wallet.goldMinesIncome);
        incomeText.text = sb.ToString();

        sb.Clear();
        sb.Append("Expenses:");
        sb.Append("\n Unemployment subsidies:").Append(Game.player.wallet.unemploymentSubsidiesExpense);
        expensesText.text = sb.ToString();
    }
    public void show()
    {
        financePanel.SetActive(true);

        panelRectTransform.SetAsLastSibling();
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
