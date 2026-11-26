using UnityEditor;
using UnityEngine;

public class WindowDictAction : EditorWindow
{
    [MenuItem("Tools/Rules Dictionary")]
    public static void OpenWindow()
    {
        WindowDictAction window = GetWindow<WindowDictAction>(false, "Tree Settings");

        window.minSize = new Vector2(300, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Rules dictionary", new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 20,
            normal = new GUIStyleState
            {
                textColor = Color.white,
            }
        });
        
        
    }
}