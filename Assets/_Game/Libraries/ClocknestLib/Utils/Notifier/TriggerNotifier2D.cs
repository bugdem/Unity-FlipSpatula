using UnityEngine;
using UnityEngine.Events;

namespace ClocknestGames.Library.Utils
{
    [System.Serializable]
    public class TriggerEvent2D : UnityEvent<Collider2D> { }

    public class TriggerNotifier2D : MonoBehaviour
    {
        public LayerMask TriggerLayer = -1;
        public bool TriggerOnce = false;

        public TriggerEvent2D OnEnterEvent;
        public TriggerEvent2D OnExitEvent;

        private bool _triggeredBefore;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!ConditionMet(collision))
                return;

            OnEnterEvent?.Invoke(collision);

            OnTrigger();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!ConditionMet(collision))
                return;

            OnExitEvent?.Invoke(collision);
        }

        private bool ConditionMet(Collider2D other)
        {
            if (TriggerOnce && _triggeredBefore)
                return false;

            if (!TriggerLayer.Contains(other.gameObject.layer))
                return false;

            return true;
        }

        private void OnTrigger()
        {
            _triggeredBefore = true;
        }
    }
}