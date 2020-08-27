using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(ContentSizeFitter))]
public class ListView : MonoBehaviour
{
    public enum ESelectType
    {
        Single,
        Multiple
    }

    Action<int, ListViewItem> m_onItemRefresh;
    Action<ListViewItem> m_onItemValueChanged;
    Action<ListViewItem> m_onItemClicked;
    
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
            int oldCount = m_itemList.Count;
            if (value > oldCount)
            {
                for (int i = oldCount; i < value; i++)
                {
                    AddItem();
                }
            }
            else
            {
                RemoveItem(value, oldCount - 1);
            }

            Refresh();
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

    public void Init(GameObject prefab, Action<int, ListViewItem> refresh, Action<ListViewItem> valueChanged, Action<ListViewItem> clicked)
    {
        if (prefab.GetComponent<ListViewItem>() == null)
        {
            Debug.LogError("ListView prefab dont have ListViewItem Component");
            return;
        }

        itemPrefab = prefab;
        m_pool = new GameObjectPool(m_transform, itemPrefab);
        m_onItemRefresh = refresh;
        m_onItemValueChanged = valueChanged;
        m_onItemValueChanged += OnValueChanged;
        m_onItemClicked = clicked;
    }

    public void Refresh()
    {
        for (int i = 0, count = m_itemList.Count; i < count; i++)
        {
            ListViewItem item = m_itemList[i];
            if (item.isSelected)
                item.isSelected = false;
            m_onItemRefresh?.Invoke(i, item);
        }
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
        item.Init(selectType, m_onItemValueChanged, m_onItemClicked);
        go.SetActive(true);
        return item;
    }

    public void RemoveItem(ListViewItem item)
    {
        if (item.isSelected)
            item.isSelected = false;
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
    
    void OnValueChanged(ListViewItem item)
    {
        if (item.isSelected)
        {
            if (selectType == ESelectType.Single)
            {
                if (m_selectedItemList.Count > 0)
                    m_selectedItemList[0].isSelected = false;
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
        // foreach (var item in m_itemList)
            // item.RemoveOnClickedHandle(OnValueChanged);
        m_pool?.Clear();
    }
}
