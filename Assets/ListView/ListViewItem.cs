using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class ListViewItem : MonoBehaviour
{
    [SerializeField] GameObject m_selectedGameObject;
    
    public int id { get; private set; }
    public ListView.ESelectType selectType { get; private set; }
    Action<ListViewItem> m_onValueChanged;
    Action<ListViewItem> m_onClicked;//适用于只在Item被单击时做操作的情况

    RectTransform m_rectTransform;

    Button m_button;

    bool m_isSelected;
    public bool isSelected
    {
        get => m_isSelected;
        set
        {
            if (m_isSelected != value)
            {
                m_isSelected = value;
                m_onValueChanged?.Invoke(this);
                UpdateSelectedUI();
            }
        }
    }

    void Awake()
    {
        id = GetInstanceID();
        m_button = GetComponent<Button>();
        isSelected = false;
        m_button.onClick.AddListener(OnClicked);

        m_rectTransform = GetComponent<RectTransform>();
        m_rectTransform.anchorMin = Vector2.up;
        m_rectTransform.anchorMax = Vector2.up;
    }

    public void Init(ListView.ESelectType type, Action<ListViewItem> onValueChanged, Action<ListViewItem> onClicked)
    {
        selectType = type;
        m_onValueChanged = onValueChanged;
        m_onClicked = onClicked;
    }
    
    void OnClicked()
    {
        if (selectType == ListView.ESelectType.Single)
            isSelected = true;
        else
            isSelected = !isSelected;
        m_onClicked?.Invoke(this);
    }

    void ClearSelected()
    {
        isSelected = false;
    }

    protected virtual void UpdateSelectedUI()
    {
        if (m_selectedGameObject != null)
        {
            m_selectedGameObject.SetActive(m_isSelected);
        }
    }
}
