using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//using Nashet.EconomicSimulation;
//using Nashet.ValueSpace;
//using Nashet.Utils;
namespace Nashet.UnityUIUtils
{
    public interface IClickable
    {
        void OnClicked();
    }

    public interface IFiltrable<T>
    {
        void AddFilter(Predicate<T> filter);

        void RemoveFilter(Predicate<T> filter);

        void ClearAllFiltres();

        void AddAllFiltres();

        bool IsSetAnyFilter();

        bool IsAppliedThatFilter(Predicate<T> filter);
    }

    //public class Filter:Predicate<T>
    //{ }
    /// <summary>
    /// Base class for UI tables. You must derive from that class your specific table. Allows only vertical scroll
    /// </summary>
    public abstract class UITableNew<T> : MonoBehaviour, IRefreshable, IFiltrable<T>
    {
        [SerializeField]
        private SimpleObjectPool buttonObjectPool;

        [SerializeField]
        private Scrollbar verticalSlider;

        [SerializeField]
        protected List<T> elementsToShow;

        [SerializeField]
        private List<Toggle> filterToggles;

        //
        //
        /// <summary>in pixels</summary>
        private readonly int rowHeight = 20;

        private int howMuchRowsShow;
        private int rowOffset;
        private bool alreadyInUpdate;

        protected abstract void AddHeader();

        /// <summary>
        /// That method takes content from child (List<T>) and applies filter
        /// </summary>
        protected abstract IEnumerable<T> ContentSelector();

        private SortOrder order;

        private List<T> Select(List<T> source, List<Predicate<T>> filter)
        {
            var res = source;
            foreach (var item in filter)
            {
                res = res.FindAll(item as Predicate<T>);
            }
            return res;
        }

        public void Refresh()
        {
            StartUpdate();
            //lock (gameObject)
            {
                RemoveButtons();
                AddHeader();
                //int counter = 0;
                //do NOT rely on elements order!
                //Expression<Func<EconomicSimulation.PopUnit, bool>> isAdult = x => x.popType == EconomicSimulation.PopType.Workers;
                Func<T, bool> sheet = x => true;
                if (IsSetAnyFilter())
                    //elementsToShow = ContentSelector().FindAll(filters);
                    elementsToShow = Select(ContentSelector().ToList(), filters);
                //elementsToShow = ContentSelector().Where(filters as Func<T, bool>).ToList();
                else
                    elementsToShow = ContentSelector().ToList();
                if (order != null && order.IsOrderSet())
                    elementsToShow = order.DoSorting(elementsToShow);
                howMuchRowsShow = ReCalcSize(elementsToShow.Count);
                FillRows();
            }
            EndUpdate();
        }

        private void SetOrder(SortOrder sortOrder)
        {
            order = sortOrder;
        }

        //protected void SetElementsToShow(List<T> list)
        //{
        //    elementsToShow = list;
        //    howMuchRowsShow = ReCalcSize(elementsToShow.Count);
        //}
        protected abstract void AddRow(T type, int number);

        protected void FillRows()
        {
            for (int i = 0; i < howMuchRowsShow; i++)
            {
                var product = elementsToShow[i + GetRowOffset()];
                AddRow(product, i);
            }
        }

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
        protected int ReCalcSize(int totalRows)
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

        protected void AddCell(string text, IClickable @object = null, Func<string> dynamicTooltip = null)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SimpleCell sampleButton = newButton.GetComponent<SimpleCell>();
            sampleButton.Setup(text, @object);
            if (dynamicTooltip != null)
            {
                newButton.GetComponent<ToolTipHandler>().SetTextDynamic(dynamicTooltip);
            }
        }

        //protected void AddHeader(string text, SortOrder @object = null, Func<string> dynamicTooltip = null)
        //{
        //    GameObject newButton = buttonObjectPool.GetObject();
        //    newButton.transform.SetParent(gameObject.transform, true);
        //    SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        //    sampleButton.Setup(text, @object);
        //    if (dynamicTooltip != null)
        //    {
        //        newButton.GetComponent<ToolTipHandler>().setDynamicString(dynamicTooltip);
        //    }
        //}

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

        //public static Predicate<T> And<T>(params Predicate<T>[] predicates)
        //{
        //    return delegate (T item)
        //    {
        //        foreach (Predicate<T> predicate in predicates)
        //        {
        //            if (!predicate(item))
        //            {
        //                return false;
        //            }
        //        }
        //        return true;
        //    };
        //}
        //static private readonly Predicate<T> showAll = new Predicate<T>(x => true);
        ///<summary>Empty means no filters applied, showing everything</summary>
        private readonly List<Predicate<T>> filters = new List<Predicate<T>>();

        public void AddFilter(Predicate<T> filter)
        {
            //if (!IsAppliedThatFilter(filter))
            //    if (IsSetAnyFilter())
            //        filters += filter;
            //    else
            //        filters = new Predicate<T>(filter);
            if (!filters.Contains(filter))
                filters.Add(filter);
        }

        public void RemoveFilter(Predicate<T> filter)
        {
            //if (IsSetAnyFilter())
            //    if (IsAppliedThatFilter(filter) && filters.GetInvocationList().Length == 1)
            //        filters = null;
            //    else
            //        filters -= filter;
            filters.Remove(filter);
        }

        public bool IsSetAnyFilter()
        {
            return filters.Count > 0;// != null;
        }

        public bool IsAppliedThatFilter(Predicate<T> filter)
        {
            //if (IsSetAnyFilter())
            //    //return filters == (filter + showAll);
            //    return filters.GetInvocationList().Contains(filter);
            ////return filters.Equals(filter + noFilter);
            ////return filters.GetInvocationList().Any(x => x.Method == filter.Method);

            ////return filters.GetInvocationList().Any(x => x.Equals(filter));
            //else
            //    return false;
            return filters.Contains(filter);
        }

        public void ClearAllFiltres()
        {
            filters.Clear();
            foreach (var item in filterToggles)
            {
                item.isOn = true;
            }
        }

        public void AddAllFiltres()
        {
            //filters.Clear();// = null;//new Predicate<T
            foreach (var item in filterToggles)
            {
                item.isOn = false;
            }
        }

        //private static UITableNew<T> ThatObject;
        //public static UITableNew<T> GetThatObject()
        //{
        //}
        protected class SortOrder : IClickable
        {
            private enum State { none, descending, ascending }

            //private enum State { descending, ascending };

            private State orderState = State.none;
            private Func<T, float> sortOrder;
            private readonly UITableNew<T> parent;

            public SortOrder(UITableNew<T> parent, Func<T, float> sortOrder)
            {
                this.parent = parent;
                this.sortOrder = sortOrder;
            }

            protected UITableNew<T> getParent()
            {
                return parent;
            }

            public bool IsOrderSet()
            {
                return orderState != State.none;
            }

            //protected State getOrder()
            //{
            //    return order;
            //}
            public virtual void OnClicked()
            {
                orderState++;
                if (orderState > State.ascending)
                    orderState = State.none;
                getParent().SetOrder(this);
                getParent().Refresh();
            }

            public string getSymbol()
            {
                switch (orderState)
                {
                    case State.none:
                        return " ";

                    case State.descending:
#if UNITY_WEBGL
                        return " v";
#else
                        return " \u25BC";
#endif
                    case State.ascending:
#if UNITY_WEBGL
                        return " ^";
#else
                        return " \u25B2";
#endif
                    default:
                        Debug.Log("Failed enum");
                        return null;
                }
            }

            public List<T> DoSorting(IEnumerable<T> list)//, Action defaultList
            {
                switch (orderState)
                {
                    case State.none:
                        //if (defaultList != null)
                        //    defaultList();
                        return list.ToList();

                    case State.descending:
                        return list.OrderByDescending(sortOrder).ToList();

                    case State.ascending:
                        return list.OrderBy(sortOrder).ToList();

                    default:
                        Debug.Log("Fail..");
                        return null;
                }
            }
        }
    }

    [Serializable]
    public class Filter<T>
    {
        private UITableNew<T> parent;
        private Predicate<T> filter;

        public Filter(Predicate<T> filter, UITableNew<T> parent)
        {
            this.parent = parent;
            this.filter = filter;
        }

        public void OnFilterChange(bool @checked)
        {
            if (@checked)
                parent.RemoveFilter(filter);
            else
                parent.AddFilter(filter);
            parent.Refresh();
        }
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
    //        newButton.GetComponent<ToolTipHandler>().setText(tooltipText);

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
    //        newButton.GetComponent<ToolTipHandler>().setDynamicString(dynamicTooltip);
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