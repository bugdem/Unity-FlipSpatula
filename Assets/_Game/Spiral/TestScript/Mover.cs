using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Spiral SpiralGenerator;
    public Vector3 GroundPoint;

    public float SpiralSpeed = .2f;
    public float SmoothTime = .2f;

    protected Vector3 _currentVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        var collider = SpiralGenerator.GetComponent<CapsuleCollider>();

        SpiralGenerator.length += SpiralSpeed * Time.deltaTime;
        SpiralGenerator.transform.Rotate(Vector3.up, SpiralSpeed * 360f * Time.deltaTime);
        // SpiralGenerator.transform.position = GroundPoint + Vector3.up * collider.radius * collider.transform.lossyScale.y;

        // SpiralGenerator.transform.position = GroundPoint + Vector3.up * collider.bounds.size.z * .5f;

        Vector3 worldSpace = SpiralGenerator.transform.TransformPoint(SpiralGenerator.finalPosition);
        Vector3 position = SpiralGenerator.transform.position;
        position.y = GroundPoint.y + (SpiralGenerator.transform.position.y - worldSpace.y);
        SpiralGenerator.transform.position = Vector3.SmoothDamp(SpiralGenerator.transform.position, position, ref _currentVelocity, SmoothTime);

        SpiralGenerator.Refresh();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GroundPoint, .5f);

        Gizmos.DrawSphere(SpiralGenerator.transform.TransformPoint(SpiralGenerator.finalPosition), .5f);
    }
}
