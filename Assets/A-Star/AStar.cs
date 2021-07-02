using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EvaluationFunctionType {
    Euclidean,
    Manhattan,
}

public class Node
{
    Int2 m_position;//下标
    public Int2 position => m_position;
    public Node parent;//上一个node
    
    //角色到该节点的实际距离
    int m_g;
    public int g {
        get => m_g;
        set {
            m_g = value;
            m_f = m_g + m_h;
        }
    }

    //该节点到目的地的估价距离
    int m_h;
    public int h {
        get => m_h;
        set {
            m_h = value;
            m_f = m_g + m_h;
        }
    }

    int m_f;
    public int f => m_f;

    public Node(Int2 pos, Node parent, int g, int h) {
        m_position = pos;
        this.parent = parent;
        m_g = g;
        m_h = h;
        m_f = m_g + m_h;
    }
}

public class AStar {
    static int FACTOR = 10;//水平竖直相邻格子的距离
    static int FACTOR_DIAGONAL = 14;//对角线相邻格子的距离

    bool m_isInit = false;
    public bool isInit => m_isInit;

    UIGridController[,] m_map;
    Int2 m_mapSize;
    Int2 m_player, m_destination;
    EvaluationFunctionType m_evaluationFunctionType;//估价方式

    Dictionary<Int2, Node> m_openDic = new Dictionary<Int2, Node>();//准备处理的网格
    Dictionary<Int2, Node> m_closeDic = new Dictionary<Int2, Node>();//完成处理的网格

    public void Init(UIGridController[,] map, Int2 mapSize, Int2 player, Int2 destination, EvaluationFunctionType type = EvaluationFunctionType.Manhattan) {
        m_map = map;
        m_mapSize = mapSize;
        m_player = player;
        m_destination = destination;
        m_evaluationFunctionType = type;

        m_openDic.Clear();
        m_closeDic.Clear();

        //将起始点加入open中
        AddNodeInOpenQueue(new Node(m_player, null, 0, 0));
        m_isInit = true;
    }

    //计算寻路
    public IEnumerator Start() {
        while(m_openDic.Count > 0) {
            //按照f的值升序排列
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ToDictionary(p => p.Key, o => o.Value);
            //提取排序后的第一个节点
            Node node = m_openDic.First().Value;
            //因为使用的不是Queue，因此要从open中手动删除该节点
            m_openDic.Remove(node.position);
            //如果该节点是目的地点，说明寻路成功
            if(node.position == m_destination) {
                ShowPath(node);
                yield break;
            }
            //处理该节点相邻的节点
            OperateNeighborNode(node);
            //处理完后将该节点加入close中
            AddNodeInCloseDic(node);
            yield return null;
        }
    }

    //处理相邻的节点
    void OperateNeighborNode(Node node) {
        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(i == 0 && j == 0)
                    continue;
                Int2 pos = new Int2(node.position.x + i, node.position.y + j);
                //超出地图范围
                if(pos.x < 0 || pos.x >= m_mapSize.x || pos.y < 0 || pos.y >= m_mapSize.y)
                    continue;
                //已经处理过的节点
                if(m_closeDic.ContainsKey(pos))
                    continue;
                //障碍物节点
                if(m_map[pos.x, pos.y].state == GridState.Obstacle)
                    continue;
                //将相邻节点加入open中
                if(i == 0 || j == 0)
                    AddNeighborNodeInQueue(node, pos, FACTOR);
                else
                    AddNeighborNodeInQueue(node, pos, FACTOR_DIAGONAL);
            }
        }
    }

    //将节点加入到open中
    void AddNeighborNodeInQueue(Node parentNode, Int2 position, int g) {
        //当前节点的实际距离g等于上个节点的实际距离加上自己到上个节点的实际距离
        int nodeG = parentNode.g + g;
        //如果该位置的节点已经在open中
        if(m_openDic.ContainsKey(position)) {
            //比较实际距离g的值，用更小的值替换
            if(nodeG < m_openDic[position].g) {
                m_openDic[position].g = nodeG;
                m_openDic[position].parent = parentNode;
                ShowOrUpdateAStarHint(m_openDic[position]);
            }
        }
        else {
            //生成新的节点并加入到open中
            Node node = new Node(position, parentNode, nodeG, GetH(position));
            AddNodeInOpenQueue(node);
        }
    }

    //加入open中，并更新网格状态
    void AddNodeInOpenQueue(Node node) {
        m_openDic[node.position] = node;
        ShowOrUpdateAStarHint(node);
    }

    void ShowOrUpdateAStarHint(Node node) {
        m_map[node.position.x, node.position.y].ShowOrUpdateAStarHint(node.g, node.h, node.f,
            node.parent == null ? Vector2.zero : new Vector2(node.parent.position.x - node.position.x, node.parent.position.y - node.position.y));
    }

    //加入close中，并更新网格状态
    void AddNodeInCloseDic(Node node) {
        m_closeDic.Add(node.position, node);
        m_map[node.position.x, node.position.y].ChangeInOpenStateToInClose();
    }

    //寻路完成，显示路径
    void ShowPath(Node node) {
        while(node != null) {
            m_map[node.position.x, node.position.y].ChangeToPathState();
            node = node.parent;
        }
    }

    //获取估价距离
    int GetH(Int2 position) {
        //if(m_evaluationFunctionType == EvaluationFunctionType.Manhattan)
            return GetManhattanDistance(position);
        //else
        //    return GetEuclideanDistance(position);
    }

    //获取曼哈顿距离
    int GetManhattanDistance(Int2 position) {
        return Mathf.Abs(m_destination.x - position.x) * FACTOR + Mathf.Abs(m_destination.y - position.y) * FACTOR;
    }

    //获取欧几里得距离,测试下来并不合适
    float GetEuclideanDistance(Int2 position) {
        return Mathf.Sqrt(Mathf.Pow((m_destination.x - position.x) * FACTOR, 2) + Mathf.Pow((m_destination.y - position.y) * FACTOR, 2));
    }

    public void Clear() {
        foreach(var pos in m_openDic.Keys) {
            m_map[pos.x, pos.y].ClearAStarHint();
        }
        m_openDic.Clear();

        foreach(var pos in m_closeDic.Keys) {
            m_map[pos.x, pos.y].ClearAStarHint();
        }
        m_closeDic.Clear();

        m_isInit = false;
    }
}
