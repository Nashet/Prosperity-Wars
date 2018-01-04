using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class FactoryPanel : DragPanel//for dragging
{
    [SerializeField]
    private Button upgradeButton, reopenButton, destroyButton, buyButton, sellButton, nationalizeButton;
    [SerializeField]
    private Toggle subidize, dontHireOnSubsidies;
    [SerializeField]
    private Slider priority;
    [SerializeField]
    private Text generaltext, efficiencyText;

    private Factory shownFactory;
    private reopenButtonStatus reopenButtonflag;
    // Use this for initialization
    void Start()
    {
        MainCamera.factoryPanel = this;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(295f, -50f);
        hide();
    }
    //void OnGUI()
    //{
    //    GUI.Button(new Rect(10, 10, 100, 20), new GUIContent("Click me", "This is the tooltip"));
    //    GUI.Label(new Rect(10, 40, 100, 40), GUI.tooltip);
    //}
    enum reopenButtonStatus { reopen, close };
    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
   
    void setGUIElementsAccesability()
    {
        upgradeButton.interactable = Factory.conditionsUpgrade.isAllTrue(shownFactory, Game.Player, out upgradeButton.GetComponentInChildren<ToolTipHandler>().tooltip);

        subidize.interactable = Factory.conditionsSubsidize.isAllTrue(shownFactory, Game.Player, out subidize.GetComponentInChildren<ToolTipHandler>().tooltip);

        if (shownFactory.isWorking())
            reopenButtonflag = reopenButtonStatus.close;
        else
            reopenButtonflag = reopenButtonStatus.reopen;
        if (reopenButtonflag == reopenButtonStatus.close)
        {
            reopenButton.GetComponentInChildren<Text>().text = "Close";
            reopenButton.interactable = Factory.conditionsClose.isAllTrue(shownFactory, Game.Player, out reopenButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        }
        else
        {
            reopenButton.interactable = Factory.conditionsReopen.isAllTrue(shownFactory, Game.Player, out reopenButton.GetComponentInChildren<ToolTipHandler>().tooltip);
            reopenButton.GetComponentInChildren<Text>().text = "Reopen";
        }

        destroyButton.interactable = Factory.conditionsDestroy.isAllTrue(shownFactory, Game.Player, out destroyButton.GetComponentInChildren<ToolTipHandler>().tooltip);

        sellButton.interactable = Factory.conditionsSell.isAllTrue(Game.Player, out sellButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        buyButton.interactable = Factory.conditionsBuy.isAllTrue(Game.Player, out buyButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        nationalizeButton.interactable = Factory.conditionsNatinalize.isAllTrue(shownFactory, Game.Player, out nationalizeButton.GetComponentInChildren<ToolTipHandler>().tooltip);

        this.priority.interactable = Factory.conditionsChangePriority.isAllTrue(shownFactory, Game.Player, out priority.GetComponentInChildren<ToolTipHandler>().tooltip);
        this.subidize.interactable = Factory.conditionsSubsidize.isAllTrue(shownFactory, Game.Player, out subidize.GetComponentInChildren<ToolTipHandler>().tooltip);
        this.dontHireOnSubsidies.interactable = Factory.conditionsDontHireOnSubsidies.isAllTrue(shownFactory, Game.Player, out dontHireOnSubsidies.GetComponentInChildren<ToolTipHandler>().tooltip);

        priority.value = shownFactory.getPriority();
        subidize.isOn = shownFactory.isSubsidized();
        dontHireOnSubsidies.isOn = shownFactory.isDontHireOnSubsidies();
    }
    public void refresh()
    {
        if (shownFactory != null)
        {
            setGUIElementsAccesability();
            Factory.modifierEfficiency.getModifier(shownFactory, out efficiencyText.GetComponentInChildren<ToolTipHandler>().tooltip);
            var sb = new StringBuilder();                                                               

            sb.Append(shownFactory.getType().name).Append(" level: ").Append(shownFactory.getLevel());  
            sb.Append("\n").Append("Workforce: ").Append(shownFactory.getWorkForce());
            sb.Append("\nGain goods: ").Append(shownFactory.getGainGoodsThisTurn().ToString());
            sb.Append("\nUnsold: ").Append(shownFactory.storage.ToString());
            sb.Append("\nBasic production: ").Append(shownFactory.getType().basicProduction);            
            sb.Append("\nSent to market: ").Append(shownFactory.getSentToMarket());
            sb.Append("\nCash: ").Append(shownFactory.cash.ToString());
            sb.Append("\nMoney income: ").Append(shownFactory.moneyIncomethisTurn);
            //if (Game.Player.economy.getValue() != Economy.PlannedEconomy)
            {
                
                sb.Append("\nProfit: ").Append(shownFactory.getProfit());
            }
            if (shownFactory.getType().hasInput())
            {
                sb.Append("\nInput required: ");
                foreach (Storage next in shownFactory.getType().resourceInput)
                    sb.Append(next.get() * shownFactory.getWorkForceFulFilling().get()).Append(" ").Append(next.getProduct()).Append(";");
            }  
            sb.Append("\nConsumed: ").Append(shownFactory.getConsumed().ToString()).Append(" Cost: ").Append(Game.market.getCost(shownFactory.getConsumed()));
            if (Game.devMode)
                sb.Append("\nConsumed LT: ").Append(shownFactory.getConsumedLastTurn());
            sb.Append("\nInput reserves: ").Append(shownFactory.getInputProductsReserve());
            sb.Append("\nInput factor: ").Append(shownFactory.getInputFactor());
            sb.Append("\nSalary (per 1000 men):").Append(shownFactory.getSalary()).Append(" Salary(total):").Append(shownFactory.getSalaryCost());
            sb.Append("\nOwner: ").Append(shownFactory.getOwner());

            if (shownFactory.constructionNeeds.Count() > 0)
                sb.Append("\nUpgrade needs: ").Append(shownFactory.constructionNeeds);    

            if (shownFactory.getDaysInConstruction() > 0)
                sb.Append("\nDays in construction: ").Append(shownFactory.getDaysInConstruction());

            if (shownFactory.getDaysUnprofitable() > 0) 
                sb.Append(" Days unprofitable: ").Append(shownFactory.getDaysUnprofitable());

            if (shownFactory.getDaysClosed() > 0)
                sb.Append(" Days closed: ").Append(shownFactory.getDaysClosed());                                                                                 

            if (shownFactory.loans.get() > 0f)
                sb.Append("\nLoan: ").Append( shownFactory.loans.ToString());

            if (Game.devMode)
                sb.Append("\nHowMuchHiredLastTurn ").Append(shownFactory.getHowMuchHiredLastTurn());
            generaltext.text = sb.ToString();
            //+ "\nExpenses:"
            efficiencyText.text = "Efficiency: "+Factory.modifierEfficiency.getModifier(shownFactory);
        }
    }
    public void Show(Factory fact)
    {
        gameObject.SetActive(true);
        shownFactory = fact;
        panelRectTransform.SetAsLastSibling();
        refresh();
    }
    public void removeFactory(Factory fact)
    {
        if (fact == shownFactory)
        {
            shownFactory = null;
            hide();
        }
    }       

    public void onSubsidizeValueChanged()
    {
        shownFactory.setSubsidized(subidize.isOn);
        refresh();
    }
    public void ondontHireOnSubsidiesValueChanged()
    {
        shownFactory.setDontHireOnSubsidies(dontHireOnSubsidies.isOn);
    }
    public void onPriorityChanged()
    {
        shownFactory.setPriority((int)Mathf.RoundToInt(priority.value)); 
    }
    public void onReopenClick()
    {
        if (reopenButtonflag == reopenButtonStatus.close)            
            shownFactory.close();
        else                                             
            shownFactory.open(Game.Player);
        refresh();
        if (MainCamera.productionWindow.isActiveAndEnabled) MainCamera.productionWindow.refreshContent();
        MainCamera.topPanel.refresh();
        if (MainCamera.financePanel.isActiveAndEnabled) MainCamera.financePanel.refresh();
    }
    public void onUpgradeClick()
    {
        //if (shownFactory.getConditionsForFactoryUpgradeFast(Game.player))
        {
            shownFactory.upgrade(Game.Player);
            if (MainCamera.productionWindow.isActiveAndEnabled) MainCamera.productionWindow.refreshContent();
            MainCamera.topPanel.refresh();
            if (MainCamera.financePanel.isActiveAndEnabled) MainCamera.financePanel.refresh();
            this.refresh();
        }
    }
    public void onDestroyClick()
    {
        //if (shownFactory.whyCantDestroyFactory() == null)
        {
            shownFactory.destroyImmediately();
            MainCamera.refreshAllActive();
        }
    }
    public void onBuyClick()
    {

    }
    public void onSellClick()
    {

    }
    public void onNationalizeClick()
    {
        shownFactory.setOwner(Game.Player);
        MainCamera.refreshAllActive();
    }

}
