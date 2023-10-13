using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using System.Diagnostics;

namespace CSGManager
{
    public class CSGHelper : MonoBehaviour
    {
        public static void Substract(GameObject removingObject, GameObject removerObject)
        {
            var removingObjectMeshFilter = removingObject.GetComponent<MeshFilter>();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            var result = CSG.Subtract(removingObject, removerObject);
            sw.Stop();



            removingObjectMeshFilter.sharedMesh = result.mesh;

            // Normalize vertices after substract.
            Mesh m = removingObjectMeshFilter.sharedMesh;

            if (m == null) return;

            int[] tris = m.triangles;
            int triangleCount = tris.Length;

            Vector3[] mesh_vertices = m.vertices;
            Vector3[] mesh_normals = m.normals;
            Vector2[] mesh_uv = m.uv;

            Vector3[] vertices = new Vector3[triangleCount];
            Vector3[] normals = new Vector3[triangleCount];
            Vector2[] uv = new Vector2[triangleCount];
            Color[] colors = new Color[triangleCount];

            for (int i = 0; i < triangleCount; i++)
            {
                Vector3 vertex = mesh_vertices[tris[i]];
                vertex = removingObject.transform.worldToLocalMatrix.MultiplyPoint3x4(vertex);

                vertices[i] = vertex;
                normals[i] = mesh_normals[tris[i]];
                uv[i] = mesh_uv[tris[i]];

                colors[i] = i % 3 == 0 ? new Color(1, 0, 0, 0) : (i % 3) == 1 ? new Color(0, 1, 0, 0) : new Color(0, 0, 1, 0);

                tris[i] = i;
            }

            Mesh wireframeMesh = new Mesh();

            wireframeMesh.Clear();
            wireframeMesh.vertices = vertices;
            wireframeMesh.triangles = tris;
            wireframeMesh.normals = normals;
            // wireframeMesh.colors = colors;
            wireframeMesh.uv = uv;

            removingObject.GetComponent<MeshFilter>().sharedMesh = wireframeMesh;
        }

    }
}