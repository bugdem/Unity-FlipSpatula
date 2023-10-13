using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ClocknestGames.Game.Core
{
    public class LevelFinish : MonoBehaviour
    {
        protected List<LevelFinishPoint> _points;
        protected LevelFinishPoint _currentLevelFinishPoint;

        protected virtual void Awake()
        {
            _points = GetComponentsInChildren<LevelFinishPoint>().ToList();
            foreach (var point in _points)
                point.Register(OnLevelFinishPointEntered);
        }

        public virtual void OnLevelFinishPointEntered(LevelFinishPoint levelFinishPoint)
        {
            // This will be handled by spatula, not here.
            return;

            if (_currentLevelFinishPoint != null) return;

            _currentLevelFinishPoint = levelFinishPoint;

            GameplayController.Instance.LevelSuccess(_currentLevelFinishPoint.Scale);
        }

        public virtual void OnLevelFinishEntered(Collider collider)
        {
            GameplayController.Instance.OnLevelFinishEntered(collider, this);
        }
    }
}