using UnityEngine;
using UnityEngine.Events;

namespace ClocknestGames.Library.Utils
{
    [System.Serializable]
    public class CollisionEvent : UnityEvent<Collision> { }

    public class CollisionNotifier : MonoBehaviour
    {
        public CollisionEvent OnEnterEvent;
        public CollisionEvent OnExitEvent;

        private void OnCollisionEnter(Collision collision)
        {
            OnEnterEvent?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            OnExitEvent?.Invoke(collision);
        }
    }
}