using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
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
    bool m_isHorizontal;
    int m_rowCount, m_columnCount;
    Vector2 m_initialSize;
    Vector2 m_padding;
    Vector2 m_itemSize;

    public ESelectType selectType;
    
    RectTransform m_rectTransform;
    GridLayoutGroup m_gridLayoutGroup;
    ScrollRect m_scrollRect;
    GameObjectPool m_pool;

    public GameObject itemPrefab { get; set; }
    int m_itemCount;
    float m_scrollStep;
    bool m_isBoundsDirty = false;

    public int itemCount
    {
        get => m_itemList.Count;
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
        m_rectTransform = GetComponent<RectTransform>();
        m_gridLayoutGroup = GetComponent<GridLayoutGroup>();
        
        m_itemList = new List<ListViewItem>();
        m_selectedItemList = new List<ListViewItem>();
        m_scrollRect = m_rectTransform.GetComponentInParent<ScrollRect>();
        if(m_scrollRect == null)
            Debug.LogError("ListView can not find ScrollRect");

        GetLayoutAttribute();
    }

    void Update()
    {
        if(m_isBoundsDirty)
            UpdateBounds();
    }

    public void Init(GameObject prefab, Action<int, ListViewItem> refresh, Action<ListViewItem> valueChanged, Action<ListViewItem> clicked)
    {
        if (prefab.GetComponent<ListViewItem>() == null)
        {
            Debug.LogError("ListView prefab dont have ListViewItem Component");
            return;
        }

        itemPrefab = prefab;
        m_pool = new GameObjectPool(m_rectTransform, itemPrefab);
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
        go.transform.SetParent(m_rectTransform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        ListViewItem item = go.GetComponent<ListViewItem>();
        m_itemList.Add(item);
        item.Init(selectType, m_onItemValueChanged, m_onItemClicked);
        go.SetActive(true);
        SetBoundsDirty();
        return item;
    }

    public void RemoveItem(ListViewItem item)
    {
        if (item.isSelected)
            item.isSelected = false;
        m_pool.Put(item.gameObject);
        m_itemList.Remove(item);
        SetBoundsDirty();
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

    void SetBoundsDirty()
    {
        m_isBoundsDirty = true;
    }

    void UpdateBounds()
    {
        if (m_isHorizontal)
        {
            m_columnCount = Mathf.CeilToInt((float)itemCount / m_rowCount);
            m_rectTransform.sizeDelta = new Vector2(m_itemSize.x * m_columnCount, m_rectTransform.sizeDelta.y);
        }
        else
        {
            m_rowCount = Mathf.CeilToInt((float)itemCount / m_columnCount);
            m_rectTransform.sizeDelta = new Vector2(m_rectTransform.sizeDelta.x, m_itemSize.y * m_rowCount);
        }
        
        m_isBoundsDirty = false;
    }

    void GetLayoutAttribute()
    {
        m_initialSize = m_rectTransform.rect.size;
        m_padding = new Vector2(m_gridLayoutGroup.padding.left + m_gridLayoutGroup.padding.right, m_gridLayoutGroup.padding.top + m_gridLayoutGroup.padding.bottom);
        m_itemSize = m_gridLayoutGroup.cellSize + m_gridLayoutGroup.spacing;
        
        if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            m_isHorizontal = false;
            m_columnCount = m_gridLayoutGroup.constraintCount;
        }
        else if(m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            m_isHorizontal = true;
            m_rowCount = m_gridLayoutGroup.constraintCount;
        }
        else
        {
            if (m_gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                m_isHorizontal = false;
                m_columnCount = Mathf.FloorToInt((m_initialSize.x - m_padding.x) / m_itemSize.x);
            }
            else
            {
                m_isHorizontal = true;
                m_rowCount = Mathf.FloorToInt((m_initialSize.y - m_padding.y) / m_itemSize.y);
            }
        }
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
