using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class TradeWindow  : DragPanel //: MonoBehaviour//
{
    public GameObject tradeWindow;
    // public GameObject ScrollViewMy;
    public ScrollRect table;
    // Use this for initialization
    void Start()
    {
        MainCamera.tradeWindow = this;
        hide();

    }
    public void hide()
    {
        tradeWindow.SetActive(false);
        //todo add button removal?      
    }

    public void show(bool bringOnTop)
    {

        tradeWindow.SetActive(true);
        if (bringOnTop)
        panelRectTransform.SetAsLastSibling();

    }
    public void onCloseClick()
    {
        hide();
    }
    public void onShowAllClick()
    {
        hide();
        List<PopUnit> er = new List<PopUnit>();        
        foreach (Province province in Game.player.ownedProvinces)
            foreach (PopUnit popUnit in province.allPopUnits)                
                er.Add(popUnit);
        Game.popsToShowInPopulationPanel = er;
        show(true);

    }
    public void Refresh()
    {
        hide();
        show(false);
    }
    // Update is called once per frame
    void Update()
    {

    }
}

