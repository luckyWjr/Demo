using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineTest : MonoBehaviour
{
    public CinemachineBrain brain;
    public CinemachineVirtualCamera vc1;
    public CinemachineVirtualCamera vc2;

    public Transform cube;
    
    float time, deltaTime;
    float x;
    Transform mainCameraTransform;

    void Start()
    {
        time = 0;
        brain.m_CameraCutEvent.AddListener(CameraCutEvent);
        brain.m_CameraActivatedEvent.AddListener(CameraActivatedEvent);
        mainCameraTransform = Camera.main.transform;
        x = mainCameraTransform.position.x;
    }
    
    void CameraCutEvent(CinemachineBrain brain)
    {
        Debug.Log(brain);
    }
    
    //第一个参数为新的Live状态的VirtualCamera，第二个参数为上一个Live状态的VirtualCamera
    void CameraActivatedEvent(ICinemachineCamera liveCamera, ICinemachineCamera lastCamera)
    {
        Debug.Log("-------------------------------------------------------------------------------------------------------------");
    }

    void Update()
    {
        deltaTime = Time.deltaTime;
        time += deltaTime;
        if (time > 3)
        {
            // time = 0;
            // vc2.Priority = 100;
            
            // cube.Translate(Vector3.right * 5 * deltaTime);
        }
        
        Debug.Log(vc1.State.ShotQuality);
    }

    void LateUpdate()
    {
        // float different = mainCameraTransform.position.x - x;
        // if (different != 0)
        //     Debug.Log(different / deltaTime);
        // x = mainCameraTransform.position.x;
    }
}
