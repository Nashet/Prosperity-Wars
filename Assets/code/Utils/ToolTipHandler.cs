using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public string tooltip;
    public MainTooltip tip;

    int counter = 0;
    internal Func<string> dynamicString;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //counter++
        //    ;
        //if (counter > 6 && tooltip != "")
        //{
        if (tooltip != "" || dynamicString != null)
        {
            if (dynamicString == null)
                tip.SetTooltip(tooltip);
            else
                tip.SetTooltip(dynamicString());

            counter = 0;
        }       // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tip != null)
            tip.HideTooltip();
    }

    //public void FixedUpdate()
    //{
    //    //if (counter > 300 && tip != null)
    //    //{
    //    //    tip.HideTooltip();
    //    //    counter = 0;
    //    //}
    //    //if (counter > 300 && tip != null)
    //    //{
    //    //    tip.HideTooltip();
    //    //    counter = 0;
    //    //}

    //    //counter++;
    //}


}
