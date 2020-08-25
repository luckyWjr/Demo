using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public abstract class ListViewItem : MonoBehaviour
{
    public int id { get; private set; }
    Action<ListViewItem, bool> m_onValueChanged;

    Toggle m_toggle;
    public Toggle toggle
    {
        get
        {
            if (m_toggle == null)
                m_toggle = GetComponent<Toggle>();
            return m_toggle;
        }
    }

    public bool isOn
    {
        set => toggle.isOn = value;
        get => toggle.isOn;
    }

    void Awake()
    {
        id = GetInstanceID();
        toggle.onValueChanged.AddListener(OnValueChanged);
    }
    
    void OnValueChanged(bool isOn)
    {
        m_onValueChanged?.Invoke(this, isOn);
    }

    public void AddValueChangedHandle(Action<ListViewItem, bool> handle)
    {
        m_onValueChanged += handle;
    }
    
    public void RemoveValueChangedHandle(Action<ListViewItem, bool> handle)
    {
        m_onValueChanged -= handle;
    }
    
    public void ClearValueChangedHandle()
    {
        m_onValueChanged = null;
    }
}
