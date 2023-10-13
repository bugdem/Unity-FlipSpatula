using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class Surface : MonoBehaviour
    {
        [SerializeField] protected bool _isStickable = true;

        public bool IsStickable => _isStickable;

        public static Surface GetFromCollider(Collider collider)
        {
            return Surface.GetFromCollider<Surface>(collider);
        }

        public static T GetFromCollider<T>(Collider collider) where T : Surface
        {
            return collider.GetComponent<T>();
        }
    }
}