using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EvaluationFunctionType {
    Euclidean,
    Manhattan,
    Diagonal,
}

public class Node
{
    Int2 m_position;//�±�
    public Int2 position => m_position;
    public Node parent;//��һ��node
    
    //��ɫ���ýڵ��ʵ�ʾ���
    int m_g;
    public int g {
        get => m_g;
        set {
            m_g = value;
            m_f = m_g + m_h;
        }
    }

    //�ýڵ㵽Ŀ�ĵصĹ��۾���
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
    static int FACTOR = 10;//ˮƽ��ֱ���ڸ��ӵľ���
    static int FACTOR_DIAGONAL = 14;//�Խ������ڸ��ӵľ���

    bool m_isInit = false;
    public bool isInit => m_isInit;

    UIGridController[,] m_map;//��ͼ����
    Int2 m_mapSize;
    Int2 m_player, m_destination;//��ʼ��ͽ���������
    EvaluationFunctionType m_evaluationFunctionType;//���۷�ʽ

    Dictionary<Int2, Node> m_openDic = new Dictionary<Int2, Node>();//׼�����������
    Dictionary<Int2, Node> m_closeDic = new Dictionary<Int2, Node>();//��ɴ��������

    Node m_destinationNode;

    public void Init(UIGridController[,] map, Int2 mapSize, Int2 player, Int2 destination, EvaluationFunctionType type = EvaluationFunctionType.Diagonal) {
        m_map = map;
        m_mapSize = mapSize;
        m_player = player;
        m_destination = destination;
        m_evaluationFunctionType = type;

        m_openDic.Clear();
        m_closeDic.Clear();

        m_destinationNode = null;

        //����ʼ�����open��
        AddNodeInOpenQueue(new Node(m_player, null, 0, 0));
        m_isInit = true;
    }

    //����Ѱ·
    public IEnumerator Start() {
        while(m_openDic.Count > 0 && m_destinationNode == null) {
            //�Ȱ���f��ֵ�������У���fֵ���ʱ�ٰ���h��ֵ��������
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ThenBy(kv => kv.Value.h).ToDictionary(p => p.Key, o => o.Value);
            //��ȡ�����ĵ�һ���ڵ�
            Node node = m_openDic.First().Value;
            //��Ϊʹ�õĲ���Queue�����Ҫ��open���ֶ�ɾ���ýڵ�
            m_openDic.Remove(node.position);
            //����ýڵ����ڵĽڵ�
            OperateNeighborNode(node);
            //������󽫸ýڵ����close��
            AddNodeInCloseDic(node);
            yield return null;
        }
        if(m_destinationNode == null)
            Debug.LogError("�Ҳ�������·��");
        else
            ShowPath(m_destinationNode);
    }

    //�������ڵĽڵ�
    void OperateNeighborNode(Node node) {
        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(i == 0 && j == 0)
                    continue;
                Int2 pos = new Int2(node.position.x + i, node.position.y + j);
                //������ͼ��Χ
                if(pos.x < 0 || pos.x >= m_mapSize.x || pos.y < 0 || pos.y >= m_mapSize.y)
                    continue;
                //�Ѿ�������Ľڵ�
                if(m_closeDic.ContainsKey(pos))
                    continue;
                //�ϰ���ڵ�
                if(m_map[pos.x, pos.y].state == GridState.Obstacle)
                    continue;
                //�����ڽڵ����open��
                if(i == 0 || j == 0)
                    AddNeighborNodeInQueue(node, pos, FACTOR);
                else
                    AddNeighborNodeInQueue(node, pos, FACTOR_DIAGONAL);
            }
        }
    }

    //���ڵ���뵽open��
    void AddNeighborNodeInQueue(Node parentNode, Int2 position, int g) {
        //��ǰ�ڵ��ʵ�ʾ���g�����ϸ��ڵ��ʵ�ʾ�������Լ����ϸ��ڵ��ʵ�ʾ���
        int nodeG = parentNode.g + g;
        //�����λ�õĽڵ��Ѿ���open��
        if(m_openDic.ContainsKey(position)) {
            //�Ƚ�ʵ�ʾ���g��ֵ���ø�С��ֵ�滻
            if(nodeG < m_openDic[position].g) {
                m_openDic[position].g = nodeG;
                m_openDic[position].parent = parentNode;
                ShowOrUpdateAStarHint(m_openDic[position]);
            }
        }
        else {
            //�����µĽڵ㲢���뵽open��
            Node node = new Node(position, parentNode, nodeG, GetH(position));
            //����ܱ���һ�����յ㣬��ô˵���Ѿ��ҵ��ˡ�
            if(position == m_destination)
                m_destinationNode = node;
            else
                AddNodeInOpenQueue(node);
        }
    }

    //����open�У�����������״̬
    void AddNodeInOpenQueue(Node node) {
        m_openDic[node.position] = node;
        ShowOrUpdateAStarHint(node);
    }

    void ShowOrUpdateAStarHint(Node node) {
        m_map[node.position.x, node.position.y].ShowOrUpdateAStarHint(node.g, node.h, node.f,
            node.parent == null ? Vector2.zero : new Vector2(node.parent.position.x - node.position.x, node.parent.position.y - node.position.y));
    }

    //����close�У�����������״̬
    void AddNodeInCloseDic(Node node) {
        m_closeDic.Add(node.position, node);
        m_map[node.position.x, node.position.y].ChangeInOpenStateToInClose();
    }

    //Ѱ·��ɣ���ʾ·��
    void ShowPath(Node node) {
        while(node != null) {
            m_map[node.position.x, node.position.y].ChangeToPathState();
            node = node.parent;
        }
    }

    //��ȡ���۾���
    int GetH(Int2 position) {
        if(m_evaluationFunctionType == EvaluationFunctionType.Manhattan)
            return GetManhattanDistance(position);
        else if(m_evaluationFunctionType == EvaluationFunctionType.Diagonal)
            return GetDiagonalDistance(position);
        else
            return Mathf.CeilToInt(GetEuclideanDistance(position));
    }

    //��ȡ�����پ���
    int GetDiagonalDistance(Int2 position) {
        int x = Mathf.Abs(m_destination.x - position.x);
        int y = Mathf.Abs(m_destination.y - position.y);
        int min = Mathf.Min(x, y);
        return min * FACTOR_DIAGONAL + Mathf.Abs(x - y) * FACTOR;
    }

    //��ȡ�Խ��߾���
    int GetManhattanDistance(Int2 position) {
        return Mathf.Abs(m_destination.x - position.x) * FACTOR + Mathf.Abs(m_destination.y - position.y) * FACTOR;
    }

    //��ȡŷ����þ���,����������������
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

        m_destinationNode = null;

        m_isInit = false;
    }
}
