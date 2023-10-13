using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class Character : MonoBehaviour
    {
        public static Character GetFromCollider(Collider collider)
        {
            return collider?.GetComponent<Character>();
        }
    }
}