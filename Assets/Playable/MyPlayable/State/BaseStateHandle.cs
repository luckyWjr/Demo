using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class BaseStateHandle<T, U>: IState  where T : BaseStateInfo, new() where U : BaseStateInfoManager<T>, new()
{
    protected int mIndex;
    public int index => mIndex;

    BasePlayable<T, U> mParent;
    Playable mTarget;

    protected BaseStateHandle(BasePlayable<T, U> playable, int index, Playable target)
    {
        mParent = playable;
        mIndex = index;
        mTarget = target;
    }

    public bool IsValid()
    {
        return mParent.ValidateInput(mIndex, mTarget);
    }

    public bool Enabled
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mParent.StateInfoManager[mIndex].Enabled;
        }
        set
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            if (value)
                mParent.StateInfoManager.EnableState(mIndex);
            else
                mParent.StateInfoManager.DisableState(mIndex);
        }
    }

    public float Time
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mParent.StateInfoManager.GetStateTime(mIndex);
        }
        set
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            mParent.StateInfoManager.SetStateTime(mIndex, value);
        }
    }

    public float NormalizedTime
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");

            float length = mParent.StateInfoManager.GetClipLength(mIndex);
            if (length == 0f)
                length = 1f;

            return mParent.StateInfoManager.GetStateTime(mIndex) / length;
        }
        set
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");

            float length = mParent.StateInfoManager.GetClipLength(mIndex);
            if (length == 0f)
                length = 1f;

            mParent.StateInfoManager.SetStateTime(mIndex, value *= length);
        }
    }

    public float Speed
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mParent.StateInfoManager.GetStateSpeed(mIndex);
        }
        set
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            mParent.StateInfoManager.SetStateSpeed(mIndex, value);
        }
    }

    public string Name
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mParent.StateInfoManager.GetStateName(mIndex);
        }
    }

    public float Weight
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mParent.StateInfoManager[mIndex].Weight;
        }
        set
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            if (value < 0)
                throw new System.ArgumentException("Weights cannot be negative");

            mParent.StateInfoManager.SetInputWeight(mIndex, value);
        }
    }

    public float Length
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mParent.StateInfoManager.GetStateLength(mIndex);
        }
    }
}
