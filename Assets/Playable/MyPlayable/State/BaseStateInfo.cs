using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class BaseStateInfo
{
    bool mEnabledDirty;
    public bool EnabledDirty => mEnabledDirty;

    bool mWeightDirty;
    public bool WeightDirty => mWeightDirty;
    
    bool mIsUpdateTimeInFrame;
    
    Playable mPlayable;
    public Playable Playable => mPlayable;
    
    public float playableDuration => (float) mPlayable.GetDuration();
    public bool isDone => mPlayable.IsDone();

    bool mIsClone;
    public bool IsClone => mIsClone;
    
    bool mReadyForCleanup;
    public bool IsReadyForCleanup => mReadyForCleanup;

    bool mEnabled;
    public bool Enabled => mEnabled;

    int mIndex;
    public int Index
    {
        get => mIndex;
        set
        {
            Debug.Assert(mIndex == 0, "Should never reassign Index");
            mIndex = value;
        }
    }

    protected int mOriginalIndex = -1;
    public int OriginalIndex => mOriginalIndex;

    protected string mStateName;
    public string StateName => mStateName;

    bool mFading;
    public bool Fading => mFading;

    float mTime;

    float mTargetWeight;
    public float TargetWeight => mTargetWeight;

    float mWeight;
    public float Weight => mWeight;

    float mFadeSpeed;
    public float FadeSpeed => mFadeSpeed;

    public virtual bool IsLoop => false;
    
    public virtual float Length => 0;
    
    List<EventInfo> mEventList;
    public List<EventInfo> EventList => mEventList;

    public Action OnStopCallback;

    protected BaseStateInfo()
    {
        mEventList = new List<EventInfo>();
    }
    
    public float speed
    {
        get => (float) mPlayable.GetSpeed();
        set => mPlayable.SetSpeed(value);
    }
    
    public float GetTime()
    {
        if (mIsUpdateTimeInFrame)
            return mTime;

        // use Playable.GetTime() once in a frame
        mTime = (float) mPlayable.GetTime();
        mIsUpdateTimeInFrame = true;
        return mTime;
    }

    public void SetTime(float newTime)
    {
        mTime = newTime;
        mPlayable.ResetTime(mTime);
        mPlayable.SetDone(mTime >= mPlayable.GetDuration());
    }

    public void Enable()
    {
        if (mEnabled)
            return;

        mEnabledDirty = true;
        mEnabled = true;
    }

    public void Disable()
    {
        if (mEnabled == false)
            return;

        mEnabledDirty = true;
        mEnabled = false;
    }

    public void Play()
    {
        mPlayable.Play();
    }
    
    public void Pause()
    {
        mPlayable.Pause();
    }

    public void Stop()
    {
        if(Enabled)
            OnStopCallback?.Invoke();
        
        mFadeSpeed = 0f;
        ForceWeight(0.0f);
        Disable();
        SetTime(0.0f);
        mPlayable.SetDone(false);
        ResetEventExecuteFlag();
        if (mIsClone)
            mReadyForCleanup = true;
    }

    public void ForceWeight(float weight)
    {
        mTargetWeight = weight;
        mFading = false;
        mFadeSpeed = 0f;
        SetWeight(weight);
    }

    public void SetWeight(float weight)
    {
        mWeight = weight;
        mWeightDirty = true;
    }

    public void FadeTo(float weight, float speed)
    {
        mFading = Mathf.Abs(speed) > 0f;
        mFadeSpeed = speed;
        mTargetWeight = weight;
    }

    public virtual void DestroyPlayable()
    {
        mEventList.Clear();
        mEventList = null;
        
        if (mPlayable.IsValid())
            mPlayable.GetGraph().DestroySubgraph(mPlayable);
    }
    
    public void SetPlayable(Playable playable)
    {
        mPlayable = playable;
    }

    public void Clone(BaseStateInfo originalStateInfo)
    {
        mIsClone = true;
        mOriginalIndex = originalStateInfo.Index;
        speed = originalStateInfo.speed;
        OnStopCallback = originalStateInfo.OnStopCallback;

        foreach (var eventInfo in originalStateInfo.EventList)
            mEventList.Add(eventInfo);
    }
    
    public void ResetDirtyFlags()
    {
        mEnabledDirty = false;
        mWeightDirty = false;
    }

    public void InvalidateTime()
    {
        mIsUpdateTimeInFrame = false;
    }

    void ResetEventExecuteFlag()
    {
        foreach (var e in mEventList)
            e.Reset();
    }

    public void AddEvent(float time, Action<object> handle, object data = null)
    {
        if (time > Length)
        {
            Debug.Log("the event time greater than Length");
            return;
        }

        mEventList.Add(new EventInfo(time, handle, data));
    }

    public class EventInfo
    {
        public readonly float ExecuteTime;
        public bool IsExecute { get; private set; }
        object Data;
        Action<object> Handle;

        public EventInfo(float time, Action<object> handle, object data = null)
        {
            ExecuteTime = time;
            Handle = handle;
            Data = data;
            IsExecute = false;
        }

        public void Execute()
        {
            Handle?.Invoke(Data);
            IsExecute = true;
        }

        public void Reset()
        {
            IsExecute = false;
        }
    }
}
