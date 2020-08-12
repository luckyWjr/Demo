using UnityEngine;
using UnityEngine.UI;
 
[ExecuteInEditMode]
// Changed to maskableGraphic so it can be masked with RectMask2D
public class Annulus : MaskableGraphic
{
    public enum ShapeType
    {
        Annulus,//圆环
        Circle,//圆
    }
    
    [SerializeField] Sprite m_image;

    public ShapeType shapeType;
    
    public float innerRadius = 10;//圆环内径，为0即是圆
    public float outerRadius = 20;//圆环外径
    
    [Range(0, 1)] [SerializeField] float m_fillAmount;//填充值
    
    [Range(0, 720)] public int segments = 360;//片数，越大锯齿越不明显

    [SerializeField] Image.Origin360 m_originType;//填充的起点

    public bool m_isClockwise = true;
 
    public override Texture mainTexture => m_image == null ? s_WhiteTexture : m_image.texture;

    float m_originRadian = -1;//根据m_originType设置相关弧度（-1表示还没设置过对应的值）
    
    public float fillAmount
    {
        get => m_fillAmount;
        set
        {
            m_fillAmount = value;
            SetVerticesDirty();
        }
    }

    public Sprite image
    {
        get => m_image;
        set
        {
            if (m_image == value)
                return;
            m_image = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }
    
    public Image.Origin360 originType
    {
        get => m_originType;
        set
        {
            if (m_originType == value)
                return;
            m_originType = value;
            SetOriginRadian();
            SetVerticesDirty();
        }
    }

    public bool isClockwise
    {
        get => m_isClockwise;
        set
        {
            if (m_isClockwise != value)
            {
                m_isClockwise = value;
                SetVerticesDirty();
            }
        }
    }
 
    UIVertex[] m_vertexes = new UIVertex[4];
    Vector2[] m_uvs = new Vector2[4];
    Vector2[] m_positions = new Vector2[4];
 
    protected override void Start()
    {
        if (m_originRadian == -1)
            SetOriginRadian();
        
        m_uvs[0] = new Vector2(0, 1);
        m_uvs[1] = new Vector2(1, 1);
        m_uvs[2] = new Vector2(1, 0);
        m_uvs[3] = new Vector2(0, 0);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        //m_fillAmount == 0，什么也不绘制
        if (m_fillAmount == 0) return;
        
#if UNITY_EDITOR
        SetOriginRadian();
#endif

        //每个面片的角度
        float degrees = 360f / segments;
        //需要绘制的面片数量
        int count = (int)(segments * m_fillAmount);
        
        float cos = Mathf.Cos(m_originRadian);
        float sin = Mathf.Sin(m_originRadian);
 
        //计算外环起点，例如m_originRadian = 0，x = -outerRadius，y = 0，所以起点是Left（九点钟方向）
        float x = -outerRadius * cos;
        float y = outerRadius * sin;
        Vector2 originOuter = new Vector2(x, y);
 
        //计算内环起点
        x = -innerRadius * cos;
        y = innerRadius * sin;
        Vector2 originInner = new Vector2(x, y);
 
        for (int i = 1; i <= count; i++)
        {
            //m_positions[0] 当前面片的外环起点
            m_positions[0] = originOuter;
            
            //当前面片的弧度 + 起始弧度 = 终止弧度
            float endRadian = i * degrees * Mathf.Deg2Rad * (isClockwise ? 1 : -1) + m_originRadian;
            cos = Mathf.Cos(endRadian);
            sin = Mathf.Sin(endRadian);
            
            //m_positions[1] 当前面片的外环终点
            m_positions[1] = new Vector2(-outerRadius * cos, outerRadius * sin);
 
            //m_positions[2] 当前面片的内环终点
            //m_positions[3] 当前面片的内环起点
            if (shapeType == ShapeType.Annulus)
            {
                m_positions[2] = new Vector2(-innerRadius * cos, innerRadius * sin);
                m_positions[3] = originInner;
            }
            else
            {
                m_positions[2] = Vector2.zero;
                m_positions[3] = Vector2.zero;
            }
 
            // 设置顶点的颜色坐标以及uv
            for (int j = 0; j < 4; j++)
            {
                m_vertexes[j].color = color;
                m_vertexes[j].position = m_positions[j];
                m_vertexes[j].uv0 = m_uvs[j];
            }
 
            //当前顶点数量
            int vertCount = vh.currentVertCount;
 
            //如果是圆只需要添加三个顶点，创建一个三角面
            vh.AddVert(m_vertexes[0]);
            vh.AddVert(m_vertexes[1]);
            vh.AddVert(m_vertexes[2]);
            //参数即三角面的顶点绘制顺序
            vh.AddTriangle(vertCount, vertCount + 2, vertCount + 1);
 
            // 如果是圆环就需要添加第四个顶点，并再创建一个三角面
            if (shapeType == ShapeType.Annulus)
            {
                vh.AddVert(m_vertexes[3]);
                vh.AddTriangle(vertCount, vertCount + 3, vertCount + 2);
            }
 
            //当前面片的终点就是下个面片的起点
            originOuter = m_positions[1];
            originInner = m_positions[2];
        }
    }

    //m_originType改变的时候需要重新设置m_originRadian
    void SetOriginRadian()
    {
        switch (m_originType)
        {
            case Image.Origin360.Left:
                m_originRadian = 0 * Mathf.Deg2Rad;
                break;
            case Image.Origin360.Top:
                m_originRadian = 90 * Mathf.Deg2Rad;
                break;
            case Image.Origin360.Right:
                m_originRadian = 180 * Mathf.Deg2Rad;
                break;
            case Image.Origin360.Bottom:
                m_originRadian = 270 * Mathf.Deg2Rad;
                break;
        }
    }
}