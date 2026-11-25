using System.Collections.Generic;
using MyPrint;
using TreeGeneration;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class TreePaintingTool : EditorWindow
    {
        [SerializeField] private TreeGeneratorData _data;
        private bool _isPainting = false;
        
        TreeGenerator generator = new TreeGenerator();
        
        HashSet<GameObject> _forest = new HashSet<GameObject>();
        private Transform _forestParent;
        
        private Tool _previousTool;
        
        //Cursor
        private Texture2D _paintCursor;
        private Texture2D _burnCursor;
        
        [MenuItem("Tools/Tree Painting Tool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TreePaintingTool), false, "Tree Painting Tool");
        }

        void OnEnable()
        {
            _paintCursor = Resources.Load<Texture2D>("PaintCursor");
            _burnCursor = Resources.Load<Texture2D>("BurnCursor");
            
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnGUI()
        {
            _data = EditorGUILayout.ObjectField(_data, typeof(TreeGeneratorData), true) as TreeGeneratorData;

            if (_isPainting)
            {
                GUIStyle style = new GUIStyle
                {
                    normal =
                    {
                        textColor = Color.red
                    }
                };

                GUILayout.Label("Painting", style);
            }
            
            GUILayout.BeginHorizontal();
            
            if(GUILayout.Button("Start Painting"))
            {
                StartPainting();
            }
            if(GUILayout.Button("Stop Painting"))
            {
                StopPainting();   
            }
            
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Burn the forest"))
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
        }
        
        private void StartPainting()
        {
            _isPainting = true;
            _previousTool = Tools.current;
            Tools.current = Tool.None;
        }

        private void StopPainting()
        {
            _isPainting = false;
            Tools.current = _previousTool;
        }
        
        private void OnSceneGUI(SceneView scene)
        {
            Event e = Event.current;

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
            
            GameObject newTree = generator.GenerateTree(_data, go.transform, _forest.Count);
            newTree.AddComponent<TreeAnimation>().PlayAppear();
            newTree.transform.localScale = Vector3.zero;
                    
            newTree.transform.SetParent(_forestParent);
                    
            _forest.Add(newTree);
                    
            DestroyImmediate(go);
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
    }
}