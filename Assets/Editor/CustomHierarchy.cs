using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class CustomHierarchy
{
    static GUIStyle m_style;
    static int m_isShowInstanceIDTagValue;
    const string IsShowInstanceIDTag = "IsShowInstanceIDTag";

    public static bool isShowInstanceID { get; private set; }

    static CustomHierarchy()
    {
        m_style = new GUIStyle();
        m_style.alignment = TextAnchor.MiddleRight;
        m_style.normal.textColor = Color.gray;

        //EditorApplication.searchChanged = HierarchySearchChanged;
        //UnityEditor.EditorUtility.audioMasterMute
        //GUIUtility.keyboardControl

        m_isShowInstanceIDTagValue = PlayerPrefs.GetInt(IsShowInstanceIDTag, 0);
        isShowInstanceID = false;
        if (m_isShowInstanceIDTagValue == 1)
            OpenShowInstanceID();
    }    

    public static void OpenShowInstanceID()
    {
        if (!isShowInstanceID)
        {
            isShowInstanceID = true;
            PlayerPrefs.SetInt(IsShowInstanceIDTag, 1);
            EditorApplication.hierarchyWindowItemOnGUI += ShowInstanceID;
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    public static void CloseShowInstanceID()
    {
        if (isShowInstanceID)
        {
            isShowInstanceID = false;
            PlayerPrefs.SetInt(IsShowInstanceIDTag, 0);
            EditorApplication.hierarchyWindowItemOnGUI -= ShowInstanceID;
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    static void HierarchySearchChanged()
    {
        //var go = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
    }

    static void ShowInstanceID(int instanceId, Rect selectionRect)
    {
        //显示Hierarchy选中的GameObject的InstanceID
        if (instanceId == Selection.activeInstanceID)
        {
            Rect rect = new Rect(50, selectionRect.y, selectionRect.width, selectionRect.height);
            GUI.Label(rect, instanceId.ToString(), m_style);
        }
    }
}