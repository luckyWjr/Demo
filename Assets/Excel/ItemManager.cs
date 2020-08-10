using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : ScriptableObject
{
    static ItemManager mInstance;
    public static ItemManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = Resources.Load<ItemManager>("DataAssets/Item");
            }
            return mInstance;
        }
    }
    public Item[] dataArray;
}
