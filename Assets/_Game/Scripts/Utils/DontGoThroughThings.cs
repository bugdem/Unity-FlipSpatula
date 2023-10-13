// Script from Daniel Brauer, Adrian
// http://wiki.unity3d.com/index.php?title=DontGoThroughThings

using UnityEngine;

public class DontGoThroughThings : MonoBehaviour
{
    public LayerMask LayerToCheck = -1; //make sure we aren't in this layer
    public float SkinWidth = 0.1f; //probably doesn't need to be changed

    private float _minimumExtent;
    private float _partialExtent;
    private float _sqrMinimumExtent;
    private Vector3 _previousPosition;
    private Rigidbody _rigidbody;
    private Collider _collider;

    //initialize values
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _previousPosition = _rigidbody.position;
        _minimumExtent = Mathf.Min(Mathf.Min(_collider.bounds.extents.x, _collider.bounds.extents.y), _collider.bounds.extents.z);
        _partialExtent = _minimumExtent * (1.0f - SkinWidth);
        _sqrMinimumExtent = _minimumExtent * _minimumExtent;
    }

    void FixedUpdate()
    {
        //have we moved more than our minimum extent?
        Vector3 movementThisStep = _rigidbody.position - _previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > _sqrMinimumExtent)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit hitInfo;

            //check for obstructions we might have missed
            if (Physics.Raycast(_previousPosition, movementThisStep, out hitInfo, movementMagnitude, LayerToCheck.value))
            {
                if (!hitInfo.collider)
                    return;

                if (!hitInfo.collider.isTrigger)
                    _rigidbody.position = hitInfo.point - (movementThisStep / movementMagnitude) * _partialExtent;
            }
        }

        _previousPosition = _rigidbody.position;
    }
}