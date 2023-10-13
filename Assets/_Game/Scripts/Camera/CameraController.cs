using UnityEngine;
using ClocknestGames.Library.Utils;
using ClocknestGames.Library.Core;

namespace ClocknestGames.Game.Core
{
    public class CameraController : Singleton<CameraController>
    {
        public Camera MainCamera;
        public CameraShake Shaker;
        public CameraFollow Follower;

        public Vector3 GetWorldPositionOfTouch()
        {
            Vector3 touchPosition = TouchUIController.Instance.GetTouchPosition();
            touchPosition.z = -transform.position.z;

            var worldPosition = MainCamera.ScreenToWorldPoint(touchPosition);
            worldPosition.z = 0f;

            return worldPosition;
        }
    }
}