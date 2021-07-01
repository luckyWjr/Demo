using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Node
{
    Int2 m_position;
    public Int2 position => m_position;
    public Node parent;
    int m_g;
    public int g {
        get => m_g;
        set {
            m_g = value;
            m_f = m_g + m_h;
        }
    }
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
    static int FACTOR = 10;
    static int FACTOR_DIAGONAL = 14;

    bool m_isInit = false;
    public bool isInit => m_isInit;

    UIGridController[,] m_map;
    Int2 m_mapSize;
    Int2 m_player, m_destination;

    Dictionary<Int2, Node> m_openDic = new Dictionary<Int2, Node>();
    Dictionary<Int2, Node> m_closeDic = new Dictionary<Int2, Node>();

    public void Init(UIGridController[,] map, Int2 mapSize, Int2 player, Int2 destination) {
        m_map = map;
        m_mapSize = mapSize;
        m_player = player;
        m_destination = destination;

        m_openDic.Clear();
        m_closeDic.Clear();

        AddNodeInOpenQueue(new Node(m_player, null, 0, 0));
        m_isInit = true;
    }

    public IEnumerator Start() {
        while(m_openDic.Count > 0) {
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ToDictionary(p => p.Key, o => o.Value);
            Node node = m_openDic.First().Value;
            m_openDic.Remove(node.position);
            Debug.Log("m_openQueue Dequeue:"+ node.f);
            if(node.position == m_destination)
                yield break;
            AddNodeInCloseDic(node);
            OperateNeighborNode(node);
            yield return null;
        }
    }

    void OperateNeighborNode(Node node) {
        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(i == 0 && j == 0)
                    continue;
                Int2 pos = new Int2(node.position.x + i, node.position.y + j);
                if(pos.x < 0 || pos.x >= m_mapSize.x || pos.y < 0 || pos.y >= m_mapSize.y)
                    continue;
                if(m_closeDic.ContainsKey(pos))
                    continue;
                if(m_map[pos.x, pos.y].state == GridState.Obstacle)
                    continue;
                if(i == 0 || j == 0)
                    AddNeighborNodeInQueue(node, pos, FACTOR);
                else
                    AddNeighborNodeInQueue(node, pos, FACTOR_DIAGONAL);
            }
        }
    }

    void AddNeighborNodeInQueue(Node parentNode, Int2 position, int g) {
        int nodeG = parentNode.g + g;
        if(m_openDic.ContainsKey(position)) {
            if(nodeG < m_openDic[position].g) {
                m_openDic[position].g = nodeG;
                m_openDic[position].parent = parentNode;
            }
        }
        else {
            Node node = new Node(position, parentNode, nodeG, GetH(position));
            AddNodeInOpenQueue(node);
        }
    }

    void AddNodeInOpenQueue(Node node) {
        m_openDic[node.position] = node;
        m_map[node.position.x, node.position.y].ShowAStarHint(node.g, node.h, node.f,
            node.parent == null ? Vector2.zero : new Vector2(node.parent.position.x - node.position.x, node.parent.position.y - node.position.y));
    }

    void AddNodeInCloseDic(Node node) {
        m_closeDic.Add(node.position, node);
        m_map[node.position.x, node.position.y].ChangeInOpenStateToInClose();
    }

    int GetH(Int2 position) {
        return GetManhattanDistance(position);
    }

    int GetManhattanDistance(Int2 position) {
        return Mathf.Abs(m_destination.x - position.x) * FACTOR + Mathf.Abs(m_destination.y - position.y) * FACTOR;
    }

    public void Clear() {
        m_isInit = false;
    }
}
