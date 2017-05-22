using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

abstract public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{

    private Vector2 pointerOffset;
    private RectTransform canvasRectTransform;
    protected RectTransform panelRectTransform;
    
    public void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {         
            canvasRectTransform = canvas.transform as RectTransform;
            //panelRectTransform = transform.parent as RectTransform;
            panelRectTransform = transform as RectTransform;
        }
    }
   
    public void OnPointerDown(PointerEventData data)
    {
        panelRectTransform.SetAsLastSibling();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out pointerOffset);
    }

    virtual public void OnDrag(PointerEventData data)
    {
        if (panelRectTransform == null)
            return;

        //Vector2 pointerPostion = ClampToWindow(data);
        //Vector2 ert;
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //canvasRectTransform, pointerPostion, data.pressEventCamera, out localPointerPosition
            canvasRectTransform, data.position, data.pressEventCamera, out localPointerPosition
        ))
        {
            //ert = localPointerPosition - pointerOffset;
            //panelRectTransform.localPosition = ert;
            GetComponent<RectTransform>().localPosition = localPointerPosition - pointerOffset;
            //GetComponent<RectTransform>().localPosition
        }
        
    }

    Vector2 ClampToWindow(PointerEventData data)
    {
        Vector2 rawPointerPosition = data.position;

        Vector3[] canvasCorners = new Vector3[4];
        canvasRectTransform.GetWorldCorners(canvasCorners);

        float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
        float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

        Vector2 newPointerPosition = new Vector2(clampedX, clampedY);
        return newPointerPosition;
        
    }
    
    public void hide()
    {
        gameObject.SetActive(false);
    }    
    virtual public void onCloseClick()
    {
        panelRectTransform.SetAsFirstSibling();
        hide();
    }

}