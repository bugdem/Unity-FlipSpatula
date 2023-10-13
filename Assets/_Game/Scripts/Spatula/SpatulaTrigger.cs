using UnityEngine;

namespace ClocknestGames.Game.Core
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class SpatulaTrigger : MonoBehaviour
    {
        [SerializeField] protected Spatula _spatula;

        public Vector3 CurrentVelocity { get; protected set; }

        protected Rigidbody _rigidbody;
        protected GameObject _placeHolderObj;
        protected Vector3 _previousPosition;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _placeHolderObj = new GameObject($"{gameObject.name}_Placeholder");
            _placeHolderObj.transform.SetParent(transform.parent);
            _placeHolderObj.transform.position = transform.position;
            _placeHolderObj.transform.rotation = transform.rotation;

            transform.SetParent(null);
        }

        protected virtual void Update()
        {
            _previousPosition = transform.position;

            transform.position = _placeHolderObj.transform.position;
            transform.rotation = _placeHolderObj.transform.rotation;

            CurrentVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        }

        public virtual void ResetVelocity()
        {
            CurrentVelocity = Vector3.zero;

            _previousPosition = transform.position;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            _spatula.OnExternalCollisionEnter(collision);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            _spatula.OnExternalTriggerEnter(collider);
        }
    }
}