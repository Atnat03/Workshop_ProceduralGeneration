
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VariableRadiusSpline : MonoBehaviour
{
    public SplineContainer splineContainer;

    [Header("One radius per segment")]
    public float[] segmentRadii;

    [Header("Tube settings")]
    [Range(3, 32)]
    public int radialSegments = 12;

    [Range(2, 100)]
    public int lengthSegments = 10;

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh()
    {
        if (splineContainer == null) return;

        Spline spline = splineContainer.Spline;
        int knotCount = spline.Count;

        int segmentCount = spline.Closed
            ? knotCount
            : knotCount - 1;

        if (segmentCount < 1) return;

        if (segmentRadii == null ||
            segmentRadii.Length != segmentCount)
        {
            Debug.LogError("One radius is required per segment.");
            return;
        }

        List<Vector3> vertices = new();
        List<int> triangles = new();

        int[] firstRings = new int[segmentCount];
        int[] lastRings = new int[segmentCount];

        Vector3 previousNormal = Vector3.up;

        for (int segment = 0; segment < segmentCount; segment++)
        {
            for (int step = 0; step <= lengthSegments; step++)
            {
                float t = (float)step / lengthSegments;
                float splineT = (segment + t) / segmentCount;

                spline.Evaluate(splineT, out float3 position, out float3 tangent, out float3 up);

                Vector3 center = splineContainer.transform.TransformPoint(position);

                Vector3 direction = splineContainer.transform.TransformDirection(tangent).normalized;

                Vector3 upVector = splineContainer.transform.TransformDirection(up);

                Vector3 normal =
                    Vector3.ProjectOnPlane(upVector, direction).normalized;

                if (normal.sqrMagnitude < 0.001f)
                    normal = Vector3.ProjectOnPlane(previousNormal, direction).normalized;

                if (normal.sqrMagnitude < 0.001f)
                    normal = Vector3.Cross(direction, Vector3.right).normalized;

                Vector3 binormal = Vector3.Cross(direction, normal).normalized;

                previousNormal = normal;

                float radius = segmentRadii[segment];

                int ringStart = vertices.Count;

                if (step == 0)
                    firstRings[segment] = ringStart;

                if (step == lengthSegments)
                    lastRings[segment] = ringStart;

                for (int i = 0; i < radialSegments; i++)
                {
                    float angle = i * Mathf.PI * 2f / radialSegments;

                    Vector3 offset =
                        normal * Mathf.Cos(angle) +
                        binormal * Mathf.Sin(angle);

                    vertices.Add(center + offset * radius);
                }

                if (step > 0)
                {
                    AddConnection(
                        triangles,
                        ringStart - radialSegments,
                        ringStart
                    );
                }
            }
        }

        for (int i = 0; i < segmentCount; i++)
        {
            int next = (i + 1) % segmentCount;

            if (!spline.Closed && next == 0)
                continue;

            if (Mathf.Abs(segmentRadii[i] - segmentRadii[next]) > 0.0001f)
            {
                AddConnection(
                    triangles,
                    lastRings[i],
                    firstRings[next]
                );
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Variable Radius Spline";

        if (vertices.Count > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void AddConnection(
        List<int> triangles,
        int previousStart,
        int currentStart)
    {
        for (int i = 0; i < radialSegments; i++)
        {
            int next = (i + 1) % radialSegments;

            triangles.Add(previousStart + i);
            triangles.Add(currentStart + i);
            triangles.Add(previousStart + next);

            triangles.Add(previousStart + next);
            triangles.Add(currentStart + i);
            triangles.Add(currentStart + next);
        }
    }
}
