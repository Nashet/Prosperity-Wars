using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
/// <summary>
/// used together with URLOpener
/// </summary>
public class PressHandler : MonoBehaviour, IPointerDownHandler
{   
    [Serializable]    
    public class ButtonPressEvent : UnityEvent { }

    public ButtonPressEvent OnPress = new ButtonPressEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPress.Invoke();
    }
}