using UnityEngine;
using UnityEngine.Events;

namespace ClocknestGames.Library.Utils
{
    [System.Serializable]
    public class CollisionEvent2D : UnityEvent<Collision2D> { }

    public class CollisionNotifier2D : MonoBehaviour
    {
        public CollisionEvent2D OnEnterEvent;
        public CollisionEvent2D OnExitEvent;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnEnterEvent?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            OnExitEvent?.Invoke(collision);
        }
    }
}