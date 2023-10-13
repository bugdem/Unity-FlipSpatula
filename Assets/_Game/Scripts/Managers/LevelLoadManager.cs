using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class LevelLoadManager : PersistentSingleton<LevelLoadManager>
    {
        private AsyncOperation _levelLoadOperation;
        private bool _levelLoading = false;

        public Scene GetCurrentScene()
        {
            return SceneManager.GetActiveScene();
        }

        public string GetCurrentSceneName()
        {
            return GetCurrentScene().name;
        }

        public string GetSceneNameByIndex(int index)
        {
            string pathToScene = SceneUtility.GetScenePathByBuildIndex(index);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(pathToScene);
            return sceneName;
        }

        public void LoadLevel(string levelName, float loadDelay)
        {
            if (_levelLoading || string.IsNullOrEmpty(levelName))
                return;

            _levelLoading = true;

            StartCoroutine(ILoad(levelName, loadDelay));
        }

        /// <summary>
        /// Loads the scene asynchronously with delay.
        /// IMPORTANT: Current version causes fps spike on call(loading.lock persistentmanager).
        /// </summary>
        public virtual IEnumerator ILoadAsynchronously(string levelName, float loadDelay, Action<float> progressAction)
        {
            yield return null;

            Debug.Log("Loading Level: " + levelName);

            // we start loading the scene
            _levelLoadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
            _levelLoadOperation.allowSceneActivation = false;
            float startTime = Time.time;

            // while the scene loads, we assign its progress to a target that we'll use to fill the progress bar smoothly
            while (_levelLoadOperation.progress < 0.9f || Time.time - startTime < loadDelay)
            {
                progressAction?.Invoke(_levelLoadOperation.progress);

                yield return null;
            }

            progressAction?.Invoke(_levelLoadOperation.progress);

            // we switch to the new scene
            _levelLoadOperation.allowSceneActivation = true;

            _levelLoading = false;
        }

        /// <summary>
        /// Loads the scene with delay.
        /// </summary>
        protected virtual IEnumerator ILoad(string levelName, float loadDelay)
        {
            yield return null;

            Debug.Log("Loading Level: " + levelName);

            yield return new WaitForSeconds(loadDelay);

            SceneManager.LoadScene(levelName, LoadSceneMode.Single);

            _levelLoading = false;
        }
    }
}