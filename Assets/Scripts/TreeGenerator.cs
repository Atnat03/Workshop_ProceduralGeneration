using System;
using System.Collections.Generic;
using DefaultNamespace;
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
        [SerializeField] private Vector2 lenght = new Vector2(1, 2);
        [SerializeField] private float angleMin;
        [SerializeField] private float angleMax;
        [SerializeField] private float angleYMin;
        [SerializeField] private float angleYMax;
        [SerializeField] private Material treeMaterial;
        
        Stack<TreeData> treeStack = new Stack<TreeData>();
        Stack<int> splineIndexStack = new Stack<int>();
        private List<List<Vector3>> lineList = new List<List<Vector3>>();
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

        public TreeGeneratorData Data;
        
        void Start()
        {
            /*CreateTree(ExpandTreeString(axiom));
            EnfanteLeafs();*/
        }

        public void GenerateTree(TreeGeneratorData data, Transform point)
        {
            Data = data;

            if (Data == null)
            {
                ABPrint.Print("Data is null", ABColor.Red);
                return;
            }
            
            CreateTree(ExpandTreeString(Data.Axiom), point);
            EnfanteLeafs();
        }
        
        string ExpandTreeString(string tree)
        {
            string expandedTree;

            for (int i = 0; i < Data.Iterations; i++)
            {
                expandedTree = "";

                bool isEnd = i == Data.Iterations - 1;

                foreach (char c in tree)
                {
                    if (isEnd && c == 'B')
                    {
                        expandedTree += "L";
                        continue;
                    }

                    bool ruleFound = false;

                    foreach (var data in Data.BranchRules)
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
        
        void CreateTree(string tree, Transform t)
        {
            currentRadius = Data.BaseRadius;
            
            SplineDataStruct splineData = CreateSpline();
            int splineIndex = splineData.Container.Splines.FindIndex(splineData.Spline);

            splineData.Spline.Add(new BezierKnot(t.position), TangentMode.AutoSmooth);
            radiusList.Clear();
            radiusList.Add(currentRadius);
            
            foreach (char c in tree)
            {
                switch (c)
                {
                    case 'F' :
                        t.Translate(Vector3.up * Random.Range(Data.Lenght.x, Data.Lenght.y));
                        splineData.Spline.Add(new BezierKnot(t.position), TangentMode.AutoSmooth);
                        
                        break;
                    case 'B':
                        break;
                    case '[':
                        treeStack.Push(new TreeData
                        {
                            Position = t.position,
                            Rotation = t.rotation,
                            Radius = currentRadius
                        });
                        splineIndexStack.Push(splineIndex);
                        
                        int splineCount = splineData.Spline.Count;
                        int prevSplineIndex = splineIndex;
                        
                        splineData.Spline = splineData.Container.AddSpline();
                        splineIndex = splineData.Container.Splines.FindIndex(splineData.Spline);
                        
                        currentRadius *= Data.RadiusRatio;
                        radiusList.Add(currentRadius);
                        
                        splineData.Spline.Add(new BezierKnot(t.position), TangentMode.AutoSmooth);
                        splineData.Container.LinkKnots(new SplineKnotIndex(prevSplineIndex, splineCount -1), new SplineKnotIndex(splineIndex, 0));
                        
                        
                        break;
                    case ']':
                        TreeData data = treeStack.Pop();
                        
                        t.position = data.Position;
                        t.rotation = data.Rotation;
                        
                        splineIndex = splineIndexStack.Pop();
                        splineData.Spline = splineData.Container.Splines[splineIndex];
                        
                        currentRadius = data.Radius;
                        
                        break;
                    case 'l':
                        t.Rotate(Vector3.back, Random.Range(Data.AngleMin, Data.AngleMax));
                        t.Rotate(Vector3.up, Random.Range(Data.AngleYMin, Data.AngleYMax));
                        break;
                    case 'r':
                        t.Rotate(Vector3.forward, Random.Range(Data.AngleMin, Data.AngleMax));
                        t.Rotate(Vector3.up, Random.Range(Data.AngleYMin, Data.AngleYMax));
                        break;
                    case 'L':
                        CreateLeave(t);
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
            meshRenderer.material = Data.TreeMaterial;
            
            SplineContainer container = _currentTree.AddComponent<SplineContainer>();
            container.RemoveSplineAt(0);
            
            CustomExtrudeSpline extrude = _currentTree.AddComponent<CustomExtrudeSpline>();
            extrude.SplineContainer = container;
            extrude.Material = Data.TreeMaterial;

            SplineDataStruct data = new SplineDataStruct
            {
                Spline = container.AddSpline(),
                Container = container,
                Extrude = extrude
            };
            
            return data;
        }
        

        private void CreateLeave(Transform t)
        {
            for (int i = 0; i < Data.LeavesPerBranch; i++)
            {
                Vector3 direction = Random.onUnitSphere;

                Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);

                Vector3 position = t.position + Random.insideUnitSphere * currentRadius;

                /*GameObject leaf = Instantiate(Data.LeafPrefab, position, rotation);

                leaf.transform.localScale = Vector3.one * Data.LeafScale;
                
                _leafList.Add(leaf.transform);*/
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