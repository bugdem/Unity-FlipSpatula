using ClocknestGames.Library.Utils;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCube : MonoBehaviour
{
    [SerializeField] protected Vector2YZ _force = new Vector2YZ(30f, 15f);
    [SerializeField] protected Vector2 _torque = new Vector2(10, 10f);
    public Transform COMStatic;
    public Transform COMDynamic;
    public float COMDynamicTargetY = 6f;

    protected Rigidbody _rigidbody;

    protected bool _isMoving = false;
    protected bool _collisionEnabled;
    protected float _collisionEnabledTimer;
    protected Vector3 _currentVelocity;
    protected float _currentRotationVelocity;

    protected float _comDynamicStartY;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _comDynamicStartY = COMDynamic.transform.localPosition.y;

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
            SetCenterOfMass();
        }
    }

    protected void SetCenterOfMass()
    {
        float currentAngle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.right);
        currentAngle = CGMaths.PositiveAngle(currentAngle);

        if (currentAngle >= 180f && currentAngle < 360f)
        {
            var localPosition = COMDynamic.transform.localPosition;
            localPosition.y = CGMaths.Remap(currentAngle, 180f, 360f, _comDynamicStartY, COMDynamicTargetY);
            COMDynamic.transform.localPosition = localPosition;
        }
        else
        {
            var localPosition = COMDynamic.transform.localPosition;
            localPosition.y = CGMaths.Remap(currentAngle.Clamp(0f, 90f), 0f, 90f, COMDynamicTargetY, _comDynamicStartY);
            localPosition.z = CGMaths.Remap(currentAngle, 0f, 180f, 5f, 0f);
            COMDynamic.transform.localPosition = localPosition;
        }

        _rigidbody.centerOfMass = (COMDynamic.transform.localPosition + COMStatic.transform.localPosition) * .5f;
    }

    protected void SetMoving(bool status)
    {
        _isMoving = status;
        if (_isMoving)
        {
            Debug.Break();

            _collisionEnabled = false;
            _collisionEnabledTimer = Time.time;

            _rigidbody.detectCollisions = false;
            _rigidbody.isKinematic = false;

            SetCenterOfMass();

            Vector3 torque = new Vector3(_torque.x, _torque.y, 0f);
            Vector3 force = new Vector3(0f, _force.y, _force.z);
            _rigidbody.AddForce(force, ForceMode.Impulse);
            _rigidbody.AddTorque(torque, ForceMode.Impulse);
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
