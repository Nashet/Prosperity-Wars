using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class StatisticsPanel : DragPanel
{
    [SerializeField]
    private MyTableNew table;
    // Use this for initialization
    void Start()
    {
        MainCamera.StatisticPanel = this;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, -100f);
        //show(false);
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

    //public void onShowAllClick()
    //{
    //    //hide();
    //    SetAllPopsToShow();
    //    //showAll = true;
    //    showingProvince = null;
    //    show(true);
    //}
    public void refreshContent()
    {       
        table.refreshContent();
    }
}

