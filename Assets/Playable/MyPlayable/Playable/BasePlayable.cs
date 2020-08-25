using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public abstract class BasePlayable<T, U> : PlayableBehaviour
    where T : BaseStateInfo, new() where U : BaseStateInfoManager<T>, new()
{
    protected struct QueuedState
    {
        // public QueuedState(BaseStateHandle<T, U> handle, float t)
        // {
        //     State = handle;
        //     FadeTime = t;
        // }

        public QueuedState(int index, float t)
        {
            Index = index;
            FadeTime = t;
        }

        // public BaseStateHandle<T, U> State;
        public int Index;
        public float FadeTime;
    }

    protected LinkedList<QueuedState> mStateQueue;
    protected U mStateInfoManager;
    public U StateInfoManager => mStateInfoManager;

    Playable mSelfPlayable;
    public Playable SelfPlayable => mSelfPlayable;

    protected PlayableGraph mGraph => SelfPlayable.GetGraph();

    protected bool mIsKeepStoppedPlayablesConnected = true;

    public bool IsKeepStoppedPlayablesConnected
    {
        get => mIsKeepStoppedPlayablesConnected;
        set
        {
            if (value != mIsKeepStoppedPlayablesConnected)
            {
                mIsKeepStoppedPlayablesConnected = value;
                UpdateStoppedPlayablesConnections();
            }
        }
    }

    public Action onDone = null;

    protected Playable mMixerPlayable;

    protected BasePlayable()
    {
        mStateInfoManager = new U();
        mStateQueue = new LinkedList<QueuedState>();
    }

    #region PlayableBehaviour Function

    public override void OnPlayableCreate(Playable playable)
    {
        mSelfPlayable = playable;
        mSelfPlayable.SetInputCount(1);
        mSelfPlayable.SetInputWeight(0, 1);
        CreateMixerPlayable();
        mGraph.Connect(mMixerPlayable, 0, mSelfPlayable, 0);
    }

    public override void PrepareFrame(Playable owner, FrameData data)
    {
        InvalidateStateTimes();
        UpdateQueuedStates();
        UpdateStates(data.deltaTime);
        CallAnimationEvent();
        UpdateDoneStatus();
        CleanClonedStates();
    }

    public override void OnGraphStop(Playable playable)
    {
        if (!mSelfPlayable.IsValid())
            return;

        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = mStateInfoManager[i];
            if (state == null) continue;

            if (state.FadeSpeed == 0f && state.TargetWeight == 0f)
            {
                Playable input = mMixerPlayable.GetInput(state.Index);
                if (!input.Equals(Playable.Null))
                    input.ResetTime(0f);
            }
        }
    }

    #endregion

    #region FrameFunction

    void InvalidateStateTimes()
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
            mStateInfoManager[i]?.InvalidateTime();
    }

    void UpdateQueuedStates()
    {
        bool mustCalculateQueueTimes = true;
        float remainingTime = -1f;

        var it = mStateQueue.First;
        while (it != null)
        {
            if (mustCalculateQueueTimes)
            {
                remainingTime = CalculateQueueTimes();
                mustCalculateQueueTimes = false;
            }

            QueuedState queuedState = it.Value;
            if (queuedState.FadeTime >= remainingTime)
            {
                Crossfade(queuedState.Index, queuedState.FadeTime);
                mustCalculateQueueTimes = true;
                mStateQueue.RemoveFirst();
                it = mStateQueue.First;
            }
            else
                it = it.Next;
        }
    }

    protected void UpdateStates(float deltaTime)
    {
        bool mustUpdateWeights = false;
        float totalWeight = 0f;

        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = mStateInfoManager[i];

            if (state == null)
                continue;

            if (state.Fading)
            {
                state.SetWeight(Mathf.MoveTowards(state.Weight, state.TargetWeight, state.FadeSpeed * deltaTime));
                if (Mathf.Approximately(state.Weight, state.TargetWeight))
                {
                    state.ForceWeight(state.TargetWeight);
                    if (state.Weight == 0f)
                        state.Stop();
                }
            }

            if (state.EnabledDirty)
            {
                if (state.Enabled)
                    state.Play();
                else
                    state.Pause();

                if (!mIsKeepStoppedPlayablesConnected)
                {
                    Playable input = mMixerPlayable.GetInput(i);
                    //if state is disabled but the corresponding input is connected, disconnect it
                    if (input.IsValid() && !state.Enabled)
                        DisconnectInput(i);
                    else if (state.Enabled && !input.IsValid())
                        ConnectInput(state.Index);
                }
            }

            if (state.Enabled && !state.IsLoop)
            {
                bool stateIsDone = state.isDone;
                float speed = state.speed;
                float time = state.GetTime();
                float duration = state.playableDuration;

                stateIsDone |= speed < 0f && time < 0f;
                stateIsDone |= speed >= 0f && time >= duration;
                if (stateIsDone)
                {
                    state.Stop();
                    if (!mIsKeepStoppedPlayablesConnected)
                        DisconnectInput(state.Index);
                }
            }

            totalWeight += state.Weight;
            if (state.WeightDirty)
                mustUpdateWeights = true;

            state.ResetDirtyFlags();
        }

        if (mustUpdateWeights)
        {
            bool hasAnyWeight = totalWeight > 0.0f;
            for (int i = 0; i < mStateInfoManager.Count; i++)
            {
                BaseStateInfo state = mStateInfoManager[i];
                if (state == null)
                    continue;

                float weight = hasAnyWeight ? state.Weight / totalWeight : 0.0f;
                mMixerPlayable.SetInputWeight(state.Index, weight);
            }
        }
    }

    void CallAnimationEvent()
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = StateInfoManager[i];
            if (state == null || !state.Enabled || state.EventList.Count == 0) continue;

            float time = state.GetTime();
            foreach (var item in state.EventList)
                if (!item.IsExecute && time >= item.ExecuteTime)
                    item.Execute();
        }
    }

    protected void UpdateDoneStatus()
    {
        if (!mStateInfoManager.AnyStatePlaying())
        {
            bool wasDone = mSelfPlayable.IsDone();
            mSelfPlayable.SetDone(true);
            if (!wasDone)
                onDone?.Invoke();
        }
    }

    protected void CleanClonedStates()
    {
        for (int i = mStateInfoManager.Count - 1; i >= 0; i--)
        {
            BaseStateInfo state = mStateInfoManager[i];
            if (state == null) continue;

            if (state.IsReadyForCleanup)
            {
                Playable toDestroy = mMixerPlayable.GetInput(state.Index);
                mGraph.Disconnect(mMixerPlayable, state.Index);
                mGraph.DestroyPlayable(toDestroy);
                mStateInfoManager.RemoveState(i);
            }
        }
    }

    #endregion

    protected abstract void CreateMixerPlayable();
    
    // protected abstract int GetStateInfoParentIndex(int index);
    
    protected abstract BaseStateInfo CloneState(int index);
    
    protected abstract BaseStateHandle<T, U> CreateStateHandle(int index, Playable playable);

    protected abstract BaseStateHandle<T, U> BaseStateInfoToHandle(BaseStateInfo info);
    
    protected void ConnectInput(int index)
    {
        BaseStateInfo state = mStateInfoManager[index];
        mGraph.Connect(state.Playable, 0, mMixerPlayable, state.Index);
    }
    
    void DisconnectInput(int index)
    {
        if (mIsKeepStoppedPlayablesConnected)
            mStateInfoManager[index].Pause();

        mGraph.Disconnect(mMixerPlayable, index);
    }
    
    public Playable GetInput(int index)
    {
        if (index >= mMixerPlayable.GetInputCount())
            return Playable.Null;

        return mMixerPlayable.GetInput(index);
    }
    
    public bool Play(string name)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot play state with name {name} because there is no state with that name");
            return false;
        }

        return Play(state.Index);
    }
    
    bool Play(int index)
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = mStateInfoManager[i];

            //if clone is remove the state is none
            if (state == null) continue;

            if (state.Index == index)
            {
                state.Enable();
                state.ForceWeight(1.0f);
            }
            else
                DoStop(i);
        }

        return true;
    }
    
    public bool PlayQueued(string name, QueueMode queueMode)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot queue Play to state with name {name} because there is no state with that name");
            return false;
        }

        return PlayQueued(state.Index, queueMode);
    }

    bool PlayQueued(int index, QueueMode queueMode)
    {
        BaseStateInfo newState = CloneState(index);

        if (queueMode == QueueMode.PlayNow)
        {
            Play(newState.Index);
            return true;
        }

        mStateQueue.AddLast(new QueuedState(newState.Index, 0f));
        return true;
    }
    
    public bool Stop(string name)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot stop state with name {name} because there is no state with that name");
            return false;
        }

        DoStop(state.Index);
        UpdateDoneStatus();
        return true;
    }
    
    public bool StopAll()
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
            DoStop(i);

        mSelfPlayable.SetDone(true);
        return true;
    }
    
    public void Rewind(string name)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot Rewind state with name {name} because there is no state with that name");
            return;
        }

        Rewind(state.Index);
    }

    void Rewind(int index)
    {
        mStateInfoManager.SetStateTime(index, 0f);
    }

    public void RewindAll()
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
            if (mStateInfoManager[i] != null)
                mStateInfoManager.SetStateTime(i, 0f);
    }
    
    public bool Crossfade(string name, float time)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot cross fade to state with name {name} because there is no state with that name");
            return false;
        }

        if (time == 0f)
            return Play(state.Index);

        return Crossfade(state.Index, time);
    }
    
    bool Blend(int index, float targetWeight, float time)
    {
        BaseStateInfo state = mStateInfoManager[index];
        if (state.Enabled == false)
            mStateInfoManager.EnableState(index);

        if (time == 0f)
            state.ForceWeight(targetWeight);
        else
            SetupLerp(state, targetWeight, time);

        return true;
    }

    public bool Blend(string name, float targetWeight, float time)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot blend state with name {name} because there is no state with that name");
            return false;
        }

        return Blend(state.Index, targetWeight, time);
    }
    
    public bool IsPlaying()
    {
        return mStateInfoManager.AnyStatePlaying();
    }

    public bool IsPlaying(string stateName)
    {
        BaseStateInfo state = mStateInfoManager.FindState(stateName);
        if (state == null)
            return false;

        return state.Enabled || IsClonePlaying(state);
    }

    bool IsClonePlaying(BaseStateInfo state)
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo otherState = mStateInfoManager[i];
            if (otherState == null)
                continue;

            if (otherState.IsClone && otherState.Enabled && otherState.OriginalIndex == state.Index)
                return true;
        }
        return false;
    }
    
    // public IEnumerable<IState> GetStates()
    // {
    //     return new IStateEnumerable(this);
    // }
    
    public IState GetState(string name)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
            return null;
    
        return CreateStateHandle(state.Index, state.Playable);
    }
    

    public bool CrossfadeQueued(string name, float time, QueueMode queueMode)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot queue cross fade to state with name {name} because there is no state with that name");
            return false;
        }

        return CrossfadeQueued(state.Index, time, queueMode);
    }
    
    bool CrossfadeQueued(int index, float time, QueueMode queueMode)
    {
        BaseStateInfo newState = CloneState(index);

        if (queueMode == QueueMode.PlayNow)
        {
            Crossfade(newState.Index, time);
            return true;
        }

        mStateQueue.AddLast(new QueuedState(newState.Index, time));
        return true;
    }
    
    public bool ValidateInput(int index, Playable input)
    {
        if (!ValidateIndex(index))
            return false;

        BaseStateInfo state = mStateInfoManager[index];
        if (state == null || !state.Playable.IsValid() || state.Playable.GetHandle() != input.GetHandle())
            return false;

        return true;
    }

    public bool ValidateIndex(int index)
    {
        return index >= 0 && index < mStateInfoManager.Count;
    }

    public void ChangeAllStateSpeed(float speed)
    {
        mStateInfoManager.ChangeAllStateSpeed(speed);
    }
    
    public int GetStateCount()
    {
        int count = 0;
        for (int i = 0; i < mStateInfoManager.Count; i++)
            if (mStateInfoManager[i] != null)
                count++;
        return count;
    }
    
    public bool RemoveState(string name)
    {
        BaseStateInfo state = mStateInfoManager.FindState(name);
        if (state == null)
        {
            Debug.LogError($"Cannot remove state with name {name}, because a state with that name doesn't exist");
            return false;
        }

        RemoveClones(state);
        mStateInfoManager.RemoveState(state.Index);
        return true;
    }
    
    void RemoveClones(BaseStateInfo state)
    {
        var it = mStateQueue.First;
        while (it != null)
        {
            var next = it.Next;

            int index = it.Value.Index;
            BaseStateInfo queuedState = mStateInfoManager[index];
            if (queuedState == null)
                return;
            if (queuedState.OriginalIndex == state.Index)
            {
                mStateQueue.Remove(it);
                DoStop(queuedState.Index);
            }

            it = next;
        }
    }
    
    void DoStop(int index)
    {
        BaseStateInfo state = mStateInfoManager[index];
        if (state == null)
            return;

        mStateInfoManager.StopState(index, state.IsClone);
        if (!state.IsClone)
            RemoveClones(state);
    }
    
    public void ClearQueuedStates()
    {
        using (var it = mStateQueue.GetEnumerator())
        {
            while (it.MoveNext())
            {
                QueuedState queuedState = it.Current;
                mStateInfoManager.StopState(queuedState.Index, true);
            }
        }

        mStateQueue.Clear();
    }

    float CalculateQueueTimes()
    {
        float longestTime = -1f;

        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = mStateInfoManager[i];
            //Skip deleted states
            if (state == null || !state.Enabled || !state.Playable.IsValid())
                continue;

            if (state.IsLoop)
                return Mathf.Infinity;

            float speed = state.speed;
            float stateTime = state.GetTime();
            float remainingTime;
            if (speed > 0)
                remainingTime = (state.Length - stateTime) / speed;
            else if (speed < 0)
                remainingTime = (stateTime) / speed;
            else
                remainingTime = Mathf.Infinity;

            if (remainingTime > longestTime)
                longestTime = remainingTime;
        }

        return longestTime;
    }
    
    bool Crossfade(int index, float time)
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = mStateInfoManager[i];
            if (state == null)
                continue;

            if (state.Index == index)
                mStateInfoManager.EnableState(index);

            if (state.Enabled == false)
                continue;

            float targetWeight = state.Index == index ? 1.0f : 0.0f;
            SetupLerp(state, targetWeight, time);
        }

        return true;
    }
    
    void SetupLerp(BaseStateInfo state, float targetWeight, float time)
    {
        float travel = Mathf.Abs(state.Weight - targetWeight);
        float newSpeed = time != 0f ? travel / time : Mathf.Infinity;

        // If we're fading to the same target as before but slower, assume CrossFade was called multiple times and ignore new speed
        if (state.Fading && Mathf.Approximately(state.TargetWeight, targetWeight) && newSpeed < state.FadeSpeed)
            return;

        state.FadeTo(targetWeight, newSpeed);
    }

    void UpdateStoppedPlayablesConnections()
    {
        for (int i = 0; i < mStateInfoManager.Count; i++)
        {
            BaseStateInfo state = mStateInfoManager[i];
            if (state == null)
                continue;
            if (state.Enabled)
                continue;
            
            if (mIsKeepStoppedPlayablesConnected)
                ConnectInput(state.Index);
            else
                DisconnectInput(state.Index);
        }
    }
}
