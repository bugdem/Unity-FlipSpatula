using ClocknestGames.Library.Utils;
using ClocknestGames.Game.Core;

namespace ClocknestGames.Game.ThirdParty
{
    public class PublisherSDKManager : PersistentSingleton<PublisherSDKManager>, EventListener<LevelEvent>
    {
        private void OnEnable()
        {
            this.EventStartListening<LevelEvent>();
        }

        private void OnDisable()
        {
            this.EventStopListening<LevelEvent>();
        }

        public void OnCGEvent(LevelEvent levelEvent)
        {
            /*
            // Scene Index on level complete is 1 less. Reason is progress manager executes before this class and GetCurrentLevelIndex is next level's index.
            var currentSceneIndex = ProgressManager.Instance.GetCurrentLevelIndex();

            switch (levelEvent.EventType)
            {
                case LevelEventType.Started: PublisherSDK.LevelStarted(currentSceneIndex); break;
                case LevelEventType.Completed: PublisherSDK.LevelCompleted(currentSceneIndex - 1); break;
                case LevelEventType.Failed: PublisherSDK.LevelFailed(currentSceneIndex); break;
            }
            */
        }
    }
}