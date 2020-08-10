using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExcelBuild : Editor
{

    [MenuItem("Tools/Excel/CreateItemAsset")]
    public static void CreateItemAsset()
    {
        ItemManager manager = ScriptableObject.CreateInstance<ItemManager>();
        //赋值
        manager.dataArray = ExcelTool.CreateItemArrayWithExcel(ExcelConfig.excelsFolderPath + "Item.xlsx");

        //确保文件夹存在
        if (!Directory.Exists(ExcelConfig.assetPath))
        {
            Directory.CreateDirectory(ExcelConfig.assetPath);
        }

        //asset文件的路径 要以"Assets/..."开始，否则CreateAsset会报错
        string assetPath = string.Format("{0}{1}.asset", ExcelConfig.assetPath, "Item");
        //生成一个Asset文件
        AssetDatabase.CreateAsset(manager, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
