using System;
using System.Collections.Generic;
using System.Linq;
using MyPrint;
using TreeGeneration;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    enum DataTreeType {FromImport, Custom}
    
    public class TreePaintingTool : EditorWindow
    {
        //Data Type
        [SerializeField] private DataTreeType _dataTreeType;
        
        //Import data
        [SerializeField] private TreeGeneratorData _data;
        
        //Custom Data
        [SerializeField] public string Axiom = "FB";
        [SerializeField] public int Iterations = 6;
        [SerializeField] public Vector2 Lenght = new Vector2(1, 2);
        [SerializeField] public float AngleMin;
        [SerializeField] public float AngleMax;
        [SerializeField] public float AngleYMin; 
        [SerializeField] public float AngleYMax;
        [SerializeField] public Material TreeMaterial;
        
        [Header("Radius")]
        [SerializeField] public float BaseRadius = 0.5f;
        [SerializeField] public float RadiusRatio = 0.95f;
        
        [Header("Leaves")]
        [SerializeField] public bool IsMultipleLeafType = false;
        [SerializeField] public List<GameObject> LeafPrefabList;
        [SerializeField] public GameObject LeafPrefabSolo;
        [SerializeField] public float LeafScale = 0.3f;
        [SerializeField] public int LeavesPerBranch = 3;
        [SerializeField] public float LeafSpread;
        
        [Header("Wobble")]
        [SerializeField] public float Wobble = 0.5f;
        [SerializeField] public float Tropism;
        
        [Header("Rules")] 
        [SerializeField] public List<BranchRules> BranchRules = new List<BranchRules>();
        
        //Tool variables
        private bool _isPainting = false;
        private TreeGenerator generator = new TreeGenerator();
        HashSet<GameObject> _forest = new HashSet<GameObject>();
        private Transform _forestParent;
        private Tool _previousTool;
        private Vector2 _scrollPosition;
        
        //Cursor
        private Texture2D _paintCursor;
        private Texture2D _burnCursor;
        
        //Styles
        GUIStyle _headerStyle = new GUIStyle();
        GUIStyle _subTitleStyle = new GUIStyle();
        GUIStyle _scrollViewStyle = new GUIStyle();
        Texture2D _normalTexture;
        Texture2D _paintTexture;
        
        [MenuItem("Tools/Tree Painting Tool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TreePaintingTool), false, "Tree Painting Tool");
        }

        void OnEnable()
        {
            _paintCursor = Resources.Load<Texture2D>("PaintCursor");
            _burnCursor = Resources.Load<Texture2D>("BurnCursor");
            
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.normal.textColor = Color.white;
            _headerStyle.fontSize = 16;

            _subTitleStyle.alignment = TextAnchor.MiddleCenter;
            _subTitleStyle.normal.textColor = Color.white;
            _subTitleStyle.fontStyle = FontStyle.Bold;
            _subTitleStyle.fontSize = 24;

            _normalTexture = MakeTexture(1, 1, Color.clear);
            _paintTexture = MakeTexture(1, 1, new  Color(0.15f, 0.6f, 0, 0.5f));
            _scrollViewStyle.normal.background = _normalTexture;
            
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, _scrollViewStyle);
            
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontSize = 36;
            
            GUILayout.Label("Tree generator 🌳", titleStyle);
            
            GUILayout.Space(36);
            
            _dataTreeType = (DataTreeType)EditorGUI.Popup(new Rect(0, 76, 150, 16), (int)_dataTreeType, Enum.GetNames(typeof(DataTreeType)));
            
            GUILayout.Space(24);
            
            switch (_dataTreeType)
            {
                case DataTreeType.FromImport:
                    GUILayout.BeginHorizontal();
                    
                    GUILayout.Label("Import Data", _subTitleStyle);
                    _data = EditorGUILayout.ObjectField(_data, typeof(TreeGeneratorData), true) as TreeGeneratorData;
                   
                    GUILayout.EndHorizontal();
                    break;
                case DataTreeType.Custom:
                    ShowCustomParameter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            GUILayout.Label("Pain shortcut => 'P'", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                }
            });
            
            GUILayout.BeginHorizontal();

            int paintButtonHeight = 40;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                fixedHeight = paintButtonHeight,
                normal = new GUIStyleState
                {
                    background = MakeTexture(1, 1, new  Color(0.15f, 0.6f, 0, 0.5f)),
                }
            };
            
            if(GUILayout.Button("Start Painting 🖌️", buttonStyle))
            {
                StartPainting();
            }

            buttonStyle.normal.background = MakeTexture(1, 1, new Color(0.6f, 0.2f, 0.2f, 0.5f));
            
            if(GUILayout.Button("Stop Painting 🛑", buttonStyle))
            {
                StopPainting();   
            }
            
            
            GUILayout.EndHorizontal();

            GUILayout.Space(16);
            
            if (GUILayout.Button("Burn the forest 🔥", GUILayout.Height(30)))
            {
                foreach (var tree in _forest)
                {
                    if (tree.TryGetComponent<TreeAnimation>(out TreeAnimation anim))
                    {
                        anim.StartDisappear();
                    }
                }
                
                _forest.Clear();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void ShowCustomParameter()
        {
            GUILayout.Label("Custom Parameter", _subTitleStyle);
            
            GUILayout.BeginVertical();
            GUILayout.Space(8);

            GUILayout.Label("Settings", _headerStyle);
            Axiom = EditorGUILayout.TextField("Axiom", Axiom);
            Iterations = EditorGUILayout.IntField("Iterations", Iterations);
            Lenght = EditorGUILayout.Vector2Field("Lenght", Lenght);
            AngleMin = EditorGUILayout.FloatField("AngleMin", AngleMin);
            AngleMax = EditorGUILayout.FloatField("AngleMax", AngleMax);
            AngleYMin = EditorGUILayout.FloatField("AngleYMin", AngleYMin);
            AngleYMax = EditorGUILayout.FloatField("AngleYMax", AngleYMax);
            TreeMaterial = EditorGUILayout.ObjectField(TreeMaterial, typeof(Material), true) as Material;
            GUILayout.Space(8);

            GUILayout.Label("Radius", _headerStyle);
            BaseRadius = EditorGUILayout.FloatField("BaseRadius", BaseRadius);
            RadiusRatio = EditorGUILayout.Slider("RadiusRatio", RadiusRatio, 0.1f, 1f);
            
            GUILayout.Space(8);

            GUILayout.Label("Leaves", _headerStyle);
            
            IsMultipleLeafType = EditorGUILayout.Toggle("Multiple leaf type", IsMultipleLeafType);         
            
            if(IsMultipleLeafType) 
                ShowLeafPrefabList();
            else
                LeafPrefabSolo = EditorGUILayout.ObjectField(LeafPrefabSolo, typeof(GameObject), true) as GameObject;
            
            LeafScale = EditorGUILayout.FloatField("LeafScale", LeafScale);
            LeavesPerBranch = EditorGUILayout.IntField("LeavesPerBranch", LeavesPerBranch);
            LeafSpread = EditorGUILayout.FloatField("LeafSpread", LeafSpread);

            GUILayout.Space(8);
            
            Wobble = EditorGUILayout.FloatField("Wobble", Wobble);
            Tropism = EditorGUILayout.FloatField("Tropism", Tropism);
            
            GUILayout.Space(8);

            GUILayout.Label("Rules", _headerStyle);
            
            GUIStyle dictionaryButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fixedHeight = 20,
                fixedWidth = 120,
            };
            
            if (GUILayout.Button("Open rules dictionary", dictionaryButtonStyle))
            {
                WindowDictAction.OpenWindow();
            }
            
            ShowBranchRules();
            
            GUILayout.Space(12);
            
            GUIStyle saveButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fixedHeight = 30,
                fixedWidth = 150,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    background = MakeTexture(1, 1, Color.lightBlue),
                }
            };
            
            GUILayout.Label("Will create a SO with above parameters at : Assets/Data", new GUIStyle
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });
            
            GUILayout.Space(4);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save Custom Data Tree", saveButtonStyle))
            {
                CreateTreeData();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(16);
            
            GUILayout.EndVertical();
        }
        
        private void ShowLeafPrefabList()
        {
            GUILayout.Label("Leaf Prefabs", EditorStyles.boldLabel);

            if (LeafPrefabList == null)
                LeafPrefabList = new List<GameObject>();

            for (int i = 0; i < LeafPrefabList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                LeafPrefabList[i] = EditorGUILayout.ObjectField(
                    $"Prefab {i}",
                    LeafPrefabList[i],
                    typeof(GameObject),
                    false
                ) as GameObject;

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    LeafPrefabList.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Prefab"))
            {
                LeafPrefabList.Add(null);
            }
        }
        
        private void ShowBranchRules()
        {
            for (int i = 0; i < BranchRules.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                string rule = EditorGUILayout.TextField(
                    "Rule",
                    BranchRules[i].Rule.ToString()
                );

                BranchRules[i].Rule = string.IsNullOrEmpty(rule) ? '\0' : rule[0];

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    BranchRules.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Label("Probabilities", EditorStyles.boldLabel);

                for (int j = 0; j < BranchRules[i].BranchRulesProbabilityList.Count; j++)
                {
                    var probability = BranchRules[i].BranchRulesProbabilityList[j];

                    EditorGUILayout.BeginHorizontal();

                    probability.Rule = EditorGUILayout.TextField(
                        "Rule : " + probability.Rule,
                        probability.Rule
                    );

                    probability.Probability = EditorGUILayout.Slider(probability.Probability, 0f, 100f);

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        BranchRules[i].BranchRulesProbabilityList.RemoveAt(j);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Probability"))
                {
                    BranchRules[i].BranchRulesProbabilityList.Add(
                        new BranchRulesProbability()
                    );
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Branch Rule"))
            {
                BranchRules.Add(new BranchRules());
            }
        }
        
        private void CreateTreeData()
        {
            TreeGeneratorData data = CreateNewDataFromCustomParameters();

            string folderPath = "Assets/Data";
            string assetPath = $"{folderPath}/NewTreeData.asset";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"Folder doesn't exist: {folderPath}");
                DestroyImmediate(data);
                return;
            }

            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = data;
            EditorGUIUtility.PingObject(data);

            _data = data;
        }
        
        private void StartPainting()
        {
            _isPainting = true;
            _previousTool = Tools.current;
            Tools.current = Tool.None;
            
            _scrollViewStyle.normal.background = _paintTexture;
        }

        private void StopPainting()
        {
            _isPainting = false;
            Tools.current = _previousTool;
            
            _scrollViewStyle.normal.background = _normalTexture;
        }
        
        private void OnSceneGUI(SceneView scene)
        {
            Event e = Event.current;

            if(e.keyCode == KeyCode.P)
            {
                if (e.type == EventType.KeyDown)
                {
                    StartPainting();
                }else if (e.type == EventType.KeyUp)
                {
                    StopPainting();
                }
            }
            
            if (!_isPainting)
                return;
            
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (e.type == EventType.Repaint) { DrawCustomCursor(e); }
            if (e.type == EventType.MouseMove) { scene.Repaint(); }
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (_data == null)
                {
                    ABPrint.Print("No Data", ABColor.Red);
                    e.Use();
                    return;
                }

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (e.alt)
                    {
                        DestroyTree(hit.collider.gameObject);
                        e.Use();
                        return;
                    }
                    
                    CreateNewTree(hit.point);
                }

                e.Use();
            }
        }

        private void CreateNewTree(Vector3 pos)
        {
            GameObject go = new GameObject
            {
                transform =
                {
                    position = pos
                }
            };

            if (generator == null)
            {
                ABPrint.Print("No Tree Generator", ABColor.Red);
                return;
            }

            if (_forestParent == null)
            {
                GameObject forest = new GameObject("Forest");
                _forestParent = forest.transform;
            }

            TreeGeneratorData data = CreateInstance<TreeGeneratorData>();

            if (_dataTreeType == DataTreeType.FromImport)
                data = _data;
            else if(_dataTreeType == DataTreeType.Custom)
            {
                data = CreateNewDataFromCustomParameters();
            }
            
            GameObject newTree = generator.GenerateTree(data, go.transform, _forest.Count);
            newTree.AddComponent<TreeAnimation>().PlayAppear();
            newTree.transform.localScale = Vector3.zero;
                    
            newTree.transform.SetParent(_forestParent);
                    
            _forest.Add(newTree);
                    
            DestroyImmediate(go);
        }

        private TreeGeneratorData CreateNewDataFromCustomParameters()
        {
            TreeGeneratorData data = CreateInstance<TreeGeneratorData>();
            data.Axiom = Axiom;
            data.Iterations = Iterations;
            data.Lenght = Lenght;
            
            data.AngleMin = AngleMin;
            data.AngleMax = AngleMax;
            data.AngleYMin = AngleYMin;
            data.AngleYMax = AngleYMax;
            
            data.TreeMaterial = TreeMaterial;
            
            data.BaseRadius = BaseRadius;
            data.RadiusRatio = RadiusRatio;
            
            if(!IsMultipleLeafType)
                LeafPrefabList.Add(LeafPrefabSolo);
            
            data.LeafPrefabList = new List<GameObject>(LeafPrefabList);
            
            data.LeafScale = LeafScale;
            data.LeavesPerBranch = LeavesPerBranch;
            data.LeafSpread = LeafSpread;

            data.Wobble = Wobble;
            data.Tropism = Tropism;
            
            data.BranchRules = BranchRules;
            
            return data;
        }
        
        private void DestroyTree(GameObject tree)
        {
            if(!_forest.Contains(tree))
                return;
            
            if (tree.TryGetComponent(out TreeAnimation anim))
            {
                anim.StartDisappear();
                _forest.Remove(tree);
            }
        }
        
        private void DrawCustomCursor(Event e)
        {
            Texture2D cursor = e.alt ? _burnCursor : _paintCursor;

            if (cursor == null)
            {
                ABPrint.Print("No cursor Texture", ABColor.Red);
                return;
            }

            float size = 32f;

            Rect cursorRect = new Rect(
                e.mousePosition.x - size * 0.5f,
                e.mousePosition.y - size * 0.5f,
                size,
                size
            );

            Handles.BeginGUI();

            GUI.DrawTexture(cursorRect, cursor, ScaleMode.ScaleToFit, true);

            Handles.EndGUI();
        }
        
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}