using System;
using System.Collections.Generic;
using DefaultNamespace;
using MyPrint;
using TreeGeneration;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

    public class TreeGenerator
    {
        public TreeGenerator(){}
        
        //Tree creation
        Stack<TreeData> treeStack = new Stack<TreeData>();
        Stack<int> splineIndexStack = new Stack<int>();
        private List<List<Vector3>> lineList = new List<List<Vector3>>();

        //Radius
        List<float> radiusList = new List<float>();
        float currentRadius;
        
        //Leafs
        List<Transform> _leafList = new List<Transform>();

        TreeGeneratorData Data;
        
        public GameObject GenerateTree(TreeGeneratorData data, Transform point, int numberTree)
        {
            Data = data;
            _leafList = new List<Transform>();
            
            if (Data == null)
            {
                ABPrint.Print("Data is null", ABColor.Red);
                return null;
            }
            
            GameObject newTree = CreateTree(ExpandTreeString(Data.Axiom), point, numberTree);
            
            newTree = EnfanteLeafs(newTree);
            
            _leafList.Clear();
            
            return newTree;
        }
        
        string ExpandTreeString(string tree)
        {
            for (int i = 0; i < Data.Iterations; i++)
            {
                var expandedTree = "";

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
        
        GameObject CreateTree(string tree, Transform t, int numberTree)
        {
            currentRadius = Data.BaseRadius;
            
            SplineDataStruct splineData = CreateSpline(numberTree, t.position);
            int splineIndex = splineData.Container.Splines.FindIndex(splineData.Spline);
            
            Vector3 localPosition = splineData.Object.transform.InverseTransformPoint(t.position);
            splineData.Spline.Add(new BezierKnot(localPosition), TangentMode.AutoSmooth);
            
            radiusList.Clear();
            radiusList.Add(currentRadius);
            
            foreach (char c in tree)
            {
                switch (c)
                {
                    case 'F' :
                        t.Translate(Vector3.up * Random.Range(Data.Lenght.x, Data.Lenght.y));

                        localPosition = splineData.Object.transform.InverseTransformPoint(t.position);

                        splineData.Spline.Add(new BezierKnot(localPosition), TangentMode.AutoSmooth);
                        
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
                        
                        splineData.Spline.Add(new BezierKnot(splineData.Object.transform.InverseTransformPoint(t.position)), TangentMode.AutoSmooth); 
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
                    case 'y':
                        t.Rotate(Vector3.up, Random.Range(Data.AngleYMin, Data.AngleYMax));
                        break;
                    case 'w':
                        t.Rotate(Random.Range(-Data.Wobble, Data.Wobble), 0f, Random.Range(-Data.Wobble, Data.Wobble));
                        break;
                    case 'g':
                    {
                        Vector3 axis = Vector3.Cross(t.up, Vector3.down);
                        float s = axis.magnitude;
                        if (s > 0.001f)
                            t.rotation = Quaternion.AngleAxis(Data.Tropism * s, axis / s) * t.rotation;
                        break;
                    }
                }
            }

            if (splineData.Extrude != null)
            {
                splineData.Extrude.GenerateMesh(radiusList.ToArray());
            }

            return splineData.Object;
        }
            
        private SplineDataStruct CreateSpline(int numberTree, Vector3 pos)
        {
            GameObject newSpline = new GameObject("Tree " + numberTree)
            {
                transform =
                {
                    position = pos
                }
            };

            MeshFilter meshFilter = newSpline.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            
            MeshRenderer meshRenderer = newSpline.AddComponent<MeshRenderer>();
            meshRenderer.material = Data.TreeMaterial;
            
            SplineContainer container = newSpline.AddComponent<SplineContainer>();
            container.RemoveSplineAt(0);
            
            CustomExtrudeSpline extrude = newSpline.AddComponent<CustomExtrudeSpline>();
            extrude.SplineContainer = container;
            extrude.Material = Data.TreeMaterial;

            SplineDataStruct data = new SplineDataStruct
            {
                Spline = container.AddSpline(),
                Container = container,
                Extrude = extrude,
                Object = newSpline,
            };
            
            return data;
        }
        

        private void CreateLeave(Transform t)
        {
            for (int i = 0; i < Data.LeavesPerBranch; i++)
            {
                Vector3 direction = Random.onUnitSphere;

                Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);

                Vector3 position = t.position + Random.insideUnitSphere * Data.LeafSpread;
                
                GameObject prefabLeaf = Data.LeafPrefabList[Random.Range(0, Data.LeafPrefabList.Count)];
                GameObject leaf = Object.Instantiate(prefabLeaf, position, rotation);

                leaf.transform.localScale = Vector3.one * Data.LeafScale;
                
                _leafList.Add(leaf.transform); 
            }
        }

        private GameObject EnfanteLeafs(GameObject tree)
        {
            if (tree == null) return null;
            
            foreach (var t in _leafList)
            {
                t.SetParent(tree.transform);
            }
            
            return tree;
        }
    }