using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xx
{
    class TimerHandler
    {
        public int Id;
        public int Delay;
        public bool Repeat;
        public bool UserFrame;
        public long ExeTime;
        public Delegate Method;
        public object[] Args;

        public void Clear()
        {
            Method = null;
            Args = null;
        }
    }

    public delegate void Handler();
    public delegate void Handler<T1>(T1 param1);
    public delegate void Handler<T1, T2>(T1 param1, T2 param2);
    public delegate void Handler<T1, T2, T3>(T1 param1, T2 param2, T3 param3);

    public class TimeManager
	{
        static TimeManager mInstance;
        public static TimeManager Instance
        {
            get {
                if (mInstance == null)
                {
                    mInstance = new TimeManager();
                }
                return mInstance;
            }
        }

        static float mUpdateBeginTime;

        public static void InitUpdateDeltaTimePerFrame()
        {
            mUpdateBeginTime = Time.realtimeSinceStartup;
        }

        public static float DeltaTime
        {
            get
            {
                float deltaTime = (Time.realtimeSinceStartup - mUpdateBeginTime) * Time.timeScale;
#if UNITY_EDITOR
                //防止断点时deltaTime过大
                deltaTime = Mathf.Clamp(deltaTime, 0.0f, 0.1f);
#endif
                return deltaTime + Time.deltaTime;
            }
        }

        long mServerMillseconds;//Milliseconds
        float mGameSyncRealtime;
        bool mIsSyncTime = false;
        Ping mServerPing;
        readonly DateTime mUnixEpochTime = new DateTime(1970, 1, 1);
        public DateTime LocalUnixEpochTime
        {
            get
            {
                return TimeZone.CurrentTimeZone.ToLocalTime(mUnixEpochTime);
            }
        }

        void Start()
		{
			
		}

		void Update()
		{
			
		}
	}
}

