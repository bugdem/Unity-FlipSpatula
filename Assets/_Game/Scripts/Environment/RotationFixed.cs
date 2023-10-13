using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class RotationFixed : MonoBehaviour
    {
        public Vector3 Rotation = new Vector3(0f, 0f, 0f);

        private void LateUpdate()
        {
            transform.rotation = Quaternion.Euler(Rotation);
        }
    }
}