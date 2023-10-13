using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class Rotator : MonoBehaviour
    {
        public float Speed = 20f;
        public Vector3 RotationAxis = new Vector3(0f, 0f, 1f);

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(RotationAxis.normalized * Speed * Time.deltaTime);
        }
    }
}