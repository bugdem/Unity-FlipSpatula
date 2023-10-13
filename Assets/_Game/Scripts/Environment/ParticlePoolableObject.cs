using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class ParticlePoolableObject : PoolableObject
    {
        [SerializeField] private ParticleSystem _particleSystem;

        public override void Execute()
        {
            base.Execute();

            _particleSystem.Play();
        }

        private void OnParticleSystemStopped()
        {
            Destroy();
        }
    }
}