using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ProductionWindow : DragPanel
{
    public List<MyTableNew> tables = new List<MyTableNew>();
    Province showingProvince;
    void Start()
    {
        MainCamera.productionWindow = this;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.bottomPanel.GetComponent<RectTransform>().rect.height - 2f);
        Canvas.ForceUpdateCanvases();
        hide();
    }
    public Province getShowingProvince()
    {
        return showingProvince;
    }
    public void show(Province inn, bool bringOnTop)
    {
        gameObject.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();
        showingProvince = inn;
        if (showingProvince != null)
        {
            Game.factoriesToShowInProductionPanel = showingProvince.allFactories;
        }
        refreshContent();
    }
    internal void SetAllFactoriesToShow()
    {
        List<Factory> er = new List<Factory>();
        //Game.popListToShow.Clear();
        foreach (Province province in Game.Player.ownedProvinces)
            foreach (Factory factory in province.allFactories)
                // Game.popListToShow.Add(popUnit);
                er.Add(factory);
        Game.factoriesToShowInProductionPanel = er;
    }
    public void onShowAllClick()
    {
        SetAllFactoriesToShow();
        show(null, true);
    }
    public void refreshContent()
    {

        if (showingProvince == null)
        {
            SetAllFactoriesToShow();
        }
        foreach (var item in tables)
            item.refreshContent();
    }

    public void removeFactory(Factory fact)
    {
        if (Game.factoriesToShowInProductionPanel != null && Game.factoriesToShowInProductionPanel.Contains(fact))
        {
            Game.factoriesToShowInProductionPanel.Remove(fact);
            if (MainCamera.productionWindow.isActiveAndEnabled) refreshContent();
        }
    }
}
