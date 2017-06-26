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
        Refresh();
    }
    abstract protected void Refresh();

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