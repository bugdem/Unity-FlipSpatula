using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class CharacterBody : MonoBehaviour
    {
        public Character Owner;
        public Collider Collider;

        private void OnValidate()
        {
            if (Owner == null)
            {
                Owner = transform.GetComponentInParentRecursive<Character>();
            }

            if (Collider == null)
            {
                Collider = transform.GetComponent<Collider>();
            }
        }
    }
}