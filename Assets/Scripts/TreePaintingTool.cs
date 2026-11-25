using MyPrint;
using TreeGeneration;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class TreePaintingTool : EditorWindow
    {
        [SerializeField] private TreeGeneratorData _data;
        private bool _isPainting = false;
        
        TreeGenerator generator = new TreeGenerator();
        
        [MenuItem("Tools/Tree Painting Tool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TreePaintingTool), false, "Tree Painting Tool");
        }

        void OnEnable()
        {
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
                _isPainting = true;
            }
            if(GUILayout.Button("Stop Painting"))
            {
                _isPainting = false;
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void OnSceneGUI(SceneView scene)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (!_isPainting) return;
                
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                ABPrint.Print("Click", ABColor.White);

                if (_data == null)
                {
                    ABPrint.Print("No Data", ABColor.Red);
                }
                
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
                {
                    Debug.Log(hit.transform.name);
                    
                    GameObject go = new GameObject
                    {
                        transform =
                        {
                            position = hit.point
                        }
                    };

                    if (generator == null)
                    {
                        ABPrint.Print("No Tree Generator", ABColor.Red);
                        return;
                    }
                    
                    generator.GenerateTree(_data, go.transform);
                }
                e.Use();

            }
        }
    }
}