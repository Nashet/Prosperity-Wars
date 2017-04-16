using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{

    public string tooltip;
    public MainTooltip tip;

    int counter = 0;
    void Start()
    {
        //tip = MainTooltip.getTip();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        counter = 0;
        if (tooltip != "")
        {

            tip.SetTooltip(tooltip);
            tip.LayoutInit();
        }


    }
    public void OnSelect(BaseEventData eventData)
    {

    }
    public void OnPointerExit(PointerEventData eventData)
    {
    }
    public void OnDeselect(BaseEventData eventData)
    {

    }
    public void FixedUpdate()
    {
        if (counter > 300 && tip != null)
        {
            tip.HideTooltip();
            counter = 0;
        }

        counter++;
    }
    //void StartHover(Vector3 position)
    //{
    //    //tip.SetTooltip(text);
    //    tip.SetTooltip(tooltip);
    //}
    //void StopHover()
    //{
    //    if (tip != null)
    //        tip.HideTooltip();
    //}

}
