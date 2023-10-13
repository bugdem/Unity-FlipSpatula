using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Library.Control
{
    public enum PathMovementDirection: byte 
    { 
        Ascending = 1,
        Descending = 2
    }

    public enum PathMovementType: byte
    {
        Loop = 1,
        BackAndForth = 2
    }

    [System.Serializable]
    public class PathMovementEvent : UnityEvent<PathWaypoint> { }

    public class PathMovement : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _pointReachThreshold = .1f;
        [SerializeField] private float _startDelay = 0f;
        [SerializeField] private float _firstWaypointDelay = 0f;
        [SerializeField] private PathMovementDirection _movementDirection = PathMovementDirection.Ascending;
        [SerializeField] private PathMovementType _movementType = PathMovementType.Loop;
        [SerializeField] private bool _rotateToMovingDirection = true;
        [SerializeField] private Vector3 _forwardDirection = new Vector3(0f, 0f, 1f);
        [SerializeField] private bool _activatedByScript = false;
        [SerializeField] private bool _deactivateOnEachPoint = false;
        [SerializeField] private bool _ignoreComparingYAxisOnDestination = false;   // Sometimes object may be higher or lower then destination on Y axis, causing never reaching destination because of gravity. To ignore that from destination reach calculations, make it true.
        [SerializeField] private List<PathWaypoint> _waypoints;
#pragma warning restore 0649

        public PathMovementEvent OnMovingToWaypoint;
        public PathMovementEvent OnWaypointReached;

        public Vector3 OriginalPosition { get; private set; }
        public Vector3 OriginalRotation { get; private set; }
        public float DistanceToNextWaypoint { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsWaiting { get; private set; }
        public bool WasWaitingLastFrame { get; private set; }
        public bool IsMoving { get; private set; }
        public List<PathWaypoint> Waypoints => _waypoints;
        public bool IsOriginalPositionSet { get; private set; }
        public PathWaypoint CurrentWaypoint { get; private set; }
        
        private int _nextWaypointIndex;
        private int _previousWaypointIndex;
        private int _moveDirection;
        private float _waitTime;

        // Start is called before the first frame update
        void Start()
        {
            if (_waypoints.Count <= 0)
                return;

            OriginalPosition = transform.position;
            OriginalRotation = transform.rotation.eulerAngles;

            _waypoints.Insert(0, new PathWaypoint
            {
                Position = Vector3.zero,
                Rotation = transform.localRotation.eulerAngles,
                Delay = _firstWaypointDelay
            });

            _moveDirection = _movementDirection == PathMovementDirection.Ascending ? 1 : -1;

            CurrentWaypoint = _waypoints[0];
            _nextWaypointIndex = 0;

            if (!_activatedByScript)
                IsActive = true;

            IsOriginalPositionSet = true;

            WaypointReached();

            // Add delay to first waypoint on start.
            _waitTime += _startDelay;
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsActive)
                return;

            if (IsWaiting)
            {
                _waitTime -= Time.deltaTime;
                if (_waitTime <= 0)
                {
                    IsWaiting = false;
                    WasWaitingLastFrame = true;
                }

                return;
            }

            if (!IsMoving)
            {
                MoveToNextWaypoint();
                return;
            }

            var destination = OriginalPosition;
            if (_ignoreComparingYAxisOnDestination)
                destination.y = transform.position.y;

            destination += CurrentWaypoint.Position;

            if (WasWaitingLastFrame)
                WasWaitingLastFrame = false;

            if (_rotateToMovingDirection && Vector3.Distance(transform.position, destination) > .1f)
            {
                var rotatedDir = Quaternion.LookRotation((transform.position - destination).normalized) * _forwardDirection;
                transform.rotation = Quaternion.LookRotation(rotatedDir);
            }

            DistanceToNextWaypoint = (transform.position - destination).magnitude;
            if (DistanceToNextWaypoint < _pointReachThreshold)
            {
                WaypointReached();
                return;
            }
        }

        public void Activate(bool status)
        {
            IsActive = status;
        }

        private void MoveToNextWaypoint()
        {
            IsMoving = true;

            OnMovingToWaypoint?.Invoke(CurrentWaypoint);
        }

        private void WaypointReached()
        {
            IsMoving = false;
            IsWaiting = true;
            _waitTime = CurrentWaypoint.Delay;

            OnWaypointReached?.Invoke(CurrentWaypoint);

            GetNextWaypoint();

            if (_deactivateOnEachPoint)
                Activate(false);
        }

        private void GetNextWaypoint()
        {
            _previousWaypointIndex = _nextWaypointIndex;
            _nextWaypointIndex += _moveDirection;
            if (_movementType == PathMovementType.BackAndForth)
            {
                if (_nextWaypointIndex < 0 || _nextWaypointIndex >= _waypoints.Count)
                {
                    _nextWaypointIndex = _previousWaypointIndex;
                    ChangeDirection();
                }
            }
            else
            {
                if (_nextWaypointIndex < 0) _nextWaypointIndex = _waypoints.Count - 1;
                else if (_nextWaypointIndex >= _waypoints.Count) _nextWaypointIndex = 0;
            }
            CurrentWaypoint = _waypoints[_nextWaypointIndex];
        }

        public Vector3 GetCurrentWaypointWorldPosition()
        {
            return GetWaypointWorldPosition(CurrentWaypoint);
        }

        public Vector3 GetWaypointWorldPosition(PathWaypoint waypoint)
        {
            return OriginalPosition + waypoint.Position;
        }

        public PathWaypoint GetPreviousWaypoint()
        {
            return _waypoints[_previousWaypointIndex];
        }

        public Vector3 GetPreviousWaypointWorldPosition()
        {
            return GetWaypointWorldPosition(GetPreviousWaypoint());
        }

        public void ChangeDirection()
        {
            _moveDirection = -_moveDirection;
            GetNextWaypoint();
        }

        public void ChangeDirectionAndActivate(bool activate)
        {
            if (IsMoving || (Waypoints.Count > 2))
                ChangeDirection();

            Activate(activate);
        }

        /// <summary>
        /// On DrawGizmos, we draw lines to show the path the object will follow
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!enabled)
                return;

            if (_waypoints == null)
            {
                return;
            }

            if (_waypoints.Count == 0)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                // if we haven't stored the object's original position yet, we do it
                if (IsOriginalPositionSet == false)
                {
                    OriginalPosition = transform.position;
                    IsOriginalPositionSet = true;
                }
                // if we're not in runtime mode and the transform has changed, we update our position
                if (transform.hasChanged && IsActive == false)
                {
                    OriginalPosition = transform.position;
                }
            }

            // for each point in the path
            for (int i = 0; i < _waypoints.Count; i++)
            {
                // we draw a green point 
                CGDebug.DrawGizmoPoint(GetWaypointWorldPosition(_waypoints[i]), 0.2f, Color.green);

                // we draw a line towards the next point in the path
                if ((i + 1) < _waypoints.Count)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(GetWaypointWorldPosition(_waypoints[i]), GetWaypointWorldPosition(_waypoints[i + 1]));
                }
                // we draw a line from the first to the last point if we're looping
                if ((i == _waypoints.Count - 1) && (_movementType == PathMovementType.Loop))
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(GetWaypointWorldPosition(_waypoints[0]), GetWaypointWorldPosition(_waypoints[i]));
                }
            }

            // if the game is playing, we add a blue point to the destination, and a red point to the last visited point
            if (Application.isPlaying)
            {
                CGDebug.DrawGizmoPoint(GetWaypointWorldPosition(CurrentWaypoint), 0.2f, Color.blue);
                CGDebug.DrawGizmoPoint(GetPreviousWaypointWorldPosition(), 0.2f, Color.red);
            }
#endif
        }
    }

    [System.Serializable]
    public class PathWaypoint
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float Delay = 0f;
    }
}