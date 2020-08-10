using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xx
{
	public class TrackMissile : MonoBehaviour
	{
        //瞄准的目标
        public Transform target;

        //炮弹本地坐标速度，有大小有方向。
        Vector3 speed = new Vector3(0, 0, 5);

        //存储转向前炮弹的本地坐标速度
        Vector3 lastSpeed;

        //旋转的速度，单位 度/秒
        int rotateSpeed = 90;

        //目标到自身连线的向量，最终朝向
        Vector3 finalForward;

        //自己的forward朝向和mFinalForward之间的夹角
        float angleOffset;

        RaycastHit hit;

        void Start()
		{
            //将炮弹的本地坐标速度转换为世界坐标
            speed = transform.TransformDirection(speed);
            Debug.Log("speed:"+ speed);
        }

		void Update()
		{
            CheckHint();
            UpdateRotation();
            UpdatePosition();
        }

        //射线检测，如果击中目标点则销毁炮弹
        void CheckHint()
        {
            if(Physics.Raycast(transform.position, transform.forward, out hit)){
                if(hit.transform == target && hit.distance < 1)
                {
                    Destroy(gameObject);
                }
            }
        }

        //更新位置
        void UpdatePosition()
        {
            transform.position = transform.position + speed * Time.deltaTime;
        }

        //旋转，使其朝向目标点，要改变速度的方向
        void UpdateRotation()
        {
            //先将速度转为本地坐标，旋转之后再变为世界坐标
            lastSpeed = transform.InverseTransformDirection(speed);

            ChangeForward(rotateSpeed * Time.deltaTime);

            speed = transform.TransformDirection(lastSpeed);
        }

        void ChangeForward(float speed)
        {
            //获得目标点到自身的朝向
            finalForward = (target.position - transform.position).normalized;
            if (finalForward != transform.forward)
            {
                angleOffset = Vector3.Angle(transform.forward, finalForward);
                //将自身forward朝向慢慢转向最终朝向
                transform.forward = Vector3.Lerp(transform.forward, finalForward, speed / angleOffset);
            }
        }
    }
}

