using System;
using System.Collections.Generic;
using MyPrint;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace TreeGeneration
{
    public class TreeGenerator : MonoBehaviour
    {
        [SerializeField] private string axiom;
        [SerializeField] private int iterations;
        Stack<TreeData> treeStack = new Stack<TreeData>();
        Stack<int> splineIndexStack = new Stack<int>();
        TreeData treeData = new TreeData();
        [SerializeField] private Vector2 lenght = new Vector2(1, 2);
        [SerializeField] private float angleMin;
        [SerializeField] private float angleMax;
        [SerializeField] private float angleYMin;
        [SerializeField] private float angleYMax;
        private List<List<Vector3>> lineList = new List<List<Vector3>>();
        [SerializeField] private Material treeMaterial;
        private GameObject _currentTree;

        [Header("Radius")]
        [SerializeField] private float _baseRadius = 0.5f;
        [SerializeField, Range(0.1f, 1)] private float _radiusRatio = 0.95f;
        List<float> radiusList = new List<float>();
        float currentRadius;
        
        [Header("Rules")] 
        [SerializeField] private List<BranchRules> _branchRules = new List<BranchRules>(); 
        
        [Header("Leaves")]
        [SerializeField] private GameObject _leafPrefab;
        [SerializeField] private float leafScale = 0.3f;
        [SerializeField] private int leavesPerBranch = 3;
        List<Transform> _leafList = new List<Transform>();

        void Start()
        {
            CreateTree(ExpandTreeString(axiom));
            EnfanteLeafs();
        }

        string ExpandTreeString(string tree)
        {
            string expandedTree;

            for (int i = 0; i < iterations; i++)
            {
                expandedTree = "";

                bool isEnd = i == iterations - 1;

                foreach (char c in tree)
                {
                    if (isEnd && c == 'B')
                    {
                        expandedTree += "L";
                        continue;
                    }

                    bool ruleFound = false;

                    foreach (var data in _branchRules)
                    {
                        if (data.Rule == c)
                        {
                            expandedTree += GetRandomVariation(
                                data.BranchRulesProbabilityList
                            );

                            ruleFound = true;
                            break;
                        }
                    }

                    if (!ruleFound)
                        expandedTree += c;
                }

                tree = expandedTree;
            }

            return tree;
        }


        string GetRandomVariation(List<BranchRulesProbability> branchProba)
        {
            float totalProbability = 0f;

            foreach (var variation in branchProba)
                totalProbability += variation.Probability;

            if (totalProbability <= 0f)
                return "";

            float randomValue = Random.Range(0f, totalProbability);

            foreach (var variation in branchProba)
            {
                randomValue -= variation.Probability;

                if (randomValue < 0f)
                    return variation.Rule;
            }

            return "";
        }
        
        void CreateTree(string tree)
        {
            currentRadius = _baseRadius;
            
            SplineDataStruct splineData = CreateSpline();
            int splineIndex = splineData.Container.Splines.FindIndex(splineData.Spline);

            splineData.Spline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);
            radiusList.Clear();
            radiusList.Add(currentRadius);
            
            foreach (char c in tree)
            {
                switch (c)
                {
                    case 'F' :
                        transform.Translate(Vector3.up * Random.Range(lenght.x, lenght.y));
                        splineData.Spline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);
                        
                        break;
                    case 'B':
                        break;
                    case '[':
                        treeStack.Push(new TreeData
                        {
                            Position = transform.position,
                            Rotation = transform.rotation,
                            Radius = currentRadius
                        });
                        splineIndexStack.Push(splineIndex);
                        
                        int splineCount = splineData.Spline.Count;
                        int prevSplineIndex = splineIndex;
                        
                        splineData.Spline = splineData.Container.AddSpline();
                        splineIndex = splineData.Container.Splines.FindIndex(splineData.Spline);
                        
                        currentRadius *= _radiusRatio;
                        radiusList.Add(currentRadius);
                        
                        splineData.Spline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);
                        splineData.Container.LinkKnots(new SplineKnotIndex(prevSplineIndex, splineCount -1), new SplineKnotIndex(splineIndex, 0));
                        
                        
                        break;
                    case ']':
                        TreeData data = treeStack.Pop();
                        
                        transform.position = data.Position;
                        transform.rotation = data.Rotation;
                        
                        splineIndex = splineIndexStack.Pop();
                        splineData.Spline = splineData.Container.Splines[splineIndex];
                        
                        currentRadius = data.Radius;
                        
                        break;
                    case 'l':
                        transform.Rotate(Vector3.back, Random.Range(angleMin, angleMax));
                        transform.Rotate(Vector3.up, Random.Range(angleYMin, angleYMax));
                        break;
                    case 'r':
                        transform.Rotate(Vector3.forward, Random.Range(angleMin, angleMax));
                        transform.Rotate(Vector3.up, Random.Range(angleYMin, angleYMax));
                        break;
                    case 'L':
                        CreateLeave();
                        break;
                }
            }

            if (splineData.Extrude != null)
            {
                splineData.Extrude.GenerateMesh(radiusList.ToArray());
            }
        }
            
        private SplineDataStruct  CreateSpline()
        {
            _currentTree = new GameObject("Tree");
            
            MeshFilter meshFilter = _currentTree.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            
            MeshRenderer meshRenderer = _currentTree.AddComponent<MeshRenderer>();
            meshRenderer.material = treeMaterial;
            
            SplineContainer container = _currentTree.AddComponent<SplineContainer>();
            container.RemoveSplineAt(0);
            
            CustomExtrudeSpline extrude = _currentTree.AddComponent<CustomExtrudeSpline>();
            extrude.SplineContainer = container;
            extrude.Material = treeMaterial;

            SplineDataStruct data = new SplineDataStruct
            {
                Spline = container.AddSpline(),
                Container = container,
                Extrude = extrude
            };
            
            return data;
        }
        

        private void CreateLeave()
        {
            for (int i = 0; i < leavesPerBranch; i++)
            {
                Vector3 direction = Random.onUnitSphere;

                Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);

                Vector3 position = transform.position + Random.insideUnitSphere * currentRadius;

                GameObject leaf = Instantiate(_leafPrefab, position, rotation);

                leaf.transform.localScale = Vector3.one * leafScale;
                
                _leafList.Add(leaf.transform);
            }
        }

        private void EnfanteLeafs()
        {
            if (_currentTree == null) return;
            
            foreach (var t in _leafList)
            {
                t.SetParent(_currentTree.transform);
            }

            _currentTree = null;
        }
    }

}