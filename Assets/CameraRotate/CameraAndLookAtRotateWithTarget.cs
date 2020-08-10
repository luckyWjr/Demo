using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraAndLookAtRotateWithTarget : MonoBehaviour
{
    [SerializeField] Transform m_target;//需要被观察的点
    [SerializeField] Transform m_lookedAt;//摄像机lookat的点

    [SerializeField] bool m_isTargetLeft;//相机看到的，m_target是否在m_lookedAt的左边

    [SerializeField] float m_defaultCameraAngleX = 180;//相机初始位置相对m_lookedAt.forward的水平角度偏移
    [SerializeField] float m_defaultCameraAngleY = 5;//相机初始位置相对法线为m_lookedAt.up的平面的角度偏移，锐角
    [SerializeField] float m_defaultCameraDistance = 8;//相机初始位置相对m_lookedAt的距离
#if UNITY_EDITOR
    [SerializeField] bool m_isChangeDefaultValue = false;//修改默认值之后设为true来重新设置摄像机初始位置，方便调试
#endif

    //相机上下的最大最小偏移角度
    [SerializeField] float m_cameraMaxAngleY = 60;
    [SerializeField] float m_cameraMinAngleY = 0;

    //相机距离m_target的最大最小距离
    [SerializeField] float m_cameraMaxDistance = 20;
    [SerializeField] float m_cameraMinDistance = 5;

    [SerializeField] float m_cameraCurrentDistance = 0;//显示当前相机距离m_target的距离，方便调试

    Transform m_transform;

    const float ANGLE_CONVERTER = Mathf.PI / 180;//弧度，用于Mathf.Sin，Mathf.Cos的计算

    //m_target到m_lookedAt的距离，m_target到相机距离
    float m_targetToLookedAtDistance, m_targetToCameraDistance;

    //m_lookedAt和相机到m_target的连线，x为相对m_target.forward的角度，y为相对法线为m_target.up的平面的角度
    float m_lookedAtAngleX, m_lookedAtAngleY, m_cameraAngleX, m_cameraAngleY;

    ///m_lookedAt和相机的位置相对m_target位置的偏移
    Vector3 m_lookedAtOffset, m_cameraOffset;

    //是否旋转或拉近拉远了
    bool m_isChange = false;

    //距离每次变化的比例
    float m_changeDistanceRatio = 0.05f;

    void Awake()
    {
        m_transform = transform;
    }

    void Start()
    {
        SetCameraDefaultPosition();
        GetInitialData();
    }

    //设置摄像机的初始位置
    void SetCameraDefaultPosition()
    {
        Vector3 offset = CalculateOffset(m_defaultCameraDistance, m_defaultCameraAngleX, m_defaultCameraAngleY);
        m_transform.position = m_lookedAt.position + offset;
        m_transform.LookAt(m_lookedAt);
    }

    void GetInitialData()
    {
        //获取距离
        m_targetToLookedAtDistance = Vector3.Distance(m_lookedAt.position, m_target.position);
        m_targetToCameraDistance = Vector3.Distance(m_transform.position, m_target.position);

        //获取角度
        Vector3 targetToLookedAt = m_lookedAt.position - m_target.position;
        m_lookedAtAngleX = Vector3.Angle(Vector3.ProjectOnPlane(targetToLookedAt, m_target.up), m_target.forward);
        m_lookedAtAngleY = Vector3.Angle(Vector3.ProjectOnPlane(targetToLookedAt, m_target.up), targetToLookedAt);
        //取锐角
        if ((m_isTargetLeft && targetToLookedAt.x < 0) || (!m_isTargetLeft && targetToLookedAt.x > 0))
        {
            m_lookedAtAngleY = 180 - m_lookedAtAngleY;
        }

        Vector3 targetToCamera = m_transform.position - m_target.position;
        m_cameraAngleX = Vector3.Angle(Vector3.ProjectOnPlane(targetToCamera, m_target.up), m_target.forward);
        m_cameraAngleY = Vector3.Angle(Vector3.ProjectOnPlane(targetToCamera, m_target.up), targetToCamera);
        if ((m_isTargetLeft && targetToCamera.x < 0) || (!m_isTargetLeft && targetToCamera.x > 0))
        {
            m_cameraAngleY = 180 - m_cameraAngleY;
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (m_isChangeDefaultValue)
        {
            SetCameraDefaultPosition();
            m_isChangeDefaultValue = false;
        }
    }
#endif

    void LateUpdate()
    {
        if (m_isChange)
        {
            //更新位置
            CalculateLookedAtAndCameraOffset();
            m_isChange = false;

            m_lookedAt.position = m_target.position + m_lookedAtOffset;
            m_transform.position = m_target.position + m_cameraOffset;

            m_transform.LookAt(m_lookedAt);
        }
    }

    //根据角度计算偏移相机和m_lookedAt相对m_target的偏移
    void CalculateLookedAtAndCameraOffset()
    {
        m_lookedAtOffset = CalculateOffset(m_targetToLookedAtDistance, m_lookedAtAngleX, m_lookedAtAngleY);
        m_cameraOffset = CalculateOffset(m_targetToCameraDistance, m_cameraAngleX, m_cameraAngleY);
        if (!m_isTargetLeft)
        {
            m_lookedAtOffset.x = -m_lookedAtOffset.x;
            m_cameraOffset.x = -m_cameraOffset.x;
        }
    }

    //可以想象成在一个球面上运动，根据xy的角度和球的半径，计算球面上的一个点相对球心的偏移
    //先计算出高度y，然后就是在一个圆面上去计算xz
    Vector3 CalculateOffset(float distance, float x, float y)
    {
        Vector3 offset;
        offset.y = distance * Mathf.Sin(y * ANGLE_CONVERTER);
        float newRadius = distance * Mathf.Cos(y * ANGLE_CONVERTER);
        offset.x = newRadius * Mathf.Sin(x * ANGLE_CONVERTER);
        offset.z = newRadius * Mathf.Cos(x * ANGLE_CONVERTER);
        return offset;
    }

    //水平方向同时修改m_lookedAtAngleX和m_cameraAngleX，即m_lookAt和相机一同绕着m_target公转
    //垂直方向，只修改m_cameraAngleY，即只有相机会有高低变化
    public void Rotate(Vector2 v)
    {
        if (v.x != 0)
        {
            if (m_isTargetLeft)
            {
                m_lookedAtAngleX -= v.x;
                m_cameraAngleX -= v.x;
            }
            else
            {
                m_lookedAtAngleX += v.x;
                m_cameraAngleX += v.x;
            }
        }
        if (v.y != 0)
        {
            m_cameraAngleY += v.y;
            m_cameraAngleY = m_cameraAngleY > m_cameraMaxAngleY ? m_cameraMaxAngleY : m_cameraAngleY;
            m_cameraAngleY = m_cameraAngleY < m_cameraMinAngleY ? m_cameraMinAngleY : m_cameraAngleY;
        }

        m_isChange = true;
    }

    //同时缩放m_target到相机和m_target到m_lookedAt的距离
    public void ChangeDistance(float value)
    {
        if (value > 0)
        {
            if (m_targetToCameraDistance < m_cameraMaxDistance)
            {
                m_targetToCameraDistance += m_targetToCameraDistance * m_changeDistanceRatio;
                m_targetToLookedAtDistance += m_targetToLookedAtDistance * m_changeDistanceRatio;
            }
        }
        else
        {
            if (m_targetToCameraDistance > m_cameraMinDistance)
            {
                m_targetToCameraDistance -= m_targetToCameraDistance * m_changeDistanceRatio;
                m_targetToLookedAtDistance -= m_targetToLookedAtDistance * m_changeDistanceRatio;
            }
        }

        m_isChange = true;
    }
}