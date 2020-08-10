using System;
using UnityEngine;

public class GyroBase
{
    public float MaxValue;//最大偏移值
    public float DefaultValue;//初始位置
    float mCurrentValue;//当前偏移量

    public float Speed;//速度
    public float DuringTime;//等待间隔
    float mCurrentDuringTime;//当前时间间隔

    public Action<float> ValueChanged;//偏移事件

    public GyroManager mManager;

    float mBackSpeed;//回弹速度（一个减速过程）
    float BackSpeed
    {
        get
        {
            if (mBackSpeed > mMinSpeed)
            {
                mBackSpeed = Mathf.Max(mBackSpeed - Speed * mDeltaTime, mMinSpeed);
            }
            return mBackSpeed;
        }
    }

    float mMinSpeed;//最小速度
    float mDeltaTime;//Time.deltaTime

    bool mIsLandScape;//检测手机水平转动还是垂直转动
    bool mIsResetBackProperty = false;

    //初始化赋值
    public void Init(float maxValue, float defaultValue, float speed, float duringTime, bool isLandscape, Action<float> action)
    {
        MaxValue = maxValue;
        DefaultValue = defaultValue;
        Speed = speed;
        DuringTime = duringTime;
        mMinSpeed = Speed * 0.2f;
        mCurrentValue = DefaultValue;
        mIsLandScape = isLandscape;

        if (mIsLandScape)
        {
            mManager.LandscapeTransToDefault += TransToDefault;
            mManager.LandscapeTransToAdd += TransToAdd;
            mManager.LandscapeTransToReduce += TransToReduce;
        }
        else
        {
            mManager.PortraitTransToDefault += TransToDefault;
            mManager.PortraitTransToAdd += TransToAdd;
            mManager.PortraitTransToReduce += TransToReduce;
        }

        ValueChanged = action;
    }

    //事件清除
    public void Clear()
    {
        if (mIsLandScape)
        {
            mManager.LandscapeTransToDefault -= TransToDefault;
            mManager.LandscapeTransToAdd -= TransToAdd;
            mManager.LandscapeTransToReduce -= TransToReduce;
        }
        else
        {
            mManager.PortraitTransToDefault -= TransToDefault;
            mManager.PortraitTransToAdd -= TransToAdd;
            mManager.PortraitTransToReduce -= TransToReduce;
        }
    }

    //重设回弹参数
    void ResetBackProperty()
    {
        if (!mIsResetBackProperty)
        {
            mIsResetBackProperty = true;
            mBackSpeed = Speed * 0.8f;
            mCurrentDuringTime = 0;
        }
    }

    //手机没转动的时候，超过间隔时间则减速回弹至默认位置
    void TransToDefault()
    {
        mIsResetBackProperty = false;
        mDeltaTime = Time.deltaTime;
        mCurrentDuringTime += mDeltaTime;
        if (mCurrentDuringTime > 1)
        {
            ValueToDefault();
            ValueChanged?.Invoke(mCurrentValue);
        }
    }

    //偏移增加
    void TransToAdd(int difference)
    {
        ResetBackProperty();
        ValueAddSpeed(difference);
        ValueChanged?.Invoke(mCurrentValue);
    }

    //偏移减小
    void TransToReduce(int difference)
    {
        ResetBackProperty();
        ValueReduceSpeed(difference);
        ValueChanged?.Invoke(mCurrentValue);
    }

    void ValueToDefault()
    {
        if (mCurrentValue > DefaultValue)
        {
            mCurrentValue = Mathf.Max(mCurrentValue - BackSpeed * mDeltaTime, DefaultValue);
        }
        else if (mCurrentValue < DefaultValue)
        {
            mCurrentValue = Mathf.Min(mCurrentValue + BackSpeed * mDeltaTime, DefaultValue);
        }
    }

    void ValueAddSpeed(int difference)
    {
        if (mCurrentValue < DefaultValue + MaxValue)
        {
            mCurrentValue = Mathf.Min(mCurrentValue + Speed * mDeltaTime * difference, DefaultValue + MaxValue);
        }
    }

    void ValueReduceSpeed(int difference)
    {
        if (mCurrentValue > DefaultValue - MaxValue)
        {
            mCurrentValue = Mathf.Max(mCurrentValue - Speed * mDeltaTime * difference, DefaultValue - MaxValue);
        }
    }
}