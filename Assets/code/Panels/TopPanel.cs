using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TopPanel : MonoBehaviour
{
    public Button btnPlay, btnStep, btnTrade;
    public Text generalText;
    // Use this for initialization
    void Start()
    {
        //generaltext = transform.FindChild("GeneralText").gameObject.GetComponent<Text>();
        btnPlay.onClick.AddListener(() => onbtnPlayClick(btnPlay));
        btnStep.onClick.AddListener(() => onbtnStepClick(btnPlay));
        btnPlay.image.color = Color.grey;
        MainCamera.topPanel = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void refresh()
    {
        
        generalText.text = "Date: " + Game.date + " Country: " + Game.player.name
            + " Population: " + Game.player.getMenPopulation() + " Storage: " + Game.player.storageSet.ToString() + " Money: "
            + Game.player.wallet.haveMoney
            + " Science points: " + Game.player.sciencePoints;
    }
    public void onTradeClick()
    {
        if (MainCamera.tradeWindow.isActiveAndEnabled)
            MainCamera.tradeWindow.hide();
        else
            MainCamera.tradeWindow.show(true);
    }
    public void onInventionsClick()
    {

        if (MainCamera.inventionsPanel.isActiveAndEnabled)
            MainCamera.inventionsPanel.hide();
        else
            MainCamera.inventionsPanel.show(true);
    }
    public void onEnterprisesClick()
    {
        //MainCamera.productionWindow.show(null, true);
        //MainCamera.productionWindow.onShowAllClick();

        //if (MainCamera.productionWindow.isActiveAndEnabled)
        //    MainCamera.productionWindow.hide();
        //else
        //{
        //    MainCamera.productionWindow.show(null, true);
        //    MainCamera.productionWindow.onShowAllClick();
        //}
        if (MainCamera.productionWindow.isActiveAndEnabled)
            if (MainCamera.productionWindow.showingProvince == null)
                MainCamera.productionWindow.hide();
            else
            {
                MainCamera.productionWindow.show(null, true);
                MainCamera.productionWindow.onShowAllClick();
            }
        else
        {
            MainCamera.productionWindow.show(null, true);
            MainCamera.productionWindow.onShowAllClick();
        }
    }
    public void onPopulationClick()
    {
        //MainCamera.populationPanel.onShowAllClick();
        ////MainCamera.populationPanel.show();

        if (MainCamera.populationPanel.isActiveAndEnabled)
            if (MainCamera.populationPanel.showAll)
                MainCamera.populationPanel.hide();
            else
                MainCamera.populationPanel.onShowAllClick();
        else
            MainCamera.populationPanel.onShowAllClick();
    }
    public void onPoliticsClick()
    {        
        if (MainCamera.politicsPanel.isActiveAndEnabled)
            MainCamera.politicsPanel.hide();
        else
            MainCamera.politicsPanel.show(true);
    }
    public void onFinanceClick()
    {
        if (MainCamera.financePanel.isActiveAndEnabled)
            MainCamera.financePanel.hide();
        else
            MainCamera.financePanel.show();
    }
    void onbtnStepClick(Button button)
    {

        Game.haveToStepSimulation = true;
    }
    void onbtnPlayClick(Button button)
    {
        Game.haveToRunSimulation = !Game.haveToRunSimulation;
        if (Game.haveToRunSimulation)
        {
            button.image.color = Color.white;
            Text text = button.GetComponentInChildren<Text>();
            text.text = "Playing";
        }
        else
        {
            button.image.color = Color.grey;
            Text text = button.GetComponentInChildren<Text>();
            text.text = "Pause";
        }
    }
}
abstract public class MyTable : MonoBehaviour
{
    //public List<Record> recordList;
    public Transform contentPanel; // myself
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
        newButton.transform.SetParent(contentPanel, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, prov);
    }
    protected void AddButton(string text)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, null);
    }
    protected void RemoveButtons()
    {
        int count = contentPanel.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject toRemove = contentPanel.GetChild(0).gameObject;
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