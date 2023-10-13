using ClocknestGames.Library.Utils;
using UnityEngine;

public class TestCube2 : MonoBehaviour
{
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Vector2YZ _force = new Vector2YZ(30f, 15f);
    [SerializeField] protected Vector2 _torque = new Vector2(10, 10f);

    protected Rigidbody _rigidbody;

    protected bool _isMoving = false;
    protected bool _collisionEnabled;
    protected float _collisionEnabledTimer;
    protected Vector3 _currentVelocity;
    protected float _currentRotationVelocity;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        SetMoving(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetMoving(true);
        }

        if (!_collisionEnabled)
        {
            if (Time.time - _collisionEnabledTimer > .5f)
            {
                _collisionEnabled = true;
                _rigidbody.detectCollisions = true;
            }
        }

        if (_isMoving)
        {

        }
    }

    private void LateUpdate()
    {
        // transform.parent.transform.position += transform.localPosition;
        // transform.localPosition = Vector3.zero;
    }

    protected void SetMoving(bool status)
    {
        _isMoving = status;
        if (_isMoving)
        {
            _collisionEnabled = false;
            _collisionEnabledTimer = Time.time;

            _rigidbody.detectCollisions = false;
            _rigidbody.isKinematic = false;

            _animator.SetTrigger("Flip");
        }
        else
        {
            _rigidbody.isKinematic = true;

            _currentVelocity = Vector3.zero;
            _currentRotationVelocity = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        SetMoving(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.localToWorldMatrix.MultiplyPoint3x4(GetComponent<Rigidbody>().centerOfMass), .5f);
    }
}
