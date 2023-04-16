using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class CustomOnScreenButtonPublic : OnScreenButton, IPointerUpHandler, IPointerDownHandler
{
    public void SendValueToControl2<TValue>(TValue value)
        where TValue : struct
    {
        SendValueToControl(value);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
    }
}