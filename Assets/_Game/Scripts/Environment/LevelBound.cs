using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class LevelBound : MonoBehaviour
    {
        public virtual void OnCollided(Collision collision)
        {
            GameplayController.Instance.OnLevelBoundsEntered(collision.collider, this);
        }

        public virtual void OnTriggered(Collider collider)
        {
            GameplayController.Instance.OnLevelBoundsEntered(collider, this);
        }
    }
}