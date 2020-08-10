using UnityEditor;
using UnityEngine;

public class ToolsWindow : EditorWindow
{
    string m_ids = "";//输入的InstanceID

    string m_log = "";//log信息
    Vector2 m_logScroll;

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        //查找GameObject
        {
            GUILayout.Label("1.通过GameObject的唯一ID(InstanceID)定位");
            GUILayout.BeginHorizontal();
            m_ids = GUILayout.TextField(m_ids, GUILayout.Width(120), GUILayout.Height(20));
            if (GUILayout.Button("定位", GUILayout.Width(60), GUILayout.Height(20)))
            {
                m_log = string.Empty;
                if (int.TryParse(m_ids, out int id))
                {
                    GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
                    if (go != null)
                    {
                        Selection.activeGameObject = go; //在Hierarchy窗口选中该GameObject
                        m_log = $"查找成功 Name为{go.name}";
                    }
                    else
                        m_log = $"没有找到ID为{id}的GameObject";
                }
                else
                    m_log = "请输入正确的ID";
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
        //是否开启Hierarchy显示InstanceID的功能
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("2.是否开启Hierarchy面板显示InstanceID的功能", GUILayout.Width(260), GUILayout.Height(20));

            if (GUILayout.Toggle(CustomHierarchy.isShowInstanceID, ""))
                CustomHierarchy.OpenShowInstanceID();
            else
                CustomHierarchy.CloseShowInstanceID();
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
        //显示Log
        {
            if (!string.IsNullOrEmpty(m_log))
            {
                m_logScroll = GUILayout.BeginScrollView(m_logScroll);
                m_log = EditorGUILayout.TextArea(m_log, GUILayout.Height(80));
                EditorGUILayout.EndScrollView();
            }
        }
        GUILayout.EndVertical();
    }
}