using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Library.Control
{
    public enum MovingPlatformAccelerationType : byte
    {
        ConstantSpeed,
        EaseOut,
        AnimationCurve
    }

    [RequireComponent(typeof(PathMovement))]
    public class MovingPlatform : MonoBehaviour
    {
#pragma warning disable 0649, 0414
        [SerializeField] private float _movementSpeed = 10f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private MovingPlatformAccelerationType _accelerationType;
        [SerializeField] private AnimationCurve _acceleration;
#pragma warning restore 0649, 0414

        public MovingPlatformAccelerationType AccelerationType => _accelerationType;

        private PathMovement _pathMovement;

        private void Awake()
        {
            _pathMovement = GetComponent<PathMovement>();
        }

        private void Update()
        {
            if (_pathMovement.IsActive)
                MoveAlongThePath();
        }

        /// <summary>
        /// Moves the object along the path according to the specified movement type.
        /// </summary>
        public virtual void MoveAlongThePath()
        {
            switch (_accelerationType)
            {
                case MovingPlatformAccelerationType.ConstantSpeed:
                    transform.position = Vector3.MoveTowards(transform.position, _pathMovement.GetCurrentWaypointWorldPosition(), Time.deltaTime * _movementSpeed);
                    break;

                case MovingPlatformAccelerationType.EaseOut:
                    transform.position = Vector3.Lerp(transform.position, _pathMovement.GetCurrentWaypointWorldPosition(), Time.deltaTime * _movementSpeed);
                    break;

                case MovingPlatformAccelerationType.AnimationCurve:
                    float distanceBetweenPoints = Vector3.Distance(_pathMovement.GetPreviousWaypoint().Position, _pathMovement.CurrentWaypoint.Position);

                    if (distanceBetweenPoints <= 0)
                    {
                        return;
                    }

                    float remappedDistance = 1 - CGMaths.Remap(_pathMovement.DistanceToNextWaypoint, 0f, distanceBetweenPoints, 0f, 1f);
                    float speedFactor = _acceleration.Evaluate(remappedDistance);

                    transform.position = Vector3.MoveTowards(transform.position, _pathMovement.GetCurrentWaypointWorldPosition(), Time.deltaTime * _movementSpeed * speedFactor);
                    break;
            }
        }
    }
}