using UnityEngine;
using System.Collections;

namespace ClocknestGames.Game.Core
{
    public class ObjectSpawner : MonoBehaviour
    {
        public GameObject ObjectPrefab;
        public Vector3 SpawnRadius = Vector3.one;
        public float SpawnStartDelay = 1f;
        public float SpawnInterval = .5f;
        public bool IsInfiniteSpawn = true;
        public int Count = 50;

        public bool IsEnabled => _isEnabled && LevelManager.Instance.IsLevelActive;

        private WaitForSeconds _spawnWFS;
        private WaitUntil _spawnWU;
        private Coroutine _spawnCoroutine;

        private bool _isEnabled = true;

        private void Start()
        {
            _spawnWFS = new WaitForSeconds(SpawnInterval);
            _spawnWU = new WaitUntil(() => IsEnabled);

            _spawnCoroutine = StartCoroutine(StartSpawning());
        }

        private IEnumerator StartSpawning()
        {
            yield return new WaitForSeconds(SpawnStartDelay);

            if (IsInfiniteSpawn)
            {
                while (true)
                {
                    yield return Spawn();
                }
            }
            else
            {
                for (int index = 0; index < Count; index++)
                {
                    yield return Spawn();
                }
            }
        }

        private IEnumerator Spawn()
        {
            yield return _spawnWU;

            var spawnedObject = Instantiate(ObjectPrefab, transform.position + Vector3.Scale(Random.insideUnitSphere, SpawnRadius), Quaternion.identity, transform);
            yield return _spawnWFS;
        }

        public void StopSpawning()
        {
            StopCoroutine(_spawnCoroutine);

            _isEnabled = false;
        }
    }
}