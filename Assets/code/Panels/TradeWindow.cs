using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class TradeWindow  : DragPanel //: MonoBehaviour//
{       
    public ScrollRect table;
    // Use this for initialization
    void Start()
    {
        MainCamera.tradeWindow = this;
        hide();

    }
    //public void hide()
    //{
    //    tradeWindow.SetActive(false);
    //    //todo add button removal?      
    //}

    public void show(bool bringOnTop)
    {

        gameObject.SetActive(true);
        if (bringOnTop)
        panelRectTransform.SetAsLastSibling();

    }
    
   
    public void Refresh()
    {
        hide();
        show(false);
    }
    // Update is called once per frame
    //void Update()
    //{

    //}
}

