using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace xxx{
	public class UITest : MonoBehaviour
	{
        public CameraGyro CameraGyro;
        public Button Btn;
        public InputField input;
        public GyroManager mManager;

        void Start()
		{
            Btn.onClick.AddListener(Clicked);
            input.text = "7";
            input.onValueChanged.AddListener(SpeedChange);

            GyroBase mGyroBase = new GyroBase();
            mGyroBase.mManager = mManager;
            mGyroBase.Init(80, Btn.transform.localPosition.x, 80, 1, true, Change);
        }

        void SpeedChange(string s)
        {
            //CameraGyro.mSpeed = int.Parse(s);
        }

        void Change(float value)
        {
            Btn.transform.localPosition = new Vector3(value, Btn.transform.localPosition.y);
        }

        void Clicked()
		{
        }
	}
}