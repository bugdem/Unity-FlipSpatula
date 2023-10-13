using UnityEngine;
using UnityEngine.Events;

namespace ClocknestGames.Library.Utils
{
    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider> { }

    public class TriggerNotifier : MonoBehaviour
    {
        public LayerMask TriggerLayer = -1;
        public bool TriggerOnce = false;

        public TriggerEvent OnEnterEvent;
        public TriggerEvent OnExitEvent;

        private bool _triggeredBefore;

        private void OnTriggerEnter(Collider other)
        {
            if (!ConditionMet(other))
                return;

            OnEnterEvent?.Invoke(other);

            OnTrigger();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ConditionMet(other))
                return;

            OnExitEvent?.Invoke(other);
        }

        private bool ConditionMet(Collider other)
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