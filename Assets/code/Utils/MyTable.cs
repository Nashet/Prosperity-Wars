using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
abstract public class MyTable : MonoBehaviour
{
    //public List<Record> recordList;
    //public Transform contentPanel; // myself
    public SimpleObjectPool buttonObjectPool;
    //public GameObject parentPanel;
    //public ShopScrollList otherShop;
    protected int rowHeight = 20;
    public int columnsAmount;

    // Use this for initialization
    void Start()
    {

    }
    // here magic is going
    void OnEnable()
    {
        //if (Game.date !=0)
        refresh();
    }
    //void Update()
    //{
    //    //if (Game.date !=0)
    //    refresh();
    //}

    abstract protected void refresh();

    //void Update()
    //{
    //    // refresh();
    //}
    protected void AddButton(string text, Province prov)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, prov);
    }
    protected void AddButton(string text, Province prov, string tooltip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, prov);
        newButton.GetComponentInChildren<ToolTipHandler>().tooltip = tooltip;
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
    protected void AddButton(string text)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, null);
    }
    protected void AddButton(string text, Product product)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, product);
    }
    protected void AddButton(string text, Storage storage)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, storage);
    }
    protected void AddButton(string text, Storage storage, Func<string> dynamicTooltip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, storage);
        newButton.GetComponentInChildren<ToolTipHandler>().setDynamicString(dynamicTooltip);
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
    protected void RemoveButtons()
    {
        int count = gameObject.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject toRemove = gameObject.transform.GetChild(0).gameObject;
            buttonObjectPool.ReturnObject(toRemove);
        }
    }

    abstract protected void AddButtons();
    //abstract protected void AddButton(string text, PopUnit record);

    //public void TryTransferItemToOtherShop(Item item)
    //{
    //    if (otherShop.gold >= item.price)
    //    {
    //        gold += item.price;
    //        otherShop.gold -= item.price;

    //        AddItem(item, otherShop);
    //        RemoveItem(item, this);

    //        RefreshDisplay();
    //        otherShop.RefreshDisplay();
    //        Debug.Log("enough gold");

    //    }
    //    Debug.Log("attempted");
    //}

    //void AddItem(Record itemToAdd, ShopScrollList shopList)
    //{
    //    shopList.recordList.Add(itemToAdd);
    //}

    //private void RemoveItem(Record itemToRemove, ShopScrollList shopList)
    //{
    //    for (int i = shopList.recordList.Count - 1; i >= 0; i--)
    //    {
    //        if (shopList.recordList[i] == itemToRemove)
    //        {
    //            shopList.recordList.RemoveAt(i);
    //        }
    //    }
    //}
}
abstract public class MyTableNew : MonoBehaviour
{
    public SimpleObjectPool buttonObjectPool;

    protected readonly int rowHeight = 20;
    //public int columnsAmount;
    public Scrollbar verticalSlider;
    protected int howMuchRowsShow;
    protected int offset;

    protected bool alreadyInUpdate;


    public abstract void refreshContent();


    /// <summary>
    /// assuming amount of pops didn't changed
    /// </summary>
    public void OnVerticalScroll()
    {
        if (!alreadyInUpdate)
            refreshContent();
    }

    protected void calcSize(int totalRows)
    {
        var rect = GetComponent<RectTransform>();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        howMuchRowsShow = (int)(rect.rect.height / rowHeight) - 1; //- header    

        if (howMuchRowsShow > totalRows)
            howMuchRowsShow = totalRows;

        int hiddenRows = totalRows - howMuchRowsShow;
        offset = (int)(hiddenRows * (1f - verticalSlider.value));

        // check if scrollbar size should be changed
        if (totalRows > 0)
        {
            verticalSlider.size = (float)howMuchRowsShow / (float)totalRows;
            verticalSlider.numberOfSteps = totalRows;
        }
        else
        {
            verticalSlider.size = 1;
            verticalSlider.numberOfSteps = 10;
        }
    }
    protected void AddButton(string text, Province prov)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, prov);
    }
    protected void AddButton(string text, Factory stor)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, stor);
    }
    protected void AddButton(string text, Province prov, string tooltip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, prov);
        newButton.GetComponentInChildren<ToolTipHandler>().tooltip = tooltip;
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
    protected void AddButton(string text)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, null);
    }

    protected void AddButton(string text, PopUnit record)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, record);
    }
    protected void AddButton(string text, PopUnit record, string toolTip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, record);
        newButton.GetComponentInChildren<ToolTipHandler>().tooltip = toolTip;
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
    protected void AddButton(string text, PopUnit record, Func<string> dynamicTooltip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, null, record);
        newButton.GetComponentInChildren<ToolTipHandler>().setDynamicString(dynamicTooltip);
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
    protected void RemoveButtons()
    {
        //lock (buttonObjectPool)
        {
            int count = gameObject.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                GameObject toRemove = gameObject.transform.GetChild(0).gameObject;
                buttonObjectPool.ReturnObject(toRemove);
            }
        }
    }

}