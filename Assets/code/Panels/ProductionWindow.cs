using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ProductionWindow : DragPanel
{
    public GameObject panel;
    // public GameObject ScrollViewMy;
    public ScrollRect table;
    // Use this for initialization
    //bool showAll;
    public Province showingProvince;
    void Start()
    {
        MainCamera.productionWindow = this;
        hide();
    }
    public void hide()
    {
        panel.SetActive(false);
        //todo add button removal?      
    }
    public void show(Province inn, bool bringOnTop)
    {
        panel.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();
        showingProvince = inn;
        if (showingProvince != null)
        {
            Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
        }
    }
    public void onCloseClick()
    {
        hide();
        //showAll = false;
        //showingProvince
    }
    //internal void setShowAll()
    //{
    //    showAll = true;
    //}
    internal void SetAllFactoriesToShow()
    {
        List<Factory> er = new List<Factory>();
        //Game.popListToShow.Clear();
        foreach (Province province in Game.player.ownedProvinces)
            foreach (Factory factory in province.allFactories)
                // Game.popListToShow.Add(popUnit);
                er.Add(factory);
        Game.factoriesToShowInProductionPanel = er;
    }
    public void onShowAllClick()
    {
        hide();
        SetAllFactoriesToShow();
        //showAll = true;
        showingProvince = null;
        show(null, true);
    }
    public void refresh()
    {

        hide();
        //if (showAll)
        if (showingProvince ==null)
        {
            SetAllFactoriesToShow();
            show(null, false);
        }
        else // take factories from province
            show(showingProvince, false); ;

    }

    public void removeFactory(Factory fact)
    {


        if (Game.factoriesToShowInProductionPanel != null && Game.factoriesToShowInProductionPanel.Contains(fact))
        {
            Game.factoriesToShowInProductionPanel.Remove(fact);
            if (MainCamera.productionWindow.isActiveAndEnabled) refresh();
        }
    }
    // Update is called once per frame
    //   void Update () {

    //}
}
