using UnityEditor;
using UnityEngine;

//添加这个标签，可以选择多个拥有Test组件的GameObject进行同时修改，否则会提示Multi-object editing not supported
[CanEditMultipleObjects]

//关联对应的MonoBehaviour类
[CustomEditor(typeof(Test))]
public class TestInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Test test = target as Test;

        //添加了这句，当你选中一个带有Test组件的GameObject时，修改了属性，点击工具栏Edit，会显示Undo RecordTest和Redo RecordTest用于撤销和重做
        //同时如果是在prefab中，加上这句，在修改了属性之后prefab会知道属性的变化，你可以apply或revert，否则prefab无法检测出。（5.3版本之前使用EditorUtility.SetDirty(test);）
        Undo.RecordObject(test, "RecordTest");

        //显示默认视图
        base.OnInspectorGUI();

        //在默认视图下方添加一行
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Button 1"))
        {
            //点击按钮触发
            Debug.Log("name:" + test.name);

            test.speed = 100;

            //通知Prefab属性变化，5.3版本前使用下面这句，之后使用Undo.RecordObject
            //EditorUtility.SetDirty(test);
        }
        if (GUILayout.Button("Button 2"))
        {
            //targets 表示选中的多个组件
            foreach (Object obj in targets)
            {
                Debug.Log("name:" + obj.name);
                Test item = obj as Test;
                item.Reset();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            //如果属性改变时调用
        }
    }
}
