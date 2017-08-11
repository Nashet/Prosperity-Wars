using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class FactoryPanel : DragPanel//for dragging
{
    public Button upgradeButton, reopenButton, destroyButton, buyButton, sellButton, nationalizeButton;
    public Toggle subidize, dontHireOnSubsidies;
    public Slider priority;
    public Text generaltext;

    private Factory shownFactory;
    reopenButtonStatus reopenButtonflag;
    // Use this for initialization
    void Start()
    {
        MainCamera.factoryPanel = this;
        Hide();
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
    void setButtonTooltip(Button but, string hint)
    {
        if (hint == null)
        {
            but.interactable = true;
            but.GetComponentInChildren<ToolTipHandler>().tooltip = "";
        }
        else
        {
            but.interactable = false;
            but.GetComponentInChildren<ToolTipHandler>().tooltip = hint;
        }
    }
    void setGUIElementsAccesability()
    {
        var economy = shownFactory.getCountry().economy;

        //upgradeButton.interactable = economy.allowsFactoryUpgradeByGovernment();
        //setButtonTooltip(upgradeButton, shownFactory.whyCantUpgradeFactory(Game.player));
        //upgradeButton.interactable = shownFactory.getConditionsForFactoryUpgrade(Game.player,  out upgradeButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        upgradeButton.interactable = Factory.conditionsUpgrade.isAllTrue(shownFactory, Game.Player, out upgradeButton.GetComponentInChildren<ToolTipHandler>().tooltip);

        subidize.interactable = Factory.conditionsSubsidize.isAllTrue(shownFactory, Game.Player, out subidize.GetComponentInChildren<ToolTipHandler>().tooltip);
        //subidize.interactable = shownFactory.con
        if (shownFactory.isWorking())
            reopenButtonflag = reopenButtonStatus.close;
        else
            reopenButtonflag = reopenButtonStatus.reopen;
        if (reopenButtonflag == reopenButtonStatus.close)
        {
            //reopenButton.interactable = economy.allowsFactoryCloseByGovernment();
            reopenButton.GetComponentInChildren<Text>().text = "Close";
            //setButtonTooltip(reopenButton, shownFactory.whyCantCloseFactory());
            reopenButton.interactable = Factory.conditionsClose.isAllTrue(shownFactory, Game.Player, out reopenButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        }
        else
        {
            //reopenButton.interactable = economy.allowsFactoryReopenByGovernment();
            //setButtonTooltip(reopenButton, shownFactory.whyCantReopenFactory());
            reopenButton.interactable = Factory.conditionsReopen.isAllTrue(shownFactory, Game.Player, out reopenButton.GetComponentInChildren<ToolTipHandler>().tooltip);
            reopenButton.GetComponentInChildren<Text>().text = "Reopen";
        }

        //destroyButton.interactable = economy.allowsFactoryDestroyByGovernment();
        // hint = shownFactory.whyCantDestroyFactory();
        //setButtonTooltip(destroyButton, shownFactory.whyCantDestroyFactory());
        destroyButton.interactable = Factory.conditionsDestroy.isAllTrue(shownFactory, Game.Player, out destroyButton.GetComponentInChildren<ToolTipHandler>().tooltip);

        sellButton.interactable = Factory.conditionsSell.isAllTrue(Game.Player, out sellButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        buyButton.interactable = Factory.conditionsBuy.isAllTrue(Game.Player, out buyButton.GetComponentInChildren<ToolTipHandler>().tooltip);
        nationalizeButton.interactable = Factory.conditionsNatinalize.isAllTrue(shownFactory, Game.Player, out nationalizeButton.GetComponentInChildren<ToolTipHandler>().tooltip);

        //sellButton.interactable = economy.allowsFactorySellByGovernment();
        //buyButton.interactable = economy.allowsFactoryBuyByGovernment();
        //nationalizeButton.interactable = economy.allowsFactoryNatonalizeByGovernment();
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

            Factory.modifierEfficiency.getModifier(shownFactory, out generaltext.GetComponentInChildren<ToolTipHandler>().tooltip);

            //var temp = shownFactory.getLifeNeeds();
            //foreach (Storage next in temp)
            //    lifeNeeds += next.ToString() + "; ";
            //lifeNeeds += shownFactory.getLifeNeedsFullfilling().ToString() + " fulfilled";
            string InputRequired = "";
            //var temp = shownFactory.getLifeNeeds();

            string construction = "";
            if (shownFactory.getDaysInConstruction() > 0)
                construction = "\nDays in construction: " + shownFactory.getDaysInConstruction();
            string unprofitable = "";
            if (shownFactory.getDaysUnprofitable() > 0)
                unprofitable = " Days unprofitable: " + shownFactory.getDaysUnprofitable();
            string daysClosed = "";
            if (shownFactory.getDaysClosed() > 0)
                daysClosed = " Days closed: " + shownFactory.getDaysClosed();
            string loans = "";
            if (shownFactory.loans.get() > 0f)
                loans = "\nLoan: " + shownFactory.loans.ToString();
            string upgradeNeeds = "";
            if (shownFactory.constructionNeeds.Count() > 0)
                upgradeNeeds = "\nUpgrade needs: " + shownFactory.constructionNeeds;

            foreach (Storage next in shownFactory.getType().resourceInput)
                InputRequired += next.get() * shownFactory.getWorkForceFulFilling().get() + " " + next.getProduct().ToString() + ";";
            generaltext.text = shownFactory.getType().name + " level: " + shownFactory.getLevel() + "\n" + "Workforce: " + shownFactory.getWorkForce()
                + "\nUnsold: " + shownFactory.storageNow.ToString()
                + "\nGain goods: " + shownFactory.gainGoodsThisTurn.ToString()
                + "\nBasic production: " + shownFactory.getType().basicProduction
                + "\nEfficiency: " + Factory.modifierEfficiency.getModifier(shownFactory)
                + "\nSent to market: " + shownFactory.sentToMarket
                + "\nCash: " + shownFactory.cash.ToString()
                + "\nMoney income: " + shownFactory.moneyIncomethisTurn
                + "\nProfit: " + shownFactory.getProfit()
                + "\nInput required: " + InputRequired
                + "\nConsumed: " + shownFactory.consumedTotal.ToString() + " Cost: " + Game.market.getCost(shownFactory.consumedTotal)
                + "\nConsumed LT: " + shownFactory.consumedLastTurn
                + "\nInput reserves: " + shownFactory.getInputProductsReserve()
                + "\nInput factor: " + shownFactory.getInputFactor()
                + "\nSalary (per 1000 men):" + shownFactory.getSalary() + " Salary(total):" + shownFactory.getSalaryCost()
                + "\nOwner: " + shownFactory.getOwner()
                + upgradeNeeds
                + construction + unprofitable + daysClosed + loans
                + "\nHowMuchHiredLastTurn " + shownFactory.getHowMuchHiredLastTurn()

                ;
            //+ "\nExpenses:"
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
            Hide();
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
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
        shownFactory.setPriority((byte)Mathf.RoundToInt(priority.value));

    }
    public void onReopenClick()
    {
        if (reopenButtonflag == reopenButtonStatus.close)
            //if (shownFactory.whyCantCloseFactory() == null)
            shownFactory.close();
        else
            //if (shownFactory.whyCantReopenFactory() == null)
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
