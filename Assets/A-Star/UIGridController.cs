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
    public static Color[] gridColors = new Color[7] { Color.white, Color.green, Color.gray, Color.red, Color.yellow, new Color(0, 0.5f, 1), new Color(0, 1, 1) };

    public Image image;
    public Button button;

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
            image.color = gridColors[(int)m_state];
        }
    }

    Int2 m_position;
    public Int2 position => m_position;

    Action<UIGridController> onClickCallback;

    public void Init(Int2 pos, Action<UIGridController> callback = null) {
        m_position = pos;
        onClickCallback = callback;
        state = GridState.Default;
        button.onClick.AddListener(OnClicked);
    }

    public void ShowAStarHint(int g, int h, int f, Vector2 forward) {
        if(state == GridState.Default) {
            state = GridState.InOpen;
            gText.text = $"G:\n{g.ToString()}";
            hText.text = $"H:\n{h.ToString()}";
            fText.text = $"F:\n{f.ToString()}";
            Arrow.SetActive(true);
            Arrow.transform.up = -forward;
        }
    }

    public void ChangeInOpenStateToInClose() {
        if(state == GridState.InOpen)
            state = GridState.InClose;
    }

    public void ClearAStarHint() {
        gText.text = "";
        hText.text = "";
        fText.text = "";
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
