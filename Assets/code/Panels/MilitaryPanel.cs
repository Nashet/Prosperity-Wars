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
            //armySendLimit.value = 0; // cause extra mobilization
            rebuildDropDown();            
        }
        sb.Clear();
        sb.Append("Military of ").Append(Game.player);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Have army: ").Append(Game.player.homeArmy.getShortName());
        

        allArmySizeText.text = sb.ToString();

        sb.Clear();
        sb.Append("Sending army: ").Append(Game.player.sendingArmy);        

        sendingArmySizeText.text = sb.ToString();

        sendArmy.interactable = Game.player.sendingArmy.getSize() > 0 ? true : false;
        //armySendLimit.interactable = Game.player.homeArmy.getSize() > 0 ? true : false;
    }

    public void show()
    {
        gameObject.SetActive(true);

        panelRectTransform.SetAsLastSibling();
        refresh(true);
    }
  
    public void onMobilizationClick()
    {
        Game.player.mobilize();
        //onArmyLimitChanged(0f);
        //MainCamera.tradeWindow.refresh();
        refresh(false);
    }
    public void onDemobilizationClick()
    {
        Game.player.demobilize();
        //MainCamera.tradeWindow.refresh();
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
                ddProvinceSelect.options.Add(new Dropdown.OptionData() { text = next.ToString() + " (" + next.getCountry() + ")" });
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
      
    }
    public void onArmyLimitChanged(float value)
    {
      
        Game.player.homeArmy.balance(Game.player.sendingArmy, new Procent(value));      
        refresh(false);
       

    }
}
