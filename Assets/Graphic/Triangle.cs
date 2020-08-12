using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Triangle : Graphic
{
    public Vector2 positionA;
    public Vector2 positionB;
    public Vector2 positionC;
    
    [SerializeField] Sprite m_image;
    
    public override Texture mainTexture => m_image == null ? s_WhiteTexture : m_image.texture;
    
    UIVertex[] m_vertexes = new UIVertex[3];
    Vector2[] m_uvs = new Vector2[3];

    protected override void Start()
    {
        m_uvs[0] = new Vector2(0, 0);
        m_uvs[1] = new Vector2(1, 1);
        m_uvs[2] = new Vector2(1, 0);
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        for (int i = 0; i < 3; i++)
        {
            m_vertexes[i].color = color;
            m_vertexes[i].uv0 = m_uvs[i];
        }

        m_vertexes[0].position = positionA;
        m_vertexes[1].position = positionB;
        m_vertexes[2].position = positionC;
        

        vh.AddVert(m_vertexes[0]);
        vh.AddVert(m_vertexes[1]);
        vh.AddVert(m_vertexes[2]);
        vh.AddTriangle(0, 1, 2);
    }
}
