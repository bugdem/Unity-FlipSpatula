using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ClocknestGames.Library.Utils
{
    [System.Serializable]
    public class TouchPointerEvent : UnityEvent<PointerEventData> { }

    public class TouchUIDetection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TouchPointerEvent OnPointerDownEvent;
        public TouchPointerEvent OnPointerUpEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke(eventData);
        }
    }
}