using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class MilitaryPanel : DragPanel
{
    public Dropdown ddProvinceSelect;
    public Text allArmySizeText, captionText, sendingArmySizeText;
    public Slider armySendLimit;
    public Button sendArmy;
    StringBuilder sb = new StringBuilder();

    List<Province> availableProvinces = new List<Province>();
    Army virtualArmyToSend;
    // Use this for initialization
    void Start()
    {
        MainCamera.militaryPanel = this;
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
            //Game.player.homeArmy.balance(Game.player.sendingArmy, new Procent(armySendLimit.value));
            armySendLimit.value = 0; // cause extra mobilization
            rebuildDropDown();
        }
        sb.Clear();
        sb.Append("Military of ").Append(Game.Player);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Have army: ").Append(Game.Player.getDefenceForces().getShortName());
        allArmySizeText.text = sb.ToString();

        if (virtualArmyToSend == null)
            virtualArmyToSend = new Army(Game.Player);
        sb.Clear();
        sb.Append("Sending army: ").Append(virtualArmyToSend.getShortName());
        sendingArmySizeText.text = sb.ToString();
        //sendArmy.interactable = virtualArmyToSend == "0" ? false : true;
        sendArmy.interactable = virtualArmyToSend.getSize() > 0 ? true : false;

    }

    public void show(Province province)
    {
        gameObject.SetActive(true);
        panelRectTransform.SetAsLastSibling();

        refresh(true);
        if (province != null)
        {
            var list = Game.Player.getNeighborProvinces();
            ddProvinceSelect.value = list.IndexOf(province);
        }
    }

    public void onMobilizationClick()
    {
        //if (Game.Player.homeArmy.getSize() == 0)
        //  Game.Player.homeArmy = new Army(Game.Player);
        Game.Player.mobilize(Game.Player.ownedProvinces);
        //onArmyLimitChanged(0f);
        //MainCamera.tradeWindow.refresh();
        refresh(false);
    }
    public void onDemobilizationClick()
    {
        Game.Player.demobilize();
        virtualArmyToSend.demobilize();
        //MainCamera.tradeWindow.refresh();
        refresh(false);
    }
    public void onSendArmyClick()
    {
        //Game.Player.sendArmy(Game.Player.sendingArmy, availableProvinces[ddProvinceSelect.value]);
        Game.Player.sendArmy(availableProvinces[ddProvinceSelect.value], new Procent(armySendLimit.value));
        //virtualArmyToSend = new Army(null);
        //Game.Player.sendingArmy = new Army(Game.Player);
        refresh(false);
    }
    void rebuildDropDown()
    {
        ddProvinceSelect.interactable = true;
        ddProvinceSelect.ClearOptions();
        byte count = 0;
        availableProvinces.Clear();
        var list = Game.Player.getNeighborProvinces();
        foreach (Province next in list)
        {
            //if (next.isAvailable(Game.player))
            {
                ddProvinceSelect.options.Add(new Dropdown.OptionData() { text = next.ToString() + " (" + next.getCountry() + ")" });
                availableProvinces.Add(next);

                //selectedReformValue = next;
                // selecting non empty option
                ddProvinceSelect.value = count;
                ddProvinceSelect.RefreshShownValue();

                count++;
            }
        }
        //onddProvinceSelectValueChanged(); // need it to set correct caption in DropDown
    }
    public void onddProvinceSelectValueChanged()
    {

    }
    public void onArmyLimitChanged(float value)
    {
        //Game.Player.staff.consolidateArmies();
        //actually creates new army here
        //virtualArmyToSend = (Game.Player.staff.consolidateArmies().getSize() * value).ToString("0");
        virtualArmyToSend = Game.Player.consolidateArmies().balance(new Procent(value));
        //virtualArmyToSend = del.getShortName();
        refresh(false);
    }
}
