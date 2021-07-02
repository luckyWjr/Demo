using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GridState {
    Default,
    Player,
    Obstacle,
    Destination,
    Path,
    InOpen,
    InClose
}

public class UIGridController : MonoBehaviour {
    //不同颜色对应不同的网格状态，便于区分
    public static Color[] GRID_COLORS = new Color[7] { Color.white, Color.green, Color.gray, Color.red, Color.yellow, new Color(0, 0.5f, 1), new Color(0, 1, 1) };

    public Image image;
    public Button button;

    //用于显示g，f，h和parent的值，方便理解算法
    [Header("A-Star")]
    public Text gText;
    public Text hText;
    public Text fText;
    public GameObject Arrow;

    RectTransform m_rectTransform;
    public RectTransform rectTransform {
        get {
            if(m_rectTransform == null)
                m_rectTransform = GetComponent<RectTransform>();
            return m_rectTransform;
        }
    }

    GridState m_state;
    public GridState state {
        get => m_state;
        set {
            m_state = value;
            image.color = GRID_COLORS[(int)m_state];
        }
    }

    Int2 m_position;
    public Int2 position => m_position;

    Action<UIGridController> onClickCallback;
    bool m_isCanShowHint;

    public void Init(Int2 pos, bool isShowHint, Action<UIGridController> callback = null) {
        m_position = pos;
        m_isCanShowHint = isShowHint;
        onClickCallback = callback;
        state = GridState.Default;
        button.onClick.AddListener(OnClicked);
    }

    //当网格被加入Open队列或者值有变动时，更新
    public void ShowOrUpdateAStarHint(int g, int h, int f, Vector2 forward) {
        if(state == GridState.Default || state == GridState.InOpen) {
            state = GridState.InOpen;
            if(m_isCanShowHint) {
                gText.text = $"G:\n{g.ToString()}";
                hText.text = $"H:\n{h.ToString()}";
                fText.text = $"F:\n{f.ToString()}";
                Arrow.SetActive(true);
                Arrow.transform.up = -forward;
            }
        }
    }

    //当网格加入Close队列
    public void ChangeInOpenStateToInClose() {
        if(state == GridState.InOpen)
            state = GridState.InClose;
    }

    //将网格标记为寻路完成后路径上的网格
    public void ChangeToPathState() {
        if(state == GridState.InOpen || state == GridState.InClose)
            state = GridState.Path;
    }

    public void ClearAStarHint() {
        gText.text = "";
        hText.text = "";
        fText.text = "";
        if(state == GridState.InOpen || state == GridState.InClose || state == GridState.Path)
            state = GridState.Default;
        Arrow.SetActive(false);
    }

    void OnClicked() {
        onClickCallback?.Invoke(this);
    }

    public void Clear() {
        Destroy(gameObject);
    }
}
