using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class DiplomacyPanel : DragPanel
{

    public Dropdown ddProvinceSelect;
    public GameObject diplomacyPanel;
    public Text allArmySizeText, captionText, sendingArmySizeText;
    StringBuilder sb = new StringBuilder();
    
    List<Province> availableProvinces = new List<Province>();
    // Use this for initialization
    void Start()
    {
        MainCamera.diplomacyPanel = this;
        hide();

    }

    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh(bool rebuildDropdown)
    {

        if (rebuildDropdown)
        {
            rebuildDropDown();            
        }
        sb.Clear();
        sb.Append("Diplomacy of ").Append(Game.player);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Have army: ").Append(Game.player.homeArmy);
        //sb.Append("\n Poor tax: ").Append(Game.player.getCountryWallet().getPoorTaxIncome());

        allArmySizeText.text = sb.ToString();

        sb.Clear();
        sb.Append("Sending army: ").Append(Game.player.sendingArmy);
        //sb.Append("\n Poor tax: ").Append(Game.player.getCountryWallet().getPoorTaxIncome());

        sendingArmySizeText.text = sb.ToString();



        //sb.Clear();
        //sb.Append("Expenses: ");
        //sb.Append("\n Unemployment subsidies: ").Append(Game.player.getCountryWallet().getUnemploymentSubsidiesExpense());
        //sb.Append("\n Enterprises subsidies: ").Append(Game.player.getCountryWallet().getfactorySubsidiesExpense());
        //sendingArmySizeText.text = sb.ToString();

    }


    public void show()
    {
        diplomacyPanel.SetActive(true);

        panelRectTransform.SetAsLastSibling();
        refresh(true);
    }
    public void hide()
    {
        diplomacyPanel.SetActive(false);
    }
    public void onCloseClick()
    {
        hide();
    }
    public void onMobilizationClick()
    {
        Game.player.mobilize();
        //onArmyLimitChanged(0f);
        refresh(false);
    }
    public void onDemobilizationClick()
    {
        Game.player.demobilize();
        refresh(false);
    }
    public void onSendArmyClick()
    {
        Game.player.sendArmy(Game.player.sendingArmy, availableProvinces[ddProvinceSelect.value]);
        Game.player.sendingArmy = new Army(Game.player);
        refresh(false);
    }
    void rebuildDropDown()
    {
        ddProvinceSelect.interactable = true;
        ddProvinceSelect.ClearOptions();
        byte count = 0;
        availableProvinces.Clear();
        var list = Game.player.getNeighborProvinces();
        foreach (Province next in list)
        {
            //if (next.isAvailable(Game.player))
            {
                ddProvinceSelect.options.Add(new Dropdown.OptionData() { text = next.ToString() + " (" + next.getOwner() + ")" });
                availableProvinces.Add(next);

                //selectedReformValue = next;
                // selecting non empty option
                ddProvinceSelect.value = count;
                ddProvinceSelect.RefreshShownValue();

                count++;
            }
        }
        onddProvinceSelectValueChanged(); // need it to set correct caption in DropDown
    }
    public void onddProvinceSelectValueChanged()
    {
        ////if current reform does not contain reform value
        //bool contain = false;
        //foreach (AbstractReformValue reformValue in selectedReform)
        //{
        //    if (reformValue == selectedReformValue) contain = true;
        //}
        //if (!contain)
        {
            //selectedReformValue = selectedReform.getValue(assotiateTable[choise.value]);
            //selectedReformValue = assotiateTable[ddProvinceSelect.value];
            //refresh(false);
            //refresh();
        }
    }
    public void onArmyLimitChanged(float value)
    {
        /// Army split = new Army();
        Game.player.homeArmy.balance(Game.player.sendingArmy, new Procent(value));
        //if (split != null)
        //{
        //    sendingArmy.add(split);
        refresh(false);
        //}

    }
}
