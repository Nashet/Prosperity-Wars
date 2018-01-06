using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.UnityUIUtils
{
    public interface ICanBeCellInTable
    {

    }
    abstract public class MyTable : MonoBehaviour, IRefreshable
    {
        [SerializeField]
        private SimpleObjectPool buttonObjectPool;

        [SerializeField]
        private int columnsAmount;

        protected int rowHeight = 20;

        abstract protected void AddHeader();
        abstract public void Refresh();
        abstract protected void AddButtons();
        // here magic is going
        void OnEnable()
        {
            //if (Game.date !=0)
            if (MainCamera.gameIsLoaded)
                Refresh();
        }
        public int GetColumnsAmount()
        {
            return columnsAmount;
        }
        protected void AddButton(string text, Province prov)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, prov);
        }
        protected void AddButton(string text, Province prov, string tooltipText)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, prov);
            newButton.GetComponentInChildren<ToolTipHandler>().setText(tooltipText);

        }
        protected void AddButton(string text)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, null);
        }
        protected void AddButton(string text, Product product)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, product);
        }
        protected void AddButton(string text, Storage storage)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, storage);
        }
        protected void AddButton(string text, Storage storage, Func<string> dynamicTooltip)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            sampleButton.Setup(text, storage);
            newButton.GetComponentInChildren<ToolTipHandler>().setDynamicString(dynamicTooltip);
        }
        protected void AddButton(string text, FactoryType type)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            //if (inventionType == null)
            //    sampleButton.Setup(text, this, null);
            //else
            sampleButton.Setup(text, type);
        }
        protected void AddButton(string text, Invention inventionType)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            //if (inventionType == null)
            //    sampleButton.Setup(text, this, null);
            //else
            sampleButton.Setup(text, inventionType);
        }
        protected void AddButton(string text, AbstractReform type)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            //newButton.transform.SetParent(contentPanel, false);
            newButton.transform.SetParent(gameObject.transform);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            //if (inventionType == null)
            //    sampleButton.Setup(text, this, null);
            //else
            sampleButton.Setup(text, type);
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
    abstract public class MyTableNew : MonoBehaviour, IRefreshable
    {
        [SerializeField]
        private SimpleObjectPool buttonObjectPool;

        [SerializeField]
        private Scrollbar verticalSlider;

        /// <summary>in pixels</summary>
        private readonly int rowHeight = 20;
        private int rowOffset;
        private bool alreadyInUpdate;

        abstract protected void addHeader();
        abstract public void Refresh();

        public int getRowOffset()
        {
            return rowOffset;
        }
        
        public void startUpdate()
        {
            alreadyInUpdate = true;
        }
        public void endUpdate()
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
        protected int calcSize(int totalRows)
        {
            var rect = GetComponent<RectTransform>();
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

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

        protected void addButton(string text, ICanBeCellInTable bject = null, Func<string> dynamicTooltip = null)
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

    }    
}