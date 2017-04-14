using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class SampleButton : MonoBehaviour
{

    public Button buttonComponent;
    public Text nameLabel;
    public Image iconImage;
    public Text priceText;
    private System.Object obj;

    //private UnityEngine.Events.UnityAction call;

    //private string item;
    private MyTable scrollList;
    //private PopUnit popUnit;
    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.AddListener(HandleClick);
        
    }

    //public void Setup(string text, PopUnit ipopUnit, MyTable currentScrollList)
    public void Setup(string text, MyTable currentScrollList, System.Object pr)
    {
        //item = currentItem;
        obj = pr;
        nameLabel.text = text; // item.name;
                               //call = incall;  UnityEngine.Events.UnityAction incall,
                               //popUnit = ipopUnit; // currentItem.popUnit;
                               //iconImage.sprite = item.icon;
                               // priceText.text = item.price.ToString();

        scrollList = currentScrollList;

    }
    private void HandleClick()
    {
        if (obj is Factory)
        {
            MainCamera.factoryPanel.Show((Factory)obj);
            MainCamera.factoryPanel.refresh();
        }
        else if (obj is PopUnit)
        {
            MainCamera.popUnitPanel.show((PopUnit)obj);
            MainCamera.popUnitPanel.refresh();
        }
        else if (obj is Product)
        {
            MainCamera.goodsPanel.Show((Product)obj, true);
            MainCamera.goodsPanel.refresh();
        }
        else if (obj is InventionType)
        {
            MainCamera.inventionsPanel.selectedInvention = (InventionType)obj;
            MainCamera.inventionsPanel.refresh();
        }
        else if (obj is FactoryType)
        {
            MainCamera.buildPanel.selectedFactoryType = (FactoryType)obj;
            MainCamera.buildPanel.refresh();
        }
        else if (obj is AbstractReform)
        {
            MainCamera.politicsPanel.selectedReform = (AbstractReform)obj;
            MainCamera.politicsPanel.refresh(true);
            //MainCamera.politicsPanel.selectedReformValue = null;
        }



    }
    //internal void Setup(string text, UnityEngine.Events.UnityAction handleClick, ProductionWindowTable productionWindowTable, Factory stor)
    //{
    //    Setup( text,  handleClick, productionWindowTable);
    //}
}