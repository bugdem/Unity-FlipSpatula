﻿using UnityEngine;
using ClocknestGames.Library.Utils;
using Facebook.Unity;

namespace ClocknestGames.Game.ThirdParty
{
    public class FacebookManager : PersistentSingleton<FacebookManager>
    {
        // Awake function from Unity's MonoBehavior
        protected override void Awake()
        {
            base.Awake();
            if (!base._enabled) return;

            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
        }

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...
            }
            else
            {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }
    }
}