using System;
using UnityEngine;

public enum EGyroType
{
    NoRotate,//不旋转
    ToUp,//手机下方向上倾斜
    ToDown,//手机下方向下倾斜
    ToLeft,//左手边向下倾斜
    ToRight,//右手边向下倾斜
}

public class GyroManager : MonoBehaviour
{
    Gyroscope mGyro;//陀螺仪
    Vector2 mCurrentLandscapeGyroValue, mCurrentPortraitGyroValue;//当前的水平垂直的gravity值
    Vector2 mLastLandscapeGyroValue, mLastPortraitGyroValue;//上一次的水平垂直的gravity值

    public EGyroType LandscapeEGyroType, PortraitEGyroType;//手机的水平垂直状态
    float mPrecision = 0.015f;//精度，若前后两次gravity值在精度内，则认为当前没有旋转
    public int LandscapeGyroDifference, PortraitGyroDifference;//模拟的一个旋转速度，gravity值差异越大，则该值越大

    bool mIsEnable;//是否开启陀螺仪

    private void Start()
    {
        mGyro = Input.gyro;
        SetGyroEnable(true);
    }

    //每种状态下需要执行的事件
    public Action LandscapeTransToDefault;
    public Action<int> LandscapeTransToAdd;
    public Action<int> LandscapeTransToReduce;

    public Action PortraitTransToDefault;
    public Action<int> PortraitTransToAdd;
    public Action<int> PortraitTransToReduce;

    public void ResetLandscape()
    {
        LandscapeEGyroType = EGyroType.NoRotate;
        SetLandScapeValue();
        mLastLandscapeGyroValue = mCurrentLandscapeGyroValue;
        LandscapeGyroDifference = 0;
    }

    public void ResetPortrait()
    {
        PortraitEGyroType = EGyroType.NoRotate;
        SetPortraitValue();
        mLastPortraitGyroValue = Vector2.zero;
        PortraitGyroDifference = 0;
    }

    void Update()
    {
        if (mIsEnable)
        {
            GetEGyroType();

            //根据解析出来的手机状态，执行对应事件
            if (LandscapeEGyroType == EGyroType.ToLeft)
            {
                LandscapeTransToReduce?.Invoke(LandscapeGyroDifference);
            }
            else if (LandscapeEGyroType == EGyroType.ToRight)
            {
                LandscapeTransToAdd?.Invoke(LandscapeGyroDifference);
            }
            else
            {
                LandscapeTransToDefault?.Invoke();
            }

            if (PortraitEGyroType == EGyroType.ToDown)
            {
                PortraitTransToReduce?.Invoke(PortraitGyroDifference);
            }
            else if (PortraitEGyroType == EGyroType.ToUp)
            {
                PortraitTransToAdd?.Invoke(PortraitGyroDifference);
            }
            else
            {
                PortraitTransToDefault?.Invoke();
            }
        }
    }

    //开启或关闭陀螺仪
    public void SetGyroEnable(bool isEnable)
    {
        if (mIsEnable != isEnable)
        {
            mIsEnable = isEnable;
            ResetLandscape();
            ResetPortrait();
            mGyro.enabled = isEnable;
        }
    }

    //解析当前手机状态
    public void GetEGyroType()
    {
        SetLandScapeValue();
        //Landscape
        if (IsEquals(mCurrentLandscapeGyroValue.x, mLastLandscapeGyroValue.x, true))
        {
            LandscapeEGyroType = EGyroType.NoRotate;
            LandscapeGyroDifference = 0;
        }
        else
        {
            LandscapeGyroDifference = (int)(Mathf.Abs(mCurrentLandscapeGyroValue.x - mLastLandscapeGyroValue.x) * 60);

            if (mCurrentLandscapeGyroValue.y < 0 && mLastLandscapeGyroValue.y < 0)
            {
                //当 z < 0，x的值变小则为ToLeft，变大则为ToRight
                if (mCurrentLandscapeGyroValue.x < mLastLandscapeGyroValue.x)
                {
                    LandscapeEGyroType = EGyroType.ToLeft;
                }
                else
                {
                    LandscapeEGyroType = EGyroType.ToRight;
                }
            }
            else if (mCurrentLandscapeGyroValue.y > 0 && mLastLandscapeGyroValue.y > 0)
            {
                //当 z > 0，x的值变大则为ToLeft，变小则为ToRight
                if (mCurrentLandscapeGyroValue.x < mLastLandscapeGyroValue.x)
                {
                    LandscapeEGyroType = EGyroType.ToRight;
                }
                else
                {
                    LandscapeEGyroType = EGyroType.ToLeft;
                }
            }
            else
            {
                if (mCurrentLandscapeGyroValue.y < mLastLandscapeGyroValue.y)
                {
                    //当 z < 0 变为 z > 0，若 x < 0 则为ToLeft，否则则为ToRight
                    if (mCurrentLandscapeGyroValue.x > 0)
                    {
                        LandscapeEGyroType = EGyroType.ToLeft;
                    }
                    else
                    {
                        LandscapeEGyroType = EGyroType.ToRight;
                    }
                }
                else
                {
                    //当 z > 0 变为 z<0，若 x< 0 则为ToRight，否则则为ToLeft
                    if (mCurrentLandscapeGyroValue.x < 0)
                    {
                        LandscapeEGyroType = EGyroType.ToLeft;
                    }
                    else
                    {
                        LandscapeEGyroType = EGyroType.ToRight;
                    }
                }
            }
        }
        mLastLandscapeGyroValue = mCurrentLandscapeGyroValue;

        SetPortraitValue();
        //Portrait
        if (IsEquals(mCurrentPortraitGyroValue.x, mLastPortraitGyroValue.x, false))
        {
            PortraitEGyroType = EGyroType.NoRotate;
            PortraitGyroDifference = 0;
        }
        else
        {
            PortraitGyroDifference = (int)(Mathf.Abs(mCurrentPortraitGyroValue.x - mLastPortraitGyroValue.x) * 60);

            if (mCurrentPortraitGyroValue.y < 0 && mLastPortraitGyroValue.y < 0)
            {
                //当 z< 0，y的值变大则为ToUp，变小则为ToDown
                if (mCurrentPortraitGyroValue.x < mLastPortraitGyroValue.x)
                {
                    PortraitEGyroType = EGyroType.ToDown;
                }
                else
                {
                    PortraitEGyroType = EGyroType.ToUp;
                }
            }
            else if (mCurrentPortraitGyroValue.y > 0 && mLastPortraitGyroValue.y > 0)
            {
                //当 z > 0，y的值变大则为ToDown，变小则为ToUp
                if (mCurrentPortraitGyroValue.x < mLastPortraitGyroValue.x)
                {
                    PortraitEGyroType = EGyroType.ToUp;
                }
                else
                {
                    PortraitEGyroType = EGyroType.ToDown;
                }
            }
            else
            {
                //当 z<0 变为 z > 0，则为ToDown，反之则为ToUp
                if (mCurrentPortraitGyroValue.y < mLastPortraitGyroValue.y)
                {
                    //>0 变 <0
                    PortraitEGyroType = EGyroType.ToUp;
                }
                else
                {
                    PortraitEGyroType = EGyroType.ToDown;
                }
            }
        }
        mLastPortraitGyroValue = mCurrentPortraitGyroValue;
    }

    //读取gravity值
    public void SetLandScapeValue()
    {
        mCurrentLandscapeGyroValue.x = mGyro.gravity.x;
        mCurrentLandscapeGyroValue.y = mGyro.gravity.z;
    }

    public void SetPortraitValue()
    {
        mCurrentPortraitGyroValue.x = mGyro.gravity.y;
        mCurrentPortraitGyroValue.y = mGyro.gravity.z;
    }

    //前后两次是否相等
    bool IsEquals(float a, float b, bool isLandscape)
    {
        if ((isLandscape && LandscapeEGyroType == EGyroType.NoRotate) || (!isLandscape && PortraitEGyroType == EGyroType.NoRotate))
        {
            if (Mathf.Abs(a - b) < 0.025f)
            {
                return true;
            }
        }
        if (Mathf.Abs(a - b) < mPrecision)
        {
            return true;
        }
        return false;
    }
}