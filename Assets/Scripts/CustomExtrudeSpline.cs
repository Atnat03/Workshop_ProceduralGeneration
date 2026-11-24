using System;
using System.Collections.Generic;
using System.ComponentModel;
using MyPrint;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CustomExtrudeSpline : MonoBehaviour
{
    public SplineContainer SplineContainer;
    
    [SerializeField] private int _radialSegment = 12;
    [SerializeField] private int _lengthSegments = 10;
    
    public Material Material;
    
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    
    
    public void GenerateMesh(float[] radiusList)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (!meshRenderer || !meshFilter)
        {
            Debug.LogError("MeshFilter ou MeshRenderer manquant !");
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int s = 0; s < SplineContainer.Splines.Count; s++)
        {
            Spline spline = SplineContainer.Splines[s];

            float radius = radiusList[s];

            int count = spline.Count;
            int segmentCount = spline.Closed ? count : count - 1;

            int previousStart = -1;
            int firstStart = -1;

            for (int i = 0; i < segmentCount; i++)
            {
                for (int j = 0; j <= _lengthSegments; j++)
                {
                    float t = (float)j / _lengthSegments;
                    float splineT = (i + t) / segmentCount;

                    spline.Evaluate(splineT, out float3 position, out float3 tangent, out float3 up);
                    
                    Vector3 vecDir = Vector3.Cross(tangent, up).normalized;
                    Vector3 secondVec = Vector3.Cross(vecDir, tangent).normalized;

                    int ringStart = vertices.Count;

                    if (firstStart == -1)
                        firstStart = ringStart;

                    for (int vert = 0; vert < _radialSegment; vert++)
                    {
                        float angle = vert * 2 * Mathf.PI / _radialSegment;

                        Vector3 P = (Vector3)position + radius * (Mathf.Cos(angle) * vecDir + Mathf.Sin(angle) * secondVec);

                        vertices.Add(P);
                    }

                    if (previousStart != -1)
                    {
                        for (int k = 0; k < _radialSegment; k++)
                        {
                            int next = (k + 1) % _radialSegment;

                            int a = previousStart + k;
                            int aNext = previousStart + next;

                            int b = ringStart + k;
                            int bNext = ringStart + next;

                            triangles.Add(a);
                            triangles.Add(b);
                            triangles.Add(aNext);

                            triangles.Add(aNext);
                            triangles.Add(b);
                            triangles.Add(bNext);
                        }
                    }

                    previousStart = ringStart;
                }
            }

            if (firstStart != -1 && previousStart != -1)
            {
                CreateCap(vertices, triangles, firstStart);
                CreateCap(vertices, triangles, previousStart);
            }
        }

        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshRenderer.material = Material;
    }

    private void CreateCap(List<Vector3> vertices, List<int> triangles, int ringStart)
    {
        Vector3 center = Vector3.zero;

        for (int i = 0; i < _radialSegment; i++)
            center += vertices[ringStart + i];

        center /= _radialSegment;

        int centerIndex = vertices.Count;
        vertices.Add(center);

        for (int i = 0; i < _radialSegment; i++)
        {
            int next = (i + 1) % _radialSegment;

            triangles.Add(centerIndex);
            triangles.Add(ringStart + i);
            triangles.Add(ringStart + next);

            triangles.Add(centerIndex);
            triangles.Add(ringStart + next);
            triangles.Add(ringStart + i);
        }
    }

}
