using UnityEngine;
using System.Collections;
using ClocknestGames.Library.Editor;

namespace ClocknestGames.Game.Core
{
    public class CameraSmoothFollow : MonoBehaviour
    {
        public Transform Target;

        public float OffsetFromMiddle = 20f;
        public float RotationDamping = 1f;
        public bool AutoDistance = true;
        [Condition("AutoDistance", false, true)]
        public float ManualDistance = 100f;

        private float _wantedRotationAngle;
        private float _currentRotationAngle;
        private Quaternion _currentRotation;

        private Quaternion _targetStartRotation;
        private Quaternion _startRotation;
        private float _autoDistance;
        private float _distance;
        private Vector3 _offset;

        private void Start()
        {
            _targetStartRotation = Target.rotation;
            _startRotation = transform.rotation;
            _offset = Target.position - transform.position;
            _autoDistance = Vector3.Distance(Target.position, transform.position);
        }

        void LateUpdate()
        {
            Follow();
        }

        private void Follow()
        {
            _distance = AutoDistance ? _autoDistance : ManualDistance;

            // Calculate the current rotation angles
            _wantedRotationAngle = Target.eulerAngles.y + _startRotation.eulerAngles.y;
            _currentRotationAngle = transform.eulerAngles.y;

            // Damp the rotation around the y-axis
            _currentRotationAngle = Mathf.LerpAngle(_currentRotationAngle, _wantedRotationAngle, RotationDamping * Time.deltaTime);

            // Convert the angle into a rotation
            _currentRotation = Quaternion.Euler(transform.eulerAngles.x, _currentRotationAngle, transform.eulerAngles.z);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = Target.position;
            transform.position -= _currentRotation * Vector3.forward * _distance; // + offsetDir;

            // Always look at the target
            transform.LookAt(Target);

            var offsetDir = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward * OffsetFromMiddle;
            transform.position -= offsetDir;
        }

        public void StopDamping()
        {
            var eulerTarget = transform.eulerAngles;
            eulerTarget.y = Target.eulerAngles.y + _startRotation.eulerAngles.y;
            transform.eulerAngles = eulerTarget;

            Follow();
        }
    }
}