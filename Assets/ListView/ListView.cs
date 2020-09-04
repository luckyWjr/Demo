using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// [ExecuteInEditMode]
public class ListView : MonoBehaviour
{
    public enum ESelectType
    {
        Single,
        Multiple
    }
    
    public enum EFlowType
    {
        Horizontal,
        Vertical,
    }
    
    class ItemInfo
    {
        public ListViewItem item;
        public bool isSelected;
    }
    
    [SerializeField] bool m_isVirtual;
    [SerializeField] ESelectType m_selectType;
    [SerializeField] EFlowType m_flowType;
    [SerializeField] int m_constraintCount;
    [SerializeField] Vector2 m_itemSpace;
    
    public GameObject itemPrefab { get; set; }
    
    Action<int, ListViewItem> m_onItemRefresh;
    Action<ListViewItem> m_onItemValueChanged;
    Action<ListViewItem> m_onItemClicked;
    
    int m_rowCount, m_columnCount;
    Vector2 m_initialSize;
    Vector2 m_itemSize;
    
    RectTransform m_rectTransform;
    ScrollRect m_scrollRect;
    GameObjectPool m_pool;

    List<ItemInfo> m_itemInfoList;

    float m_scrollStep;
    bool m_isBoundsDirty = false;

    int m_itemRealCount;
    public int itemCount
    {
        get => m_isVirtual ? m_itemRealCount : m_itemList.Count;
        set
        {
            ResetPosition();
            if (m_isVirtual)
            {
                m_itemRealCount = value;
                SetSize();
                int oldCount = m_itemInfoList.Count;
                if (m_itemRealCount > oldCount)
                {
                    for (int i = oldCount; i < m_itemRealCount; i++)
                    {
                        ItemInfo info = new ItemInfo();
                        m_itemInfoList.Add(info);
                    }
                }
                else
                {
                    for (int i = m_itemRealCount; i < oldCount; i++)
                    {
                        if (m_itemInfoList[i].item != null)
                        {
                            RemoveItem(m_itemInfoList[i].item);
                            m_itemInfoList[i].item = null;
                        }
                    }
                }
                RenderVirtualItem(true);
            }
            else
            {
                int oldCount = m_itemList.Count;
                if (value > oldCount)
                {
                    for (int i = oldCount; i < value; i++)
                        AddItem();
                }
                else
                    RemoveItem(value, oldCount - 1);

                Refresh();
            }
        }
    }

    List<ListViewItem> m_itemList;

    List<ListViewItem> m_selectedItemList;
    // public List<ListViewItem> selectedItemList => m_selectedItemList;
    
    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_rectTransform.pivot = Vector2.up;
        m_rectTransform.anchorMax = Vector2.up;
        m_rectTransform.anchorMin = Vector2.up;
        
        m_itemList = new List<ListViewItem>();
        m_selectedItemList = new List<ListViewItem>();
        m_itemInfoList = new List<ItemInfo>();
        m_scrollRect = m_rectTransform.GetComponentInParent<ScrollRect>();
        if(m_scrollRect == null)
            Debug.LogError("ListView can not find ScrollRect");
        m_scrollRect.onValueChanged.AddListener(OnScroll);
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
        
        GetLayoutAttribute();
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
        go.transform.localScale = Vector3.one;
        ListViewItem item = go.GetComponent<ListViewItem>();
        m_itemList.Add(item);
        item.Init(m_selectType, m_onItemValueChanged, m_onItemClicked);
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
        if(m_isVirtual)
            return;
        SetSize();
        for (int i = 0, count = itemCount; i < count; i++)
        {
            m_itemList[i].transform.localPosition = CalculatePosition(i);
        }
        
