using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Func<string> dynamicString;
    public string tooltip;
    public MainTooltip tip;

    //int counter = 0;


    public void setDynamicString(Func<string> dynamicString)
    {
        this.dynamicString = dynamicString;
        //if (dynamicString != null && tip != null)
        //{
        //    //tip.HideTooltip();
        //    //tip.SetTooltip(dynamicString());
        //    tip.redrawDynamicString(dynamicString());
        //    //OnPointerExit(null);
        //    //OnPointerEnter(null);
        //}
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != "" || dynamicString != null)
        {
            if (dynamicString == null)
                tip.SetTooltip(tooltip);
            else
                tip.SetTooltip(dynamicString());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tip != null)
            tip.HideTooltip();
    }
    public void OnMouseOver()
    {
        if (dynamicString != null && tip != null)
            tip.SetTooltip(dynamicString());
    }
}
