using UnityEngine;
using UnityEngine.EventSystems;

public class UIRotate : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField]CameraAndLookAtRotateWithTarget m_controller;
    Vector2 m_lastPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //屏幕拖动
        m_controller.Rotate(m_lastPosition - eventData.position);
        m_lastPosition = eventData.position;
    }

    void Update()
    {
        //鼠标滚轮
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            m_controller.ChangeDistance(1);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            m_controller.ChangeDistance(-1);
        }
    }
}