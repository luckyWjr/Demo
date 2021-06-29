using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct Int2
{
    public int x;
    public int y;

    public Int2(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public override string ToString() {
        return $"x:{x.ToString()}   y:{y.ToString()}";
    }
}

public enum GridState
{
    Default,
    Player,
    Obstacle,
    Destination,
    Path,
    Calculate
}

public class Grid
{
    public static Color[] gridColors = new Color[6] { Color.white , Color.green, Color.gray, Color.red, Color.yellow, Color.blue };
    
    public Image image;
    
    GridState m_state;
    public GridState state {
        get => m_state;
        set {
            m_state = value;
            image.color = gridColors[(int)m_state];
        }
    }

    Int2 m_position;
    Action<Grid> onClickCallback;

    public Grid(Image image, Int2 pos, Action<Grid> callback = null) {
        this.image = image;
        m_position = pos;
        onClickCallback = callback;
        state = GridState.Default;
        this.image.GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    void OnClicked() {
        onClickCallback?.Invoke(this);
    }

    public void Clear() {
        GameObject.Destroy(image);
        image = null;
    }
}

public class UIMapController : MonoBehaviour
{
    public Image gridPrefab;
    public int gridSize;
    public Transform gridParent;
    public Button SetPlayerButton;
    public Button SetDestinationButton;
    public Button SetObstacleButton;
    public Button ClearMapButton;
    public Text HintText;

    Grid[,] m_map;
    Int2 m_mapSize;
    GridState m_settingState;
    Text SetObstacleButtonText;
    Grid m_player, m_destination;
    List<Grid> m_obstacleList = new List<Grid>();

    void Start()
    {
        InitMap();
        SetPlayerButton.onClick.AddListener(OnSetPlayerButtonClicked);
        SetDestinationButton.onClick.AddListener(OnSetDestinationButtonClicked);
        SetObstacleButton.onClick.AddListener(OnSetObstacleButtonClicked);
        ClearMapButton.onClick.AddListener(OnClearMapButtonClicked);

        m_settingState = GridState.Default;
    }

    void InitMap() {
        if(m_map != null)
            return;

        Int2 offset = new Int2(50 + gridSize / 2, 50 + gridSize / 2);
        m_mapSize = new Int2((Screen.width - 100) / gridSize, (Screen.height - 200) / gridSize);
        m_map = new Grid[m_mapSize.x, m_mapSize.y];

        for(int i = 0; i < m_mapSize.x; i++) {
            for(int j = 0; j < m_mapSize.y; j++) {
                Image image = Instantiate(gridPrefab, gridParent);
                image.rectTransform.anchoredPosition = new Vector2(gridSize * i + offset.x, gridSize * j + offset.y);
                image.rectTransform.localScale = Vector3.one;
                image.gameObject.SetActive(true);
                m_map[i, j] = new Grid(image, new Int2(i, j), OnGridClicked);
            }
        }
    }

    void OnGridClicked(Grid grid) {
        if(m_settingState == GridState.Player) {
            grid.state = GridState.Player;
            if(m_player != null)
                m_player.state = GridState.Default;
            m_player = grid;
            SetHint();
            m_settingState = GridState.Default;
        }
        else if(m_settingState == GridState.Destination) {
            grid.state = GridState.Destination;
            if(m_destination != null)
                m_destination.state = GridState.Default;
            m_destination = grid;
            SetHint();
            m_settingState = GridState.Default;
        }
        else if(m_settingState == GridState.Obstacle) {
            if(grid.state == GridState.Default) {
                grid.state = GridState.Obstacle;
                m_obstacleList.Add(grid);
            }
            else if(grid.state == GridState.Obstacle) {
                grid.state = GridState.Default;
                m_obstacleList.Remove(grid);
            }
        }
    }

    void OnSetPlayerButtonClicked() {
        if(m_settingState == GridState.Obstacle)
            return;
        m_settingState = GridState.Player;
        SetHint("请设置玩家位置");
    }

    void OnSetDestinationButtonClicked() {
        if(m_settingState == GridState.Obstacle)
            return;
        m_settingState = GridState.Destination;
        SetHint("请设置目的地位置");
    }

    void OnSetObstacleButtonClicked() {
        if(SetObstacleButtonText == null)
            SetObstacleButtonText = SetObstacleButton.GetComponentInChildren<Text>();

        if(m_settingState == GridState.Obstacle) {
            m_settingState = GridState.Default;
            SetObstacleButtonText.text = "设置障碍";
            SetHint();
        } else {
            m_settingState = GridState.Obstacle;
            SetObstacleButtonText.text = "完成障碍设置";
            SetHint("请设置障碍物");
        }
    }

    void OnClearMapButtonClicked() {
        if(m_settingState == GridState.Obstacle)
            return;
        ResetMap();
    }

    void SetHint(string hint = null) {
        HintText.text = hint;
    }

    void ResetMap() {
        if(m_player != null)
            m_player.state = GridState.Default;
        m_player = null;

        if(m_destination != null)
            m_destination.state = GridState.Default;
        m_destination = null;

        foreach(var grid in m_obstacleList)
            grid.state = GridState.Default;
        m_obstacleList.Clear();
    }

    void ClearMap() {
        if(m_map == null)
            return;
        for(int i = 0; i < m_mapSize.x; i++) {
            for(int j = 0; j < m_mapSize.y; j++) {
                m_map[i, j].Clear();
                m_map[i, j] = null;
            }
        }
        m_map = null;
    }

    void OnDestroy() {
        ClearMap();
    }
}
