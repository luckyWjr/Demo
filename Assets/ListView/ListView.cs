using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[RequireComponent(typeof(GridLayoutGroup))]
public class ListView : MonoBehaviour
{
    public enum ESelectType
    {
        Single,
        Multiple
    }

    public Action<int, ListViewItem> onItemRefresh;
    
    bool m_isVirtual;

    public ESelectType selectType;
    
    Transform m_transform;
    GridLayoutGroup m_gridLayoutGroup;
    ScrollRect m_scrollRect;
    GameObjectPool m_pool;

    public GameObject itemPrefab { get; set; }
    int m_itemCount;

    public int itemCount
    {
        get => m_itemCount;
        set
        {
            int count = m_itemList.Count;
            if (value > count)
            {
                for (int i = count; i < value; i++)
                {
                    AddItem();
                }
            }
            else
            {
                RemoveItem(value, count - 1);
            }

            if (onItemRefresh != null)
            {
                for (int i = 0; i < value; i++)
                {
                    onItemRefresh.Invoke(i, m_itemList[i]);
                }
            }
        }
    }

    List<ListViewItem> m_itemList;

    List<ListViewItem> m_selectedItemList;
    // public List<ListViewItem> selectedItemList => m_selectedItemList;
    
    void Awake()
    {
        m_transform = transform;
        m_gridLayoutGroup = GetComponent<GridLayoutGroup>();
        
        m_itemList = new List<ListViewItem>();
        m_selectedItemList = new List<ListViewItem>();
    }

    public void Init(GameObject prefab, int num, Action<int, ListViewItem> action = null)
    {
        if (prefab.GetComponent<ListViewItem>() == null)
        {
            Debug.LogError("ListView prefab dont have ListViewItem Component");
            return;
        }

        itemPrefab = prefab;
        m_pool = new GameObjectPool(m_transform, itemPrefab);
        onItemRefresh = action;
        itemCount = num;
    }
    
    public ListViewItem AddItem()
    {
        if (itemPrefab == null) return null;

        GameObject go = m_pool.Get();
        go.transform.SetParent(m_transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        ListViewItem item = go.GetComponent<ListViewItem>();
        m_itemList.Add(item);
        item.AddValueChangedHandle(OnValueChanged);
        go.SetActive(true);
        return item;
    }

    public void RemoveItem(ListViewItem item)
    {
        item.ClearValueChangedHandle();
        if (item.isOn)
        {
            item.isOn = false;
            m_selectedItemList.Remove(item);
        }
        m_pool.Put(item.gameObject);
        m_itemList.Remove(item);
    }
    
    public void RemoveItem(int index)
    {
        if(index < 0 || index >= m_itemList.Count) return;
        RemoveItem(m_itemList[index]);
    }
    
    public void RemoveItem(int beginIndex, int endIndex)
    {
        if(beginIndex > endIndex) return;
        for (int i = beginIndex; i <= endIndex; i++)
            RemoveItem(beginIndex);
    }
    
    void OnValueChanged(ListViewItem item, bool isOn)
    {
        if (isOn)
        {
            if (selectType == ESelectType.Single)
            {
                if (m_selectedItemList.Count > 0)
                {
                    m_selectedItemList[0].isOn = false;
                    m_selectedItemList.Clear();
                }
                m_selectedItemList.Add(item);
            }
            else
                m_selectedItemList.Add(item);
        }
        else
        {
            if (selectType == ESelectType.Single)
                m_selectedItemList.Clear();
            else
                m_selectedItemList.Remove(item);
        }
    }

    void OnDestroy()
    {
        foreach (var item in m_itemList)
            item.RemoveValueChangedHandle(OnValueChanged);
        m_pool?.Clear();
    }
}
