using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateInfoManager<T> where T : BaseStateInfo, new()
{
    protected List<T> mStateInfoList;

    int mCount;
    public int Count => mCount;

    public T this[int i] => mStateInfoList[i];

    public BaseStateInfoManager()
    {
        mStateInfoList = new List<T>();
    }

    public T InsertState()
    {
        T state = new T();
        int firstAvailable = mStateInfoList.FindIndex(s => s == null);
        if (firstAvailable == -1)
        {
            firstAvailable = mStateInfoList.Count;
            mStateInfoList.Add(state);
        }
        else
            mStateInfoList.Insert(firstAvailable, state);

        state.Index = firstAvailable;
        mCount++;
        return state;
    }

    public bool AnyStatePlaying()
    {
        return mStateInfoList.FindIndex(s => s != null && s.Enabled) != -1;
    }

    public void RemoveState(int index)
    {
        BaseStateInfo removed = mStateInfoList[index];
        mStateInfoList[index] = null;
        removed.DestroyPlayable();
        mCount = mStateInfoList.Count;
    }
    
    public BaseStateInfo FindState(string name)
    {
        int index = mStateInfoList.FindIndex(s => s != null && s.StateName == name);
        if (index == -1)
            return null;

        return mStateInfoList[index];
    }

    public void EnableState(int index)
    {
        BaseStateInfo state = mStateInfoList[index];
        state.Enable();
    }

    public void DisableState(int index)
    {
        BaseStateInfo state = mStateInfoList[index];
        state.Disable();
    }

    public void SetInputWeight(int index, float weight)
    {
        BaseStateInfo state = mStateInfoList[index];
        state.SetWeight(weight);

    }

    public void SetStateTime(int index, float time)
    {
        BaseStateInfo state = mStateInfoList[index];
        state.SetTime(time);
    }

    public float GetStateTime(int index)
    {
        BaseStateInfo state = mStateInfoList[index];
        return state.GetTime();
    }

    public abstract float GetStateLength(int index);

    public abstract float GetClipLength(int index);
    
    protected abstract bool IsCloneOf(int potentialCloneIndex, int originalIndex);

    public float GetStateSpeed(int index)
    {
        return mStateInfoList[index].speed;
    }

    public void SetStateSpeed(int index, float value)
    {
        mStateInfoList[index].speed = value;
    }

    public float GetInputWeight(int index)
    {
        return mStateInfoList[index].Weight;
    }

    public float GetStatePlayableDuration(int index)
    {
        return mStateInfoList[index].playableDuration;
    }

    public string GetStateName(int index)
    {
        return mStateInfoList[index].StateName;
    }
    
    public void StopState(int index, bool cleanup)
    {
        if (cleanup)
            RemoveState(index);
        else
            mStateInfoList[index].Stop();
    }

    public void ChangeAllStateSpeed(float speed)
    {
        foreach (T state in mStateInfoList)
        {
            if (state != null)
                state.speed = speed;
        }
    }
}
