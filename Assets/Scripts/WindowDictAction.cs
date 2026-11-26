using UnityEditor;
using UnityEngine;

public class WindowDictAction : EditorWindow
{
    GUIStyle ruleNameStyle;
    GUIStyle ruleDescriptionStyle;
    GUIStyle arrowDescriptionStyle;
    
    [MenuItem("Tools/Rules Dictionary")]
    public static void OpenWindow()
    {
        WindowDictAction window = GetWindow<WindowDictAction>(false, "Tree Settings");

        window.minSize = new Vector2(300, 300);
        window.Show();
    }

    private void OnEnable()
    {
        ruleNameStyle = new GUIStyle
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            normal = new GUIStyleState
            {
                textColor = Color.white,
            }
        };

        ruleDescriptionStyle = new GUIStyle
        {
            fontSize = 12,
            normal = new GUIStyleState
            {
                textColor = Color.black,
            }
        };

        arrowDescriptionStyle = new GUIStyle
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            normal = new GUIStyleState
            {
                textColor = Color.grey,
            }
        };
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
        
        GUILayout.Space(8);
        ShowRuleInfo("F", "Advanced 'Lenght' upwards");
        GUILayout.Space(8);
        ShowRuleInfo("B", "End of the branch");
        GUILayout.Space(8);
        ShowRuleInfo("[", "Start of the branch mesh");
        GUILayout.Space(8);
        ShowRuleInfo("]", "End of the branch mesh -> back to the last branch started");
        GUILayout.Space(8);
        ShowRuleInfo("l", "Rotate to the left");
        GUILayout.Space(8);
        ShowRuleInfo("r", "Rotate to the right");
        GUILayout.Space(8);
        ShowRuleInfo("L", "Add a leaf");
        GUILayout.Space(8);        
        ShowRuleInfo("y", "Pure rotation, change 2d plan");
        GUILayout.Space(8);        
        ShowRuleInfo("w", "Random wave");
        GUILayout.Space(8);
        ShowRuleInfo("g", "Curve : >0 curve to the bottom, <0 curve to the top");
        GUILayout.Space(8);
        ShowRuleInfo("Any other letter", "Can create pattern with all the above actions");
        GUILayout.Space(8);
    }
    
    private void ShowRuleInfo(string ruleName, string ruleDescription)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(ruleName, ruleNameStyle);
        
        GUILayout.Space(2);
        GUILayout.Label("=>", arrowDescriptionStyle);
        GUILayout.Space(2);
        
        GUILayout.Label(ruleDescription, ruleDescriptionStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}