using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class PopulationPanel : DragPanel
{    
    internal Province showingProvince;
    public List<MyTableNew> tables = new List<MyTableNew>();
    // Use this for initialization
    void Start()
    {
        MainCamera.populationPanel = this;
        //show(false);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, MainCamera.topPanel.GetComponent<RectTransform>().rect.height *-1f);
        Canvas.ForceUpdateCanvases();
        hide();
    }

    public void show(bool bringOnTop)
    {
        gameObject.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();
        refreshContent();
    }
    override public void onCloseClick()
    {
        base.onCloseClick();
        //showAll = false;
    }
    internal void SetAllPopsToShow()
    {
        if (Game.Player != null)
        {
            List<PopUnit> er = new List<PopUnit>();
            //Game.popListToShow.Clear();
            foreach (Province province in Game.Player.ownedProvinces)
                foreach (PopUnit popUnit in province.allPopUnits)
                    // Game.popListToShow.Add(popUnit);
                    er.Add(popUnit);
            Game.popsToShowInPopulationPanel = er;
        }
    }
    public void onShowAllClick()
    {
        //hide();
        SetAllPopsToShow();
        //showAll = true;
        showingProvince = null;                
        show(true);
    }
    public void refreshContent()
    {       
        if (showingProvince == null)
            SetAllPopsToShow();
        foreach (var item in tables)
            item.refreshContent();     
    }    
}
