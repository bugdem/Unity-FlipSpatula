using UnityEngine;
using UnityEngine.Events;

namespace ClocknestGames.Library.Utils
{
    public class AnimationNotifier : MonoBehaviour
    {
        public UnityEvent OnAnimationEvent1;
        public UnityEvent OnAnimationEvent2;
        public UnityEvent OnAnimationEvent3;

        public void OnAnimationEvent1Triggered()
        {
            OnAnimationEvent1?.Invoke();
        }

        public void OnAnimationEvent2Triggered()
        {
            OnAnimationEvent2?.Invoke();
        }

        public void OnAnimationEvent3Triggered()
        {
            OnAnimationEvent3?.Invoke();
        }
    }
}