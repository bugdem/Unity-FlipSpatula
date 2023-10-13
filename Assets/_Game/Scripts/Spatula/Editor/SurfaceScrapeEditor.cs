using UnityEngine;
using UnityEditor;
using Dreamteck.Splines;

namespace ClocknestGames.Game.Core
{
    [CustomEditor(typeof(SurfaceScrape))]
    public class SurfaceScrapeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var gameplay = FindObjectOfType<GameplayController>();

            var surfaceScrape = (SurfaceScrape)target;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Upper Face"))
            {
                ClearFaces(surfaceScrape);
                AddUpperFace(surfaceScrape);
            }
            if (GUILayout.Button("Generate Lower Face"))
            {
                ClearFaces(surfaceScrape);
                AddLowerFace(surfaceScrape);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Faces"))
            {
                ClearFaces(surfaceScrape);
                AddUpperFace(surfaceScrape);
                AddLowerFace(surfaceScrape);
            }

            if (GUILayout.Button("Clear Faces"))
            {
                ClearFaces(surfaceScrape);
            }

            if (GUILayout.Button("Swap Right Vector Signs"))
            {
                var children = surfaceScrape.GetComponentsInChildren<SurfaceScrapePart>();
                if (children.Length == 2)
                {
                    float sign = children[0].RightVectorSign;
                    children[0].RightVectorSign = -sign;
                    children[1].RightVectorSign = sign;
                }
            }
        }

        protected virtual void AddUpperFace(SurfaceScrape surfaceScrape)
        {
            var originalSplineMesh = surfaceScrape.GetComponent<SplineMesh>();
            var operationMesh = originalSplineMesh.GetChannel(0).NextMesh();

            Vector3 upperScale = new Vector3(operationMesh.scale.x, surfaceScrape.SurfaceScale, operationMesh.scale.z);
            Vector3 upperOffset = new Vector3(operationMesh.offset.x, operationMesh.offset.y + operationMesh.scale.y * .5f + surfaceScrape.SurfaceScale * .5f, operationMesh.offset.z);
            var upper = CreateMeshPart(surfaceScrape, "Upper", upperScale, upperOffset);
            var upperScrapePart = upper.gameObject.AddComponent<SurfaceScrapePart>();
            upperScrapePart.Owner = surfaceScrape;
            upperScrapePart.Mesh = upper;
            upperScrapePart.RightVectorSign = 1f;
        }

        protected virtual void AddLowerFace(SurfaceScrape surfaceScrape)
        {
            var originalSplineMesh = surfaceScrape.GetComponent<SplineMesh>();
            var operationMesh = originalSplineMesh.GetChannel(0).NextMesh();

            Vector3 lowerScale = new Vector3(operationMesh.scale.x, surfaceScrape.SurfaceScale, operationMesh.scale.z);
            Vector3 lowerOffset = new Vector3(operationMesh.offset.x, -(operationMesh.offset.y + operationMesh.scale.y * .5f + surfaceScrape.SurfaceScale * .5f), operationMesh.offset.z);
            var lower = CreateMeshPart(surfaceScrape, "Lower", lowerScale, lowerOffset);
            var lowerScrapePart = lower.gameObject.AddComponent<SurfaceScrapePart>();
            lowerScrapePart.Owner = surfaceScrape;
            lowerScrapePart.Mesh = lower;
            lowerScrapePart.RightVectorSign = -1f;
        }

        protected virtual void ClearFaces(SurfaceScrape surfaceScrape)
        {
            var children = surfaceScrape.GetComponentsInChildren<SurfaceScrapePart>();
            for (int index = children.Length - 1; index >= 0; index--)
                DestroyImmediate(children[index].gameObject);
        }

        protected virtual SplineMesh CreateMeshPart(SurfaceScrape surfaceScrape, string name, Vector3 scale, Vector3 offset)
        {
            var originalTransform = surfaceScrape.transform;
            var originalMeshFilter = surfaceScrape.GetComponent<MeshFilter>();
            var originalMeshRenderer = surfaceScrape.GetComponent<MeshRenderer>();
            var originalSplineMesh = surfaceScrape.GetComponent<SplineMesh>();

            var upperObj = new GameObject(name);

            // UnityEditorInternal.ComponentUtility.CopyComponent(originalTransform);
            // UnityEditorInternal.ComponentUtility.PasteComponentValues(upperObj.transform);
            upperObj.transform.position = surfaceScrape.transform.position;

            UnityEditorInternal.ComponentUtility.CopyComponent(originalMeshFilter);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(upperObj);

            UnityEditorInternal.ComponentUtility.CopyComponent(originalMeshRenderer);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(upperObj);

            var upperObjSplineMesh = upperObj.AddComponent<SplineMesh>();
            UnityEditorInternal.ComponentUtility.CopyComponent(originalSplineMesh);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(upperObjSplineMesh);

            upperObj.transform.SetParent(surfaceScrape.transform);

            for (int index = 0; index < upperObjSplineMesh.GetChannelCount(); index++)
            {
                var channel = upperObjSplineMesh.GetChannel(0).NextMesh();
                channel.scale = scale;
                channel.offset = offset;
            }

            upperObj.AddComponent<MeshCollider>();

            return upperObjSplineMesh;
        }
    }
}