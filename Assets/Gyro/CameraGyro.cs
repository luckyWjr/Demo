using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xxx{
	public class CameraGyro : MonoBehaviour
	{
        public GyroManager mManager;

        Transform mTransform;
        Vector3 mCameraAngle;

        GyroBase mGyroBase;

        void Start()
		{
            mTransform = transform;
            mCameraAngle = Vector3.zero;

            mGyroBase = new GyroBase();
            mGyroBase.mManager = mManager;
            mGyroBase.Init(5, 0, 5, 1, false, Change);
        }

        void Change(float value)
        {
            mCameraAngle.x = value;
            mTransform.localEulerAngles = mCameraAngle;
        }
    }
}