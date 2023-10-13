using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public enum FlipAxis
    {
        X,
        Y,
        Z
    }

    [System.Serializable]
    public enum FlipInterrupt
    {
        Greater,
        Lower
    }

    public class FlipZone : MonoBehaviour
    {
        [SerializeField] protected FlipAxis _flipAxis = FlipAxis.Y;
        [SerializeField] protected FlipInterrupt _flipFlipInterrupt = FlipInterrupt.Greater;

        public virtual bool CanFlip(Vector3 point)
        {
            float testingPoint = point.x;
            float zonePoint = transform.position.x;
            if (_flipAxis == FlipAxis.Y) 
            {
                testingPoint = point.y;
                zonePoint = transform.position.y;
            }
            else if (_flipAxis == FlipAxis.Z) 
            {
                testingPoint = point.z;
                zonePoint = transform.position.z;
            }

            bool interrupted = (_flipFlipInterrupt == FlipInterrupt.Greater && testingPoint > zonePoint)
                                || (_flipFlipInterrupt == FlipInterrupt.Lower && testingPoint < zonePoint);
            return !interrupted;
        }
    }
}