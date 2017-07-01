using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class SampleButton : MonoBehaviour, IPointerDownHandler
{

    public Button buttonComponent;
    public Text nameLabel;
    public Image iconImage;
    private System.Object link;
    private DragPanel parent;
    private MyTable scrollList;

    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.AddListener(HandleClick);
    }

    //public void Setup(string text, PopUnit ipopUnit, MyTable currentScrollList)
    public void Setup(string text, MyTable currentScrollList, System.Object pr)
    {
        //item = currentItem;
        link = pr;
        nameLabel.text = text; // item.name;
                               //call = incall;  UnityEngine.Events.UnityAction incall,
                               //popUnit = ipopUnit; // currentItem.popUnit;
                               //iconImage.sprite = item.icon;
                               // priceText.text = item.price.ToString();

        scrollList = currentScrollList;
        parent = GetComponentInParent<DragPanel>();
    }
    public void OnPointerDown(PointerEventData data)
    {
        parent.OnPointerDown(data);
    }
    private void HandleClick()
    {


        if (link is Factory)
        {
            MainCamera.factoryPanel.Show((Factory)link);
            MainCamera.factoryPanel.refresh();
        }
        else if (link is PopUnit)
        {
            MainCamera.popUnitPanel.show((PopUnit)link);
            MainCamera.popUnitPanel.refresh();
        }
        else if (link is Product)
        {
            MainCamera.goodsPanel.Show((Product)link, true);
            MainCamera.goodsPanel.refresh();
        }
        else if (link is Invention)
        {
            MainCamera.inventionsPanel.selectedInvention = (Invention)link;
            MainCamera.inventionsPanel.refresh();
        }
        else if (link is FactoryType)
        {
            MainCamera.buildPanel.selectedFactoryType = (FactoryType)link;
            MainCamera.buildPanel.refresh();
        }
        else if (link is AbstractReform)
        {
            MainCamera.politicsPanel.selectedReform = (AbstractReform)link;
            MainCamera.politicsPanel.refresh(true);
            //MainCamera.politicsPanel.selectedReformValue = null;
        }
        else if (link is Province)
        {
            //MainCamera.politicsPanel.selectedReform = (AbstractReform)obj;
            //MainCamera.politicsPanel.refresh(true);
            //MainCamera.politicsPanel.selectedReformValue = null;
            Province temp = (Province)(link);
            MainCamera.SelectProvince(temp.getID());

        }


    }
    //internal void Setup(string text, UnityEngine.Events.UnityAction handleClick, ProductionWindowTable productionWindowTable, Factory stor)
    //{
    //    Setup( text,  handleClick, productionWindowTable);
    //}
}