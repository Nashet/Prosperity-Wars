using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragButton : MonoBehaviour, IPointerDownHandler
{
    private DragPanel parent;
    public void OnPointerDown(PointerEventData data)
    {
        parent.OnPointerDown(data);
    }
    // Use this for initialization
    void Start()
    {
        parent = GetComponentInParent<DragPanel>();
    }   
}
