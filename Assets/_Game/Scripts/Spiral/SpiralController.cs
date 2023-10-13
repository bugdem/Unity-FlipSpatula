using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    [RequireComponent(typeof(Spiral), typeof(Rigidbody), typeof(CapsuleCollider))]
    public class SpiralController : MonoBehaviour
    {
        [SerializeField] protected float _spiralGenerateSpeed = .2f;
        [SerializeField] protected Vector3 _spiralSpeedOnRelease = new Vector3(0f, 0f, 5f);
        [SerializeField] protected float _spiralLifeTime = 10f;
        [SerializeField] protected float _spiralReleaseTorque = 20f;
        [SerializeField] protected float _spiralTipAngle = 200f;

        public bool IsSpinning { get; protected set; }
        public bool IsSpiralReleased { get; protected set; }
        public CapsuleCollider Collider => _capsuleCollider;
        public Vector3 Velocity => _rigidbody.velocity;

        protected Rigidbody _rigidbody;
        protected CapsuleCollider _capsuleCollider;
        protected Spiral _spiral;
        protected MeshRenderer _meshRenderer;

        protected Material _mainMaterial;
        protected Material _surfaceMaterial;

        protected Transform _groundPoint;
        protected Vector3 _groundPointOffset;

        protected Vector3 _spiralTip => _spiral.radiusSurfaceVertices[_spiral.radiusSurfaceVertices.Length - 2];

        protected Vector3 _previousVelocity;
        protected Vector3 _previousAngularVelocity;

        protected virtual void Awake()
        {
            _spiral = GetComponent<Spiral>();
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();

            //_meshRenderer.materials = new Material[]
            //{
            //    new Material(_meshRenderer.materials[0]),
            //    new Material(_meshRenderer.materials[1]),
            //};

            //_capsuleCollider.enabled = true;
            //_capsuleCollider.isTrigger = false;
            _rigidbody.isKinematic = true;
        }

        protected virtual void FixedUpdate()
        {
            _previousVelocity = _rigidbody.velocity;
            _previousAngularVelocity = _rigidbody.angularVelocity;
        }

        protected virtual void Update()
        {
            if (IsSpinning)
            {
                Vector3 spiralTipPreviousLocalPos = _spiralTip;

                _spiral.length += _spiralGenerateSpeed * Time.deltaTime;
                _spiral.Refresh();

                //Vector3 spiralTipLastLocalPos = _spiralTip;
                //Vector3 spiralTipPoint = _groundPoint.transform.TransformPoint(_groundPointOffset);

                Transform refTransform = GameplayController.Instance.Follower.transform;
                Vector3 groundPosition = _groundPoint.TransformPoint(_groundPointOffset);
                Vector3 targetSpiralTipDir = Quaternion.Euler(_spiralTipAngle, 0f, 0f) * refTransform.up;

                Vector3 spiralTipPoint = _spiral.transform.position + targetSpiralTipDir;
                Vector3 vertexPoint = _spiral.transform.TransformPoint(_spiralTip);
                vertexPoint.x = _spiral.transform.position.x;
                spiralTipPoint.x = _spiral.transform.position.x;

                Vector3 vertexPointLocal = _spiral.transform.position - vertexPoint;
                Vector3 spiralTipPointLocal = _spiral.transform.position - spiralTipPoint;

                float angle = CGMaths.SignedAngleBetween(vertexPointLocal, spiralTipPointLocal, Vector3.right);
                _spiral.transform.Rotate(Vector3.up, angle);

                // Reset some values after rotation
                vertexPoint = _spiral.transform.TransformPoint(_spiralTip);
                vertexPoint.x = _spiral.transform.position.x;
                vertexPointLocal = _spiral.transform.position - vertexPoint;

                Vector3 spiralPosition = groundPosition;
                spiralPosition += refTransform.right * _spiral.height * .5f;
                spiralPosition += refTransform.forward * Mathf.Abs(vertexPointLocal.z);
                spiralPosition += refTransform.up * Mathf.Abs(vertexPointLocal.y) * Spatula.Instance.FollowerUpVectorSign;
                _spiral.transform.position = spiralPosition;

                /*
                Vector3 spiralTipLastLocalPos = _spiralTip;
                float angle = Vector3.Angle(spiralTipPreviousLocalPos, spiralTipLastLocalPos);
                Vector3 position = _groundPoint.TransformPoint(_groundPointOffset);
                position += _groundPoint.transform.right * _spiral.height * .5f;
                position += -_groundPoint.transform.forward * spiralTipLastLocalPos.magnitude;





                _spiral.transform.position = position;
                _spiral.transform.Rotate(Vector3.up, angle);

                Vector3 spiralTipPoint = _groundPoint.transform.TransformPoint(_groundPointOffset);
                Vector3 vertexPoint = _spiral.transform.TransformPoint(_spiralTip);
                vertexPoint.x = spiralTipPoint.x;

                float ang = CGMaths.SignedAngleBetween(_spiral.transform.position - vertexPoint, _spiral.transform.position - spiralTipPoint, Vector3.right);
                _spiral.transform.Rotate(Vector3.up, ang);

                // _spiral.transform.Rotate(Vector3.up, angleDifference);
                Debug.DrawLine(_spiral.transform.position, vertexPoint, Color.yellow);
                Debug.DrawLine(_spiral.transform.position, spiralTipPoint, Color.red);
                CGDebug.DrawBox(_spiral.transform.TransformPoint(_spiralTip), Vector3.one * .5f, Quaternion.identity, Color.red);
                */
            }
        }

        public virtual void RestorePhysics()
        {
            _rigidbody.velocity = _previousVelocity;
            _rigidbody.angularVelocity = _previousAngularVelocity;
        }

        public static SpiralController GetFromCollider(Collider collider)
        {
            return collider.GetComponent<SpiralController>();
        }

        public virtual void Spin(Transform groundPoint, Vector3 groundPointOffset, Material mainMaterial, Material surfaceMaterial, float spiralWidth)
        {
            if (IsSpiralReleased) return;

            IsSpinning = true;

            _groundPoint = groundPoint;
            _groundPointOffset = groundPointOffset;
            _mainMaterial = mainMaterial;
            _surfaceMaterial = surfaceMaterial;

            //_meshRenderer.material.EnableKeyword("_NORMALMAP");
            //_meshRenderer.material.EnableKeyword("_METALLICGLOSSMAP");
            // _meshRenderer.materials[0].SetTexture("_BaseMap", _mainMaterial);
            // _meshRenderer.materials[1].SetTexture("_BaseMap", _surfaceMaterial);

            var mats = _meshRenderer.materials;

            if (_mainMaterial != null) mats[0] = new Material(_mainMaterial);
            if (_surfaceMaterial != null) mats[1] = new Material(_surfaceMaterial);

            _meshRenderer.materials = mats;

            _spiral.height = spiralWidth;

            // Position spiral on ground point.
            Vector3 position = _groundPoint.TransformPoint(_groundPointOffset);
            position += _groundPoint.transform.right * spiralWidth * .5f;
            position += -_groundPoint.transform.forward * _spiral.finalPosition.magnitude;
            _spiral.transform.position = position;

            // Rotate to match spiral tip with ground point.
            Vector3 spiralTip = _spiral.transform.TransformPoint(_spiral.finalPosition);
            Vector3 groundPosition = groundPoint.position + groundPointOffset;
            groundPosition.y = spiralTip.y;
            _spiral.transform.Rotate(Vector3.up, Vector3.Angle(groundPosition, spiralTip));

            _spiral.Refresh();
        }

        public virtual void Release()
        {
            if (IsSpiralReleased) return;

            IsSpinning = false;
            IsSpiralReleased = true;

            _capsuleCollider.enabled = true;
            _capsuleCollider.isTrigger = false;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = _spiralSpeedOnRelease;
            _rigidbody.AddTorque(Vector3.right * _spiralReleaseTorque, ForceMode.Impulse);

            Destroy(gameObject, _spiralLifeTime);
        }
    }
}