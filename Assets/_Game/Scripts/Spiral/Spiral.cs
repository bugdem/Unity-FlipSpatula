using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class Spiral : MonoBehaviour
{

    [Range(.01f, 100)] [SerializeField] public float radius = 5;
    [Range(.01f, 100)] [SerializeField] public float radiusIncrease = .005f;
    [Range(.1f, 10f)] [SerializeField] public float width = 1.5f;
    [Range(.1f, 10f)] [SerializeField] public float height = .5f;
    [Range(.01f, 100)] [SerializeField] public float length = 9.06f;
    [Range(4, 720)] [SerializeField] public int sides = 45;
    [Range(0f, 100)] [SerializeField] public float offset = 2f;

    Mesh mesh;

    public Vector3 finalPosition;
    public Vector3[] radiusSurfaceVertices { get; protected set; }
    public Vector3[] vertices { get; protected set; }
    public Vector2[] uvs { get; protected set; }
    int[] triangles;
    int[] trianglesSurface;

    protected CapsuleCollider _collider;
    protected MeshRenderer _meshRenderer;
    protected MeshFilter _meshFilter;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        // MeshRenderer
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        }

        // MeshFilter
        mesh = new Mesh();
        _meshFilter.mesh = mesh;

        Refresh();
    }

    public void Refresh()
    {
        CreateScaffold();
        UpdateMesh();
        UpdatePlotter();
        UpdateCollider();
    }

    public SpiralPlacementModel GetPlacementAlongLength(float percent)
    {
        var vertOffset = Mathf.RoundToInt(percent * vertices.Length);
        vertOffset = Mathf.RoundToInt(Mathf.Clamp(vertOffset, 0, vertices.Length - 5));
        var p1 = vertices[vertOffset + 0];
        var p2 = vertices[vertOffset + 1];
        var p3 = vertices[vertOffset + 4];
        var angle = Vector3.Cross(p2 - p1, p2 - p3).normalized;

        return new SpiralPlacementModel(radiusSurfaceVertices[vertOffset / 4], angle);
    }

    void CreateScaffold()
    {
        // Helpers
        float halfWidth = width / 2;
        var sidesIncludingCaps = sides + 2;
        int dist = Mathf.CeilToInt(((int)sidesIncludingCaps) * length);
        Vector3 vertexSurfaceRadius;
        Vector3 vertexInnerTop;
        Vector3 vertexOuterTop;
        Vector3 vertexInnerBottom;
        Vector3 vertexOuterBottom;

        // Vertices
        radiusSurfaceVertices = new Vector3[dist];
        int verticesLength = dist * 4;
        vertices = new Vector3[verticesLength * 3];
        uvs = new Vector2[verticesLength * 3];
        float step = offset / sides;
        float y = 0;
        float radiusp = 0f;
        for (var i = 0; i < dist; i++)
        {
            float currentRad = radius + radiusp;
            // Initial centered vert
            var angle = i * Mathf.PI * 2 / sides;
            var pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            var upOffset = Vector3.up * y;
            var downOffset = Vector3.up * (y - height);

            // Radius surface helpers
            vertexSurfaceRadius = pos * currentRad;
            vertexSurfaceRadius += upOffset;

            // Inner top vertex
            vertexInnerTop = pos * (currentRad - halfWidth);
            vertexInnerTop += upOffset;

            // Outer top vertex
            vertexOuterTop = pos * (currentRad + halfWidth);
            vertexOuterTop += upOffset;

            // Inner bottom vertex
            vertexInnerBottom = pos * (currentRad - halfWidth);
            vertexInnerBottom += downOffset;

            // Outer bottom vertex
            vertexOuterBottom = pos * (currentRad + halfWidth);
            vertexOuterBottom += downOffset;

            // Update vertices
            radiusSurfaceVertices[i] = vertexSurfaceRadius;
            vertices[i * 4 + 0] = vertexInnerTop;
            vertices[i * 4 + 1] = vertexOuterTop;
            vertices[i * 4 + 2] = vertexInnerBottom;
            vertices[i * 4 + 3] = vertexOuterBottom;

            vertices[verticesLength + i * 4 + 0] = vertexInnerTop;
            vertices[verticesLength + i * 4 + 1] = vertexOuterTop;
            vertices[verticesLength + i * 4 + 2] = vertexInnerBottom;
            vertices[verticesLength + i * 4 + 3] = vertexOuterBottom;

            vertices[verticesLength * 2 + i * 4 + 0] = vertexInnerTop;
            vertices[verticesLength * 2 + i * 4 + 1] = vertexOuterTop;
            vertices[verticesLength * 2 + i * 4 + 2] = vertexInnerBottom;
            vertices[verticesLength * 2 + i * 4 + 3] = vertexOuterBottom;

            y += step;

            radiusp += radiusIncrease;
        }

        float distanceConst = 20f;
        float totalDistance = 0f;

        // Triangles
        var vertexIterationCount = 24;
        var trianglePathVertexCount = dist * vertexIterationCount;
        var triangleCapVertexCount = 12;
        triangles = new int[trianglePathVertexCount + triangleCapVertexCount];
        trianglesSurface = new int[trianglePathVertexCount / 4];
        var vertAnchor = 0;
        var vertAnchorSurface = 0;
        var meshAnchor = 0;
        var innerTop = 0;
        var outerTop = 1;
        var innerBottom = 2;
        var outerBottom = 3;
        var xdiff = .125f;
        for (var i = 0; i < dist; i++)
        {
            float distance = 0f;
            if (i < dist - 1)
                distance = Vector3.Distance(radiusSurfaceVertices[i + 1], radiusSurfaceVertices[i]);

            // Start cap
            if (i == 0)
            {
                triangles[vertAnchor + 0] = innerBottom;
                triangles[vertAnchor + 1] = innerTop;
                triangles[vertAnchor + 2] = outerTop;
                triangles[vertAnchor + 3] = innerBottom;
                triangles[vertAnchor + 4] = outerTop;
                triangles[vertAnchor + 5] = outerBottom;
                vertAnchor += 6;

                // Start cap UVs
                uvs[innerTop] = new Vector2(0f, 1f - xdiff);
                uvs[outerTop] = new Vector2(0f, 1f);
                uvs[innerBottom] = new Vector2(xdiff, 1f - xdiff);
                uvs[outerBottom] = new Vector2(xdiff, 1f);
            }

            // Path
            else if (i < dist - 1)
            {
                // Top quad
                triangles[vertAnchor + 0] = verticesLength * 2 + meshAnchor + innerTop;
                triangles[vertAnchor + 1] = verticesLength * 2 + meshAnchor + innerTop + 4;
                triangles[vertAnchor + 2] = verticesLength * 2 + meshAnchor + outerTop + 4;
                triangles[vertAnchor + 3] = verticesLength * 2 + meshAnchor + innerTop;
                triangles[vertAnchor + 4] = verticesLength * 2 + meshAnchor + outerTop + 4;
                triangles[vertAnchor + 5] = verticesLength * 2 + meshAnchor + outerTop;

                // Right quad
                //triangles[vertAnchor + 6] = verticesLength + meshAnchor + outerTop;
                //triangles[vertAnchor + 7] = verticesLength + meshAnchor + outerTop + 4;
                //triangles[vertAnchor + 8] = verticesLength + meshAnchor + outerBottom + 4;
                //triangles[vertAnchor + 9] = verticesLength + meshAnchor + outerTop;
                //triangles[vertAnchor + 10] = verticesLength + meshAnchor + outerBottom + 4;
                //triangles[vertAnchor + 11] = verticesLength + meshAnchor + outerBottom;

                trianglesSurface[vertAnchorSurface + 0] = verticesLength + meshAnchor + outerTop;
                trianglesSurface[vertAnchorSurface + 1] = verticesLength + meshAnchor + outerTop + 4;
                trianglesSurface[vertAnchorSurface + 2] = verticesLength + meshAnchor + outerBottom + 4;
                trianglesSurface[vertAnchorSurface + 3] = verticesLength + meshAnchor + outerTop;
                trianglesSurface[vertAnchorSurface + 4] = verticesLength + meshAnchor + outerBottom + 4;
                trianglesSurface[vertAnchorSurface + 5] = verticesLength + meshAnchor + outerBottom;

                // Bottom quad
                triangles[vertAnchor + 12] = verticesLength * 2 + meshAnchor + outerBottom;
                triangles[vertAnchor + 13] = verticesLength * 2 + meshAnchor + outerBottom + 4;
                triangles[vertAnchor + 14] = verticesLength * 2 + meshAnchor + innerBottom + 4;
                triangles[vertAnchor + 15] = verticesLength * 2 + meshAnchor + outerBottom;
                triangles[vertAnchor + 16] = verticesLength * 2 + meshAnchor + innerBottom + 4;
                triangles[vertAnchor + 17] = verticesLength * 2 + meshAnchor + innerBottom;

                // Left quad
                triangles[vertAnchor + 18] = verticesLength + meshAnchor + innerBottom;
                triangles[vertAnchor + 19] = verticesLength + meshAnchor + innerBottom + 4;
                triangles[vertAnchor + 20] = verticesLength + meshAnchor + innerTop + 4;
                triangles[vertAnchor + 21] = verticesLength + meshAnchor + innerBottom;
                triangles[vertAnchor + 22] = verticesLength + meshAnchor + innerTop + 4;
                triangles[vertAnchor + 23] = verticesLength + meshAnchor + innerTop;

                //var innerTop = 0;
                //var outerTop = 1;
                //var innerBottom = 2;
                //var outerBottom = 3;

                /*
				// Top UVs (Right surface)
				uvs[verticesLength * 2 + meshAnchor + innerTop + 4] = new Vector2(0.5f, 1f);
				uvs[verticesLength * 2 + meshAnchor + outerTop + 4] = new Vector2(.5f, .75f);
				uvs[verticesLength * 2 + meshAnchor + innerTop] = new Vector2(0.25f, 1f);
				uvs[verticesLength * 2 + meshAnchor + outerTop] = new Vector2(.25f, .75f);

				// Right UVs (Outer long surface)
				uvs[verticesLength + meshAnchor + outerTop] = new Vector2(0.25f, 0.75f);
				uvs[verticesLength + meshAnchor + outerTop + 4] = new Vector2(0.5f, 0.75f);
				uvs[verticesLength + meshAnchor + outerBottom] = new Vector2(0.25f, .5f);
				uvs[verticesLength + meshAnchor + outerBottom + 4] = new Vector2(0.5f, 0.5f);

				// Bottom UVs (Left surface)
				uvs[verticesLength * 2 + meshAnchor + innerBottom] = new Vector2(.25f, .25f);
				uvs[verticesLength * 2 + meshAnchor + outerBottom] = new Vector2(0.25f, .5f);
				uvs[verticesLength * 2 + meshAnchor + innerBottom + 4] = new Vector2(.5f, .25f);
				uvs[verticesLength * 2 + meshAnchor + outerBottom + 4] = new Vector2(0.5f, 0.5f);

				// Left UVs (Inner long surface)
				uvs[verticesLength + meshAnchor + innerTop + 4] = new Vector2(0.75f, 0.75f);
				uvs[verticesLength + meshAnchor + innerTop] = new Vector2(1f, 0.75f);
				uvs[verticesLength + meshAnchor + innerBottom + 4] = new Vector2(0.75f, .5f);
				uvs[verticesLength + meshAnchor + innerBottom] = new Vector2(1f, 0.5f);
				*/

                // Top UVs (Right surface)
                uvs[verticesLength * 2 + meshAnchor + innerTop + 4] = new Vector2(0f, 1f - 5 * xdiff);
                uvs[verticesLength * 2 + meshAnchor + outerTop + 4] = new Vector2(0f, 1f - 4 * xdiff);
                uvs[verticesLength * 2 + meshAnchor + innerTop] = new Vector2(xdiff, 1f - 5 * xdiff);
                uvs[verticesLength * 2 + meshAnchor + outerTop] = new Vector2(xdiff, 1f - 4 * xdiff);


                // Right UVs (Outer long surface)
                //uvs[verticesLength + meshAnchor + outerTop] = new Vector2(0f, 1f - 3 * xdiff);
                //uvs[verticesLength + meshAnchor + outerTop + 4] = new Vector2(0f, 1f - 2 * xdiff);
                //uvs[verticesLength + meshAnchor + outerBottom] = new Vector2(xdiff, 1f - 3 * xdiff);
                //uvs[verticesLength + meshAnchor + outerBottom + 4] = new Vector2(xdiff, 1f - 2 * xdiff);

                /*
                if (meshAnchor % 8 == 0)
                {
                    // Right UVs (Outer long surface)
                    uvs[verticesLength + meshAnchor + outerTop] = new Vector2(0f, 1f - 3 * xdiff);
                    uvs[verticesLength + meshAnchor + outerTop + 4] = new Vector2(0f, 1f - 2 * xdiff);
                    uvs[verticesLength + meshAnchor + outerBottom] = new Vector2(xdiff, 1f - 3 * xdiff);
                    uvs[verticesLength + meshAnchor + outerBottom + 4] = new Vector2(xdiff, 1f - 2 * xdiff);
                }
                */



                if (meshAnchor % 8 == 0)
                {
                    //uvs[verticesLength + meshAnchor + outerTop] = new Vector2(0f, meshAnchor / (float)dist);
                    //uvs[verticesLength + meshAnchor + outerTop + 4] = new Vector2(0f, (meshAnchor + 4f) / (float)dist);
                    //uvs[verticesLength + meshAnchor + outerBottom] = new Vector2(1f, meshAnchor / (float)dist);
                    //uvs[verticesLength + meshAnchor + outerBottom + 4] = new Vector2(1f, (meshAnchor + 4f) / (float)dist);

                    float fromY = Mathf.FloorToInt(totalDistance / distanceConst) + (totalDistance % distanceConst) / distanceConst;
                    float toY = Mathf.FloorToInt((totalDistance + distance) / distanceConst) + ((totalDistance + distance) % distanceConst) / distanceConst;

                    uvs[verticesLength + meshAnchor + outerTop] = new Vector2(0f, fromY);
                    uvs[verticesLength + meshAnchor + outerTop + 4] = new Vector2(0f, toY);
                    uvs[verticesLength + meshAnchor + outerBottom] = new Vector2(1f, fromY);
                    uvs[verticesLength + meshAnchor + outerBottom + 4] = new Vector2(1f, toY);
                }

                // Bottom UVs (Left surface)
                uvs[verticesLength * 2 + meshAnchor + innerBottom] = new Vector2(2 * xdiff, 1f - 5 * xdiff);
                uvs[verticesLength * 2 + meshAnchor + outerBottom] = new Vector2(2 * xdiff, 1f - 4 * xdiff);
                uvs[verticesLength * 2 + meshAnchor + innerBottom + 4] = new Vector2(3 * xdiff, 1f - 5 * xdiff);
                uvs[verticesLength * 2 + meshAnchor + outerBottom + 4] = new Vector2(3 * xdiff, 1f - 4 * xdiff);

                // Left UVs (Inner long surface)
                uvs[verticesLength + meshAnchor + innerTop + 4] = new Vector2(2 * xdiff, 1f - 3 * xdiff);
                uvs[verticesLength + meshAnchor + innerTop] = new Vector2(2 * xdiff, 1f - 2 * xdiff);
                uvs[verticesLength + meshAnchor + innerBottom + 4] = new Vector2(3 * xdiff, 1f - 3 * xdiff);
                uvs[verticesLength + meshAnchor + innerBottom] = new Vector2(3 * xdiff, 1f - 2 * xdiff);

                //uvs[meshAnchor + 0] = new Vector2(0, 0);
                //uvs[meshAnchor + 1] = new Vector2(0, (meshAnchor / 4));
                //uvs[meshAnchor + 2] = new Vector2(0, 0);
                //uvs[meshAnchor + 3] = new Vector2(1, (meshAnchor / 4));

                // Offset helpers
                vertAnchor += vertexIterationCount;
                vertAnchorSurface += vertexIterationCount / 4;
                meshAnchor += 4;
            }

            // End cap
            else
            {
                vertAnchor += vertexIterationCount;
                triangles[vertAnchor + 0] = meshAnchor + outerTop;
                triangles[vertAnchor + 1] = meshAnchor + innerTop;
                triangles[vertAnchor + 2] = meshAnchor + innerBottom;
                triangles[vertAnchor + 3] = meshAnchor + outerTop;
                triangles[vertAnchor + 4] = meshAnchor + innerBottom;
                triangles[vertAnchor + 5] = meshAnchor + outerBottom;

                // End cap UVs
                uvs[meshAnchor + innerBottom] = new Vector2(2 * xdiff, 1f - xdiff);
                uvs[meshAnchor + outerBottom] = new Vector2(2 * xdiff, 1f);
                uvs[meshAnchor + innerTop] = new Vector2(3 * xdiff, 1f - xdiff);
                uvs[meshAnchor + outerTop] = new Vector2(3 * xdiff, 1f);
            }

            totalDistance += distance;
        }

        finalPosition = radiusSurfaceVertices[radiusSurfaceVertices.Length - 1];
    }

    void UpdateMesh()
    {
        if (mesh != null)
        {
            mesh.Clear();
            mesh.subMeshCount = 2;

            mesh.vertices = vertices;
            // mesh.triangles = triangles;
            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(trianglesSurface, 1);
            mesh.uv = uvs;

            // mesh.Optimize();
            mesh.RecalculateNormals();
        }
    }

    void UpdatePlotter()
    {
        var spiralPlotter = GetComponent<SpiralPlotter>();
        if (spiralPlotter != null)
        {
            spiralPlotter.UpdatePlotter();
        }
    }


    void UpdateCollider()
    {
        if (_collider == null)
            _collider = GetComponent<CapsuleCollider>();

        if (mesh != null)
        {
            Vector3 colliderEdge = (vertices[vertices.Length - 1] + vertices[vertices.Length - 3]) * .5f;
            _collider.center = mesh.bounds.center;
            _collider.radius = Vector3.Distance(_collider.center, colliderEdge);
        }
    }

    void OnValidate()
    {
        Refresh();
    }
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        UnityEditor.Handles.color = Color.red;
        if (vertices.Length > 0)
        {
            for (int index = vertices.Length - 8; index < vertices.Length; index++)
            {
                UnityEditor.Handles.Label(transform.localToWorldMatrix.MultiplyPoint3x4(vertices[index]), index.ToString());
            }

            for (int index = 0; index < 16; index++)
            {
                UnityEditor.Handles.Label(transform.localToWorldMatrix.MultiplyPoint3x4(vertices[index]), index.ToString());
            }
        }

        //if (radiusSurfaceVertices.Length > 0)
        //{
        //	for (int index = 0; index < 50; index++)
        //	{
        //		UnityEditor.Handles.Label(radiusSurfaceVertices[index], index.ToString());
        //	}
        //}

        //var bounds = new Bounds(mesh.bounds.center, Vector3.zero);
        //bounds.Encapsulate(mesh.bounds);
        //Gizmos.DrawWireCube(bounds.center, bounds.size);
#endif
    }
}
