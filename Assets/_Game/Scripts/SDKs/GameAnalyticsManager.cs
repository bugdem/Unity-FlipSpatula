using ClocknestGames.Library.Utils;
using ClocknestGames.Game.Core;
using GameAnalyticsSDK;

namespace ClocknestGames.Game.ThirdParty
{
    public class GameAnalyticsManager : PersistentSingleton<GameAnalyticsManager>, EventListener<LevelEvent>
    {
        protected override void Awake()
        {
            base.Awake();
            if (!base._enabled) return;

            GameAnalytics.Initialize();
        }

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
            // Scene Index on level complete is 1 less. Reason is progress manager executes before this class and GetCurrentLevelIndex is next level's index.
            var currentSceneIndex = ProgressManager.Instance.GetCurrentLevelIndex();

            switch (levelEvent.EventType)
            {
                case LevelEventType.Started: GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, currentSceneIndex.ToString()); break;
                case LevelEventType.Completed: GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, (currentSceneIndex - 1).ToString()); break;
                case LevelEventType.Failed: GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, currentSceneIndex.ToString()); break;
            }
        }
    }
}