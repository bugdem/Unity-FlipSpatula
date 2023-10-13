using UnityEngine;

namespace ClocknestGames.Game.Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Clocknest Games/Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Mobile")]
        public int TargetFrameRate = 30;

        [Header("Mobile - IOS")]
        public string IOSAppId;
        public string IOSAppURI
        {
            get
            {
                return string.Format("itms-apps://itunes.apple.com/app/id{0}", IOSAppId);
            }
        }

        [Header("Mobile - Android")]
        public string AndroidAppId;
        public string AndroidAppURI
        {
            get
            {
                return string.Format("market://details?id={0}", AndroidAppId);
            }
        }
    }
}