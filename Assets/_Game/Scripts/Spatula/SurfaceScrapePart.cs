using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClocknestGames.Library.Editor;
using Dreamteck.Splines;

namespace ClocknestGames.Game.Core
{
    public class SurfaceScrapePart : MonoBehaviour
    {
        [ReadOnly] public SplineMesh Mesh;
        [ReadOnly] public SurfaceScrape Owner;
        [ReadOnly] public float RightVectorSign = 1f;

        public static SurfaceScrapePart GetFromCollider(Collider collider)
        {
            return collider.GetComponent<SurfaceScrapePart>();
        }
    }
}