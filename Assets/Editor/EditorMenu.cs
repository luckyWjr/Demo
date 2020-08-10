using UnityEngine;
using UnityEditor;

public class EditorMenu
{
    [MenuItem("Tools/ToolsWindow")]
    static void ShowToolsWindow()
    {
        var window = (ToolsWindow)EditorWindow.GetWindow(typeof(ToolsWindow), false, "Tools Window");
        window.Show();
    }
}