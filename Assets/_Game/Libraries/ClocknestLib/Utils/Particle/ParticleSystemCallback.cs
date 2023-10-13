using UnityEngine;
using UnityEngine.Events;

namespace ClocknestGames.Library.Utils
{
    public class ParticleSystemCallback : MonoBehaviour
    {
        public UnityEvent OnParticleStopped;

        private void OnParticleSystemStopped()
        {
            OnParticleStopped?.Invoke();
        }
    }
}