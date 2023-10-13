using UnityEngine;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public GameSettings Settings;

        protected override void Awake()
        {
            base.Awake();
            if (!enabled) return;

            Application.targetFrameRate = Settings.TargetFrameRate;
            // Input.multiTouchEnabled = false;
            DG.Tweening.DOTween.SetTweensCapacity(1250, 50);

#if UNITY_IOS
            // Check the user's consent status.
            // If the status is undetermined, display the request request: 
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
        }
    }
}