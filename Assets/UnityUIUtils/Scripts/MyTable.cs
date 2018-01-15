using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//using Nashet.EconomicSimulation;
//using Nashet.ValueSpace;
//using Nashet.Utils;
namespace Nashet.UnityUIUtils
{
    public interface ICanBeCellInTable
    {
        void OnClickedCell();
    }
    /// <summary>
    /// Base class for UI tables. You must derive from that class your specific table. Allows only vertical scroll
    /// </summary>
    abstract public class UITableNew : MonoBehaviour, IRefreshable
    {
        [SerializeField]
        private SimpleObjectPool buttonObjectPool;

        [SerializeField]
        private Scrollbar verticalSlider;

        /// <summary>in pixels</summary>
        private readonly int rowHeight = 20;
        private int rowOffset;
        private bool alreadyInUpdate;

        abstract protected void AddHeader();
        abstract public void Refresh();

        protected int GetRowOffset()
        {
            return rowOffset;
        }
        protected void StartUpdate()
        {
            alreadyInUpdate = true;
        }
        protected void EndUpdate()
        {
            alreadyInUpdate = false;
        }
        /// <summary>
        /// assuming amount of pops didn't changed
        /// </summary>
        public void OnVerticalScroll()
        {
            if (!alreadyInUpdate)
                Refresh();
        }
        /// <summary>
        /// Returns how much rows should be shown. It's never bigger than totalRows
        /// </summary>    
        protected int CalcSize(int totalRows)
        {
            var rect = GetComponent<RectTransform>();            

            int howMuchRowsShow = (int)(rect.rect.height / rowHeight) - 1; //- header    

            if (howMuchRowsShow > totalRows)
                howMuchRowsShow = totalRows;

            int hiddenRows = totalRows - howMuchRowsShow;
            rowOffset = (int)(hiddenRows * (1f - verticalSlider.value));

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
            return howMuchRowsShow;
        }

        protected void AddButton(string text, ICanBeCellInTable bject = null, Func<string> dynamicTooltip = null)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, bject);
            if (dynamicTooltip != null)
            {
                newButton.GetComponentInChildren<ToolTipHandler>().setDynamicString(dynamicTooltip);
            }
        }

        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

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
        abstract protected class SortOrder : ICanBeCellInTable
        {
            private enum State { none, descending, ascending };
            //private enum State { descending, ascending };

            private State order = State.none;
            private readonly UITableNew parent;

            public SortOrder(UITableNew parent)
            {
                this.parent = parent;
            }
            protected IRefreshable getParent()
            {
                return parent;
            }
            //protected State getOrder()
            //{
            //    return order;
            //}
            public virtual void OnClickedCell()
            {
                order++;
                if (order > State.ascending)
                    order = State.none;
                    //order = State.descending;
            }
            public string getSymbol()
            {
                switch (order)
                {
                    case State.none:
                        return " ";
                    case State.descending:
                        return "^";
                    case State.ascending:
                        return "$";
                    default:
                        Debug.Log("Failed enum");
                        return null;
                }
            }

            protected List<T> DoSorting<T>(IEnumerable<T> list, Func<T, float> selector)//, Action defaultList
            {
                switch (order)
                {
                    case State.none:
                        //if (defaultList != null)
                        //    defaultList();
                        return list.ToList();
                    case State.descending:
                        return list.OrderByDescending(x => selector(x)).ToList();

                    case State.ascending:
                        return list.OrderBy(x => selector(x)).ToList();

                    default:
                        Debug.Log("Fail..");
                        return null;
                }
            }
        };

    }
    /// <summary>
    /// Old version, refreshes entire table. Don't use it
    /// </summary>    
    //abstract public class UITable : MonoBehaviour, IRefreshable
    //{
    //    [SerializeField]
    //    private SimpleObjectPool buttonObjectPool;

    //    [SerializeField]
    //    private int columnsAmount;

    //    protected int rowHeight = 20;

    //    abstract protected void AddHeader();
    //    abstract public void Refresh();
    //    abstract protected void AddButtons();
    //    // here magic is going
    //    void OnEnable()
    //    {
    //        //if (Game.date !=0)
    //        if (MainCamera.gameIsLoaded)
    //            Refresh();
    //    }
    //    public int GetColumnsAmount()
    //    {
    //        return columnsAmount;
    //    }
    //    protected void AddButton(string text, Province prov)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        sampleButton.Setup(text, prov);
    //    }
    //    protected void AddButton(string text, Province prov, string tooltipText)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        sampleButton.Setup(text, prov);
    //        newButton.GetComponentInChildren<ToolTipHandler>().setText(tooltipText);

    //    }
    //    protected void AddButton(string text)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        sampleButton.Setup(text, null);
    //    }
    //    protected void AddButton(string text, Product product)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        sampleButton.Setup(text, product);
    //    }
    //    protected void AddButton(string text, Storage storage)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        sampleButton.Setup(text, storage);
    //    }
    //    protected void AddButton(string text, Storage storage, Func<string> dynamicTooltip)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        sampleButton.Setup(text, storage);
    //        newButton.GetComponentInChildren<ToolTipHandler>().setDynamicString(dynamicTooltip);
    //    }
    //    protected void AddButton(string text, FactoryType type)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        //if (inventionType == null)
    //        //    sampleButton.Setup(text, this, null);
    //        //else
    //        sampleButton.Setup(text, type);
    //    }
    //    protected void AddButton(string text, Invention inventionType)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        newButton.transform.SetParent(gameObject.transform, true);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        //if (inventionType == null)
    //        //    sampleButton.Setup(text, this, null);
    //        //else
    //        sampleButton.Setup(text, inventionType);
    //    }
    //    protected void AddButton(string text, AbstractReform type)
    //    {
    //        GameObject newButton = buttonObjectPool.GetObject();
    //        //newButton.transform.SetParent(contentPanel, false);
    //        newButton.transform.SetParent(gameObject.transform);
    //        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
    //        //if (inventionType == null)
    //        //    sampleButton.Setup(text, this, null);
    //        //else
    //        sampleButton.Setup(text, type);
    //    }
    //    protected void RemoveButtons()
    //    {
    //        int count = gameObject.transform.childCount;
    //        for (int i = 0; i < count; i++)
    //        {
    //            GameObject toRemove = gameObject.transform.GetChild(0).gameObject;
    //            buttonObjectPool.ReturnObject(toRemove);
    //        }
    //    }


    //    //abstract protected void AddButton(string text, PopUnit record);

    //    //public void TryTransferItemToOtherShop(Item item)
    //    //{
    //    //    if (otherShop.gold >= item.price)
    //    //    {
    //    //        gold += item.price;
    //    //        otherShop.gold -= item.price;

    //    //        AddItem(item, otherShop);
    //    //        RemoveItem(item, this);

    //    //        RefreshDisplay();
    //    //        otherShop.RefreshDisplay();
    //    //        Debug.Log("enough gold");

    //    //    }
    //    //    Debug.Log("attempted");
    //    //}

    //    //void AddItem(Record itemToAdd, ShopScrollList shopList)
    //    //{
    //    //    shopList.recordList.Add(itemToAdd);
    //    //}

    //    //private void RemoveItem(Record itemToRemove, ShopScrollList shopList)
    //    //{
    //    //    for (int i = shopList.recordList.Count - 1; i >= 0; i--)
    //    //    {
    //    //        if (shopList.recordList[i] == itemToRemove)
    //    //        {
    //    //            shopList.recordList.RemoveAt(i);
    //    //        }
    //    //    }
    //    //}
    //}    
}