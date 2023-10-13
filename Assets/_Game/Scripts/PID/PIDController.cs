using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDController : MonoBehaviour
{
    [SerializeField]
    [Range(0, 5000)]
    private float _thrust = 1000f;
    [SerializeField]
    private float _maxAngularVelocity = 20;
    [SerializeField]
    private Vector3 _targerRotationEuler = new Vector3(60f, 0f, 0f);

    [SerializeField]
    [Range(-10, 10)]
    private float _xAxisP, _xAxisI, _xAxisD;

    [SerializeField]
    [Range(-10, 10)]
    private float _yAxisP, _yAxisI, _yAxisD;

    [SerializeField]
    [Range(-10, 10)]
    private float _zAxisP, _zAxisI, _zAxisD;

    private PID _xAxisPIDController;
    private PID _yAxisPIDController;
    private PID _zAxisPIDController;

    private Rigidbody _rb;

    private bool _isEnabled;
    private float _enableCurrentDelay;
    private const float _enableDelay = .5f;

    private bool _canControl => _isEnabled && _enableCurrentDelay <= 0f;

    void Start()
    {
        //_pidController = gameObject.GetComponents<PID>()[0];
        _rb = GetComponent<Rigidbody>();
        _rb.maxAngularVelocity = _maxAngularVelocity;
        _xAxisPIDController = new PID(_xAxisP, _xAxisI, _xAxisD);
        _yAxisPIDController = new PID(_yAxisP, _yAxisI, _yAxisD);
        _zAxisPIDController = new PID(_zAxisP, _zAxisI, _zAxisD);
    }

    private void Update()
    {
        if (_enableCurrentDelay > 0f)
            _enableCurrentDelay -= Time.deltaTime;

        if (!_canControl) return;

        _xAxisPIDController.Kp = _xAxisP;
        _xAxisPIDController.Ki = _xAxisI;
        _xAxisPIDController.Kd = _xAxisD;

        _yAxisPIDController.Kp = _yAxisP;
        _yAxisPIDController.Ki = _yAxisI;
        _yAxisPIDController.Kd = _yAxisD;

        _zAxisPIDController.Kp = _zAxisP;
        _zAxisPIDController.Ki = _zAxisI;
        _zAxisPIDController.Kd = _zAxisD;
    }

    void FixedUpdate()
    {
        if (!_canControl) return;

        //Get the required rotation based on the target position - we can do this by getting the direction
        //from the current position to the target. Then use rotate towards and look rotation, to get a quaternion thingy.
        Quaternion targetRotation = Quaternion.Euler(_targerRotationEuler);

        //Figure out the error for each asix
        float xAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.x);
        float xTorqueCorrection = _xAxisPIDController.GetOutput(xAngleError, Time.fixedDeltaTime);

        float yAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, targetRotation.eulerAngles.y);
        float yTorqueCorrection = _yAxisPIDController.GetOutput(yAngleError, Time.fixedDeltaTime);

        float zAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, targetRotation.eulerAngles.z);
        float zTorqueCorrection = _zAxisPIDController.GetOutput(zAngleError, Time.fixedDeltaTime);

        _rb.AddRelativeTorque((xTorqueCorrection * Vector3.right) + (yTorqueCorrection * Vector3.up) + (zTorqueCorrection * Vector3.forward));
    }

    public void EnableController(bool enabled)
    {
        _isEnabled = enabled;
        _enableCurrentDelay = _enableDelay;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Quaternion.Euler(_targerRotationEuler) * Vector3.forward * 5f);
    }
}