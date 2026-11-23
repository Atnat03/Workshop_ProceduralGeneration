using System;
using System.Collections.Generic;
using MyPrint;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class TreeGenerator : MonoBehaviour
    {
        [SerializeField] private string axiom;
        [SerializeField] private int iterations;
        [SerializeField] private int forestCount = 1;
        [SerializeField] private int stepBetween2Trees = 5;
        Stack<TreeData> treeStack = new Stack<TreeData>();
        Stack<int> splineIndexStack = new Stack<int>();
        TreeData treeData = new TreeData();
        [SerializeField] private float lenght;
        [SerializeField] private float angleMin;
        [SerializeField] private float angleMax;
        [SerializeField] private float angleYMin;
        [SerializeField] private float angleYMax;
        private List<List<Vector3>> lineList = new List<List<Vector3>>();
        [SerializeField] private Material treeMaterial;
        
        void Start()
        {
            for (int i = 0; i < forestCount; i++)
            {
                CreateTree(transform.position + (Vector3.right * stepBetween2Trees) * i, ExpandTreeString(axiom));
            }
        }
        
        string ExpandTreeString(string tree)
        {
            string expandedTree;

            for (int i = 0; i < iterations; i++)
            {
                expandedTree = "";
                foreach (char c in tree)
                {
                    switch (c)
                    {
                        case 'F':
                            if (Random.Range(0, 100) < 50)
                            {
                                expandedTree += "F";
                            }
                            else
                            {
                                expandedTree += "FF";
                            }

                            break;
                        case 'B':
                            if (Random.Range(0, 100) < 50)
                            {
                                expandedTree += "[llFB][rFB]";
                            }
                            else
                            {
                                expandedTree += "[lFB][rrFB]";
                            }
                            
                            break;
                        default:
                            expandedTree += c;
                            break;
                    }                    
                }
                tree = expandedTree;
            }

            return tree;
        }

        void CreateTree(Vector3 basePos, string tree)
        {
            GameObject treeObject = new GameObject("Tree");
            
            MeshFilter meshFilter = treeObject.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            
            MeshRenderer meshRenderer = treeObject.AddComponent<MeshRenderer>();
            meshRenderer.material = treeMaterial;
            
            SplineContainer container = treeObject.AddComponent<SplineContainer>();
            container.RemoveSplineAt(0);
            
            SplineExtrude extrude = treeObject.AddComponent<SplineExtrude>();
            extrude.Container = container;
            
            var currentSpline = container.AddSpline();
            var splineIndex = container.Splines.FindIndex(currentSpline);

            currentSpline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);
            
            foreach (char c in tree)
            {
                switch (c)
                {
                    case 'F' :
                        transform.Translate(Vector3.up * lenght);
                        currentSpline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);

                        break;
                    case 'B':
                        //do nothing
                        break;
                    case '[':
                        treeStack.Push(new TreeData
                        {
                            Position = transform.position,
                            Rotation = transform.rotation,
                        });
                        splineIndexStack.Push(splineIndex);
                        int splineCount = currentSpline.Count;
                        int prevSplineIndex = splineIndex;
                        currentSpline = container.AddSpline();
                        splineIndex = container.Splines.FindIndex(currentSpline);
                        currentSpline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);
                        container.LinkKnots(new SplineKnotIndex(prevSplineIndex, splineCount -1), new SplineKnotIndex(splineIndex, 0));
                        
                        break;
                    case ']':
                        TreeData data = treeStack.Pop();
                        transform.position = data.Position;
                        transform.rotation = data.Rotation;
                        splineIndex = splineIndexStack.Pop();
                        currentSpline = container.Splines[splineIndex];
                        break;
                    case 'l':
                        transform.Rotate(Vector3.back, Random.Range(angleMin, angleMax));
                        transform.Rotate(Vector3.up, Random.Range(angleYMin, angleYMax));
                        break;
                    case 'r':
                        transform.Rotate(Vector3.forward, Random.Range(angleMin, angleMax));
                        transform.Rotate(Vector3.up, Random.Range(angleYMin, angleYMax));
                        break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (List<Vector3> line in lineList)
            {
                Gizmos.color = Color.brown;
                Gizmos.DrawLine(line[0], line[1]);
            }
        }
    }
    
    
    public static class TreeGeneratorExtension
    {
        public static int FindIndex(this IReadOnlyList<Spline> splines, Spline spline)
        {
            for (int i = 0; i < splines.Count; i++)
            {
                if (splines[i] == spline)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}