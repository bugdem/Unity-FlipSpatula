using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public float SmoothTime = 0.3f;

        public bool FollowTarget { get; set; } = true;

        private Vector3 _currentVelocity;
        private Vector3 _startOffset;
        private Vector3 _currentOffset;
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
            _startOffset = transform.position - Target.position;
            _currentOffset = _startOffset;
        }

        private void LateUpdate()
        {
            if (!FollowTarget || Target == null)
                return;

            // Define a target position above and behind the target transform
            Vector3 targetPosition = Target.position + _currentOffset;
            targetPosition.y = transform.position.y;

            // Smoothly move the camera towards that target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, SmoothTime);
        }

        public void SetPosition(Vector3 newPosition)
        {
            transform.position = newPosition;
            SetOffset(newPosition - Target.position);
        }

        public void AddOffset(Vector3 offset)
        {
            _currentOffset += offset;
        }

        public void ResetOffset()
        {
            _currentOffset = _startOffset;
        }

        public void SetOffset(Vector3 offset)
        {
            _currentOffset = offset;
        }

        public Vector3 GetOffset()
        {
            return _currentOffset;
        }
    }
}