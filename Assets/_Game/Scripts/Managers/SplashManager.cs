using UnityEngine;
using System.Collections;

namespace ClocknestGames.Game.Core
{
    public class SplashManager : MonoBehaviour
    {
        public float MinLoadingTimeToStartGame = 1f;

        private void Start()
        {
            StartCoroutine(ILoadScene());
        }

        private IEnumerator ILoadScene()
        {
            yield return LevelLoadManager.Instance.ILoadAsynchronously(ProgressManager.Instance.GetLevelToLoad(), MinLoadingTimeToStartGame, (progress) => { });
        }
    }
}