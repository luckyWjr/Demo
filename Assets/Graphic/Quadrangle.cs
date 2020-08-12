using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Quadrangle : Graphic
{
    public Vector2 positionLeftTop;
    public Vector2 positionRightTop;
    public Vector2 positionRightBottom;
    public Vector2 positionLeftBottom;
    
    [SerializeField] Sprite m_image;
    
    public override Texture mainTexture => m_image == null ? s_WhiteTexture : m_image.texture;
    
    UIVertex[] m_vertexes = new UIVertex[4];
    Vector2[] m_uvs = new Vector2[4];

    protected override void Start()
    {
        m_uvs[0] = new Vector2(0, 1);
        m_uvs[1] = new Vector2(1, 1);
        m_uvs[2] = new Vector2(1, 0);
        m_uvs[3] = new Vector2(0, 0);
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        for (int i = 0; i < 4; i++)
        {
            m_vertexes[i].color = color;
            m_vertexes[i].uv0 = m_uvs[i];
        }

        m_vertexes[0].position = positionLeftTop;
        m_vertexes[1].position = positionRightTop;
        m_vertexes[2].position = positionRightBottom;
        m_vertexes[3].position = positionLeftBottom;
        
        vh.AddVert(m_vertexes[0]);
        vh.AddVert(m_vertexes[1]);
        vh.AddVert(m_vertexes[2]);
        vh.AddVert(m_vertexes[3]);
        //参数即三角面的顶点绘制顺序
        vh.AddTriangle(0, 2, 1);
        vh.AddTriangle(0, 3, 2);
    }
}