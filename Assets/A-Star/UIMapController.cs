using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//两个Int组成的结构图，用作网格下标
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

    //为什么要重写GetHashCode方法，阅读： https://www.cnblogs.com/xiaochen-vip8/p/5506478.html
    public override int GetHashCode() {
        return x ^ (y * 256);
    }

    public override bool Equals(object obj) {
        if(obj.GetType() != typeof(Int2))
            return false;
        Int2 int2 = (Int2)obj;
        return x == int2.x && y == int2.y;
    }

    public static bool operator ==(Int2 a, Int2 b) {
        return a.Equals(b);
    }

    public static bool operator !=(Int2 a, Int2 b) {
        return !a.Equals(b);
    }
}

public class UIMapController : MonoBehaviour
{
    public UIGridController gridPrefab;
    public int gridSize;
    public Transform gridParent;
    public Button setPlayerButton;
    public Button setDestinationButton;
    public Button setObstacleButton;
    public Button resetMapButton;
    public Button aStarButton;
    public Text hintText;
    public bool isShowGridHint;
    public bool isStepOneByOne;

    UIGridController[,] m_map;
    Int2 m_mapSize;
    GridState m_settingState;
    Text SetObstacleButtonText;
    UIGridController m_player, m_destination;//玩家与目的地
    Dictionary<Int2, UIGridController> m_obstacleDic = new Dictionary<Int2, UIGridController>();//障碍物

    AStar m_aStar;
    IEnumerator m_aStarProcess;

    void Start()
    {
        InitMap();
        setPlayerButton.onClick.AddListener(OnSetPlayerButtonClicked);
        setDestinationButton.onClick.AddListener(OnSetDestinationButtonClicked);
        setObstacleButton.onClick.AddListener(OnSetObstacleButtonClicked);
        resetMapButton.onClick.AddListener(OnResetMapButtonClicked);
        aStarButton.onClick.AddListener(OnAStarButtonClicked);

        m_settingState = GridState.Default;
        m_aStar = new AStar();
    }

    //初始化网格
    void InitMap() {
        if(m_map != null)
            return;

        Int2 offset = new Int2(50 + gridSize / 2, 50 + gridSize / 2);
        m_mapSize = new Int2((Screen.width - 100) / gridSize, (Screen.height - 200) / gridSize);
        m_map = new UIGridController[m_mapSize.x, m_mapSize.y];

        for(int i = 0; i < m_mapSize.x; i++) {
            for(int j = 0; j < m_mapSize.y; j++) {
                UIGridController grid = Instantiate(gridPrefab, gridParent);
                grid.rectTransform.sizeDelta = new Vector2(gridSize, gridSize);
                grid.rectTransform.anchoredPosition = new Vector2(gridSize * i + offset.x, gridSize * j + offset.y);
                grid.rectTransform.localScale = Vector3.one;
                grid.gameObject.SetActive(true);
                grid.Init(new Int2(i, j), isShowGridHint, OnGridClicked);
                m_map[i, j] = grid;
            }
        }
    }

    //单击某个网格，设置对应状态
    void OnGridClicked(UIGridController grid) {
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
                m_obstacleDic[grid.position] = grid;
            }
            else if(grid.state == GridState.Obstacle) {
                grid.state = GridState.Default;
                m_obstacleDic.Remove(grid.position);
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
            SetObstacleButtonText = setObstacleButton.GetComponentInChildren<Text>();

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

    void OnResetMapButtonClicked() {
        if(m_settingState == GridState.Obstacle)
            return;
        ResetMap();
    }

    //开始寻路，分一步一步寻路和一次性完成寻路
    void OnAStarButtonClicked() {
        if(!m_aStar.isInit) {
            m_aStar.Init(m_map, m_mapSize, m_player.position, m_destination.position);
            m_aStarProcess = m_aStar.Start();
        }
        if(isStepOneByOne) {
            if(!m_aStarProcess.MoveNext()) {
                SetHint("寻路完成");
            }
        }
        else {
            while(m_aStarProcess.MoveNext())
                ;
            SetHint("寻路完成");
        }
    }

    //右上角提示语
    void SetHint(string hint = null) {
        hintText.text = hint;
    }

    void ResetMap() {
        //先只清空寻路
        if(m_aStar.isInit) {
            m_aStar.Clear();
            return;
        }

        //再次点击清空地图设置
        if(m_player != null)
            m_player.state = GridState.Default;
        m_player = null;

        if(m_destination != null)
            m_destination.state = GridState.Default;
        m_destination = null;

        foreach(var grid in m_obstacleDic.Values)
            grid.state = GridState.Default;
        m_obstacleDic.Clear();
        
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