        m_isBoundsDirty = false;
    }

    void SetSize()
    {
        m_rectTransform.sizeDelta = m_flowType == EFlowType.Horizontal ? new Vector2(GetContentLength(), m_initialSize.y) : new Vector2(m_initialSize.x, GetContentLength());
    }

    void ResetPosition()
    {
        m_rectTransform.localPosition = Vector3.zero;
    }
    
    Vector2 CalculatePosition(int index)
    {
        int row, column;
        if (m_flowType == EFlowType.Horizontal)
        {
            row = index % m_rowCount;
            column = index / m_rowCount;
        }
        else
        {
            row = index / m_columnCount;
            column = index % m_columnCount;
        }
        
        float x = column * (m_itemSize.x + m_itemSpace.x) + m_itemSize.x / 2;
        float y = row * (m_itemSize.y + m_itemSpace.y) + m_itemSize.y / 2;
        
        return new Vector2(x, -y);
    }
    
    float GetContentLength()
    {
        if (m_flowType == EFlowType.Horizontal)
        {
            int columnCount = Mathf.CeilToInt((float)itemCount / m_rowCount);
            return m_itemSize.x * columnCount + m_itemSpace.x * (columnCount - 1);
        }
        else
        {
            int rowCount = Mathf.CeilToInt((float)itemCount / m_columnCount);
            return m_itemSize.y * rowCount + m_itemSpace.y * (rowCount - 1);
        }
    }

    void OnScroll(Vector2 position)
    {
        RenderVirtualItem(false);
    }

    void RenderVirtualItem(bool isForceRender)
    {
        if (m_flowType == EFlowType.Horizontal)
        {
            ScrollHorizontal(isForceRender);
        }
        else
        {
            ScrollVertical(isForceRender);
        }
    }

    void ScrollVertical(bool isForceRender)
    {
        float currentY = m_rectTransform.localPosition.y;
        // Debug.Log("currentY:"+currentY);
        int startIndex = GetCurrentIndex(currentY);
        // Debug.Log("startIndex:"+startIndex);
        float endY = -currentY - m_initialSize.y - m_itemSize.y - m_itemSpace.y;
        // Debug.Log("endY:"+endY);
        int endIndex = GetCurrentIndex(-endY);
        // Debug.Log("endIndex:"+endIndex);
        for (int i = startIndex; i < itemCount && i < endIndex; i++)
        {
            bool needRender = false;
            // if(CalculatePosition(i).y < endY)
            //     break;
            ItemInfo info = m_itemInfoList[i];

            //scroll down
            if (info.item == null)
            {
                for (int j = 0; j < startIndex; j++)
                {
                    if (m_itemInfoList[j].item != null)
                    {
                        info.item = m_itemInfoList[j].item;
                        m_itemInfoList[j].item = null;
                        needRender = true;
                        break;
                    }
                }
            }
            
            if (info.item == null)
            {
                for (int j = endIndex; j < itemCount; j++)
                {
                    if (m_itemInfoList[j].item != null)
                    {
                        info.item = m_itemInfoList[j].item;
                        m_itemInfoList[j].item = null;
                        needRender = true;
                        break;
                    }
                }
            }
            
            if (info.item == null)
            {
                info.item = AddItem();
                needRender = true;
            }

            if (isForceRender || needRender)
            {
                info.item.transform.localPosition = CalculatePosition(i);
                m_onItemRefresh?.Invoke(i, info.item);
            }
        }
    }
    
    void ScrollHorizontal(bool isForceRender)
    {
        int startIndex = GetCurrentIndex(m_rectTransform.localPosition.x);
    }

    //当前页面显示的item中，第一个item的下标
    int GetCurrentIndex(float position)
    {
        if (m_flowType == EFlowType.Horizontal)
        {
            position = -position;
            if (position < m_itemSize.x) return 0;
            position -= m_itemSize.x;
            return (Mathf.FloorToInt(position / (m_itemSize.x + m_itemSpace.x)) + 1) * m_rowCount;
        }
        else
        {
            if (position < m_itemSize.y) return 0;
            position -= m_itemSize.y;
            return (Mathf.FloorToInt(position / (m_itemSize.y + m_itemSpace.y)) + 1) * m_columnCount;
        }
    }

    public void SetVirtual()
    {
        if (!m_isVirtual)
        {
            m_itemInfoList.Clear();
            m_isVirtual = true;
        }
    }

    void GetLayoutAttribute()
    {
        m_itemSize = itemPrefab.GetComponent<RectTransform>().rect.size;
        m_initialSize = m_rectTransform.parent.GetComponent<RectTransform>().rect.size;//Viewport Size

        if (m_flowType == EFlowType.Horizontal)
        {
            m_rowCount = m_constraintCount;
            if (m_rowCount == 0)
                m_rowCount = Mathf.FloorToInt((m_initialSize.y + m_itemSpace.y) / (m_itemSize.y + m_itemSpace.y));
            if (m_rowCount == 0)
                m_rowCount = 1;
        }
        else
        {
            m_columnCount = m_constraintCount;
            if (m_columnCount == 0)
                m_columnCount = Mathf.FloorToInt((m_initialSize.x + m_itemSpace.x) / (m_itemSize.x + m_itemSpace.x));
            if (m_columnCount == 0)
                m_columnCount = 1;
        }
    }
    
    void OnValueChanged(ListViewItem item)
    {
        if (item.isSelected)
        {
            if (m_selectType == ESelectType.Single)
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
            if (m_selectType == ESelectType.Single)
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
        
        m_itemList.Clear();
        m_selectedItemList.Clear();
        m_itemInfoList.Clear();
    }
}
