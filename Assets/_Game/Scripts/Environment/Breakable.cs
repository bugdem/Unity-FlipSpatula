using ClocknestGames.Library.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class Breakable : MonoBehaviour
    {
        [Header("Breakable")]
        [SerializeField] protected float _force = 2f;
        [SerializeField] protected float _forceUpwards = 0f;
        [SerializeField] protected float _maxForce = 40f;
        [SerializeField] protected float _maxForceUpwards = 10f;
        [SerializeField] protected float _collisionThresholdToBreak = 2f;
        [SerializeField] protected float _spinningSpiralForce = 20f;
        [SerializeField] protected Vector3 _forcePointOffset;
        [SerializeField] protected GameObject _collisionParticle;
        [SerializeField] protected Collider _breakTrigger;
        [SerializeField] protected List<Collider> _colliders;
        [SerializeField] protected List<Rigidbody> _breakableParts;

        [Header("Rewards")]
        [SerializeField] protected Item _item;
        [SerializeField] protected Vector3 _pointLocalPos;

        public bool IsBroken { get; protected set; }

        /*
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (IsBroken) return;

            var spiralController = SpiralController.GetFromCollider(collision.collider);
            if (spiralController != null)
            {
                float contactForce = collision.impulse.magnitude;
                Vector3 contactPoint = collision.contacts[0].point;
                Break(spiralController, contactForce, contactPoint);
            }
        }
        */

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (IsBroken) return;

            var spiralController = SpiralController.GetFromCollider(other);
            if (spiralController != null)
            {
                float contactForce = spiralController.Velocity.magnitude;
                Vector3 contactPoint = _breakTrigger.GetClosestPoint(other);
                if (spiralController.IsSpinning)
                    contactForce = _spinningSpiralForce;

                Break(spiralController, contactForce, contactPoint);
            }
        }

        protected virtual void Break(SpiralController spiralController, float contactForce, Vector3 contactPoint)
        {
            if (IsBroken) return;

            float collisionMagnitute = contactForce;
            if (collisionMagnitute < _collisionThresholdToBreak)
                return;

            // spiralController.RestorePhysics();

            IsBroken = true;

            for (int index = 0; index < _colliders.Count; index ++)
            {
                Destroy(_colliders[index]);
            }

            Vector3 forcePoint = contactPoint + _forcePointOffset;
            float force = (collisionMagnitute * _force).ClampMax(_maxForce);
            float forceUpwards = (collisionMagnitute * _forceUpwards).ClampMax(_maxForceUpwards);

            foreach (var breakablePart in _breakableParts)
            {
                breakablePart.isKinematic = false;
                breakablePart.AddForce((breakablePart.transform.position - forcePoint).normalized * force + Vector3.up * forceUpwards, ForceMode.Impulse);

                Destroy(breakablePart.gameObject, 10f);
                // breakablePart.AddExplosionForce(force, forcePoint, _forceRadius, forceUpwards, ForceMode.Impulse);
            }

            Vector3 pointPosition = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
            GameplayController.Instance.ShowItemUI(pointPosition, (int)_item.Quantity);
            LevelManager.Instance.PickItem(new PickableItemEvent(_item));

            HapticManager.Instance.HapticOnBreakable();

            if (_collisionParticle != null)
            {
                var particle = Instantiate(_collisionParticle, GameplayController.Instance.LevelContainer);
                particle.transform.position = forcePoint;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 position = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
            Gizmos.DrawWireSphere(position, .5f);
        }
    }
}