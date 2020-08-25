using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AnimationSimplePlayable : BasePlayable<AnimationStateInfo, AnimationStateInfoManager>
{
    AnimationStateInfoManager mAnimationStateInfoManager;
    public AnimationStateInfoManager AnimationStateInfoManager => mAnimationStateInfoManager;

    public AnimationSimplePlayable() : base()
    {
        mAnimationStateInfoManager = mStateInfoManager;
    }
    
    protected override void CreateMixerPlayable()
    {
        mMixerPlayable = AnimationMixerPlayable.Create(mGraph, 1, true);
    }
    
    AnimationStateInfo DoAddState(string name, AnimationClip clip, Dictionary<string, Action<object>> eventDic = null)
    {
        AnimationStateInfo newState = mStateInfoManager.InsertState();
        newState.Init(name, clip, clip.wrapMode, eventDic);
        
        int index = newState.Index;
        if (index == mMixerPlayable.GetInputCount())
            mMixerPlayable.SetInputCount(index + 1);

        var clipPlayable = AnimationClipPlayable.Create(mGraph, clip);
        clipPlayable.SetApplyFootIK(false);
        clipPlayable.SetApplyPlayableIK(false);
        if (!clip.isLooping || newState.WrapMode == WrapMode.Once)
            clipPlayable.SetDuration(clip.length);
        
        newState.SetPlayable(clipPlayable);
        newState.Pause();

        if (mIsKeepStoppedPlayablesConnected)
            ConnectInput(newState.Index);
        
        return newState;
    }

    public AnimationStateInfo AddState(string name, AnimationClip clip, Dictionary<string, Action<object>> eventDic = null)
    {
        if (mStateInfoManager.FindState(name) != null)
        {
            Debug.LogError($"Cannot add state with name {name}, because a state with that name already exists");
            return null;
        }

        AnimationStateInfo info = DoAddState(name, clip, eventDic);
        UpdateDoneStatus();

        return info;
    }

    public bool RemoveState(AnimationClip clip)
    {
        return mAnimationStateInfoManager.RemoveClip(clip);
    }
    
    protected override BaseStateInfo CloneState(int index)
    {
        AnimationStateInfo original = mAnimationStateInfoManager[index];
        string newName = original.StateName + "Queued Clone";
        AnimationStateInfo clone = DoAddState(newName, original.Clip);
        clone.Clone(original);
        // clone.SetAsCloneOf(CreateStateHandle(original.Index, original.Playable));
        return clone;
    }
    
    // protected override int GetStateInfoParentIndex(int index)
    // {
    //     AnimationStateInfo otherState = mStateInfoManager[index];
    //     return otherState.ParentState.index;
    // }
    
    protected override BaseStateHandle<AnimationStateInfo, AnimationStateInfoManager> BaseStateInfoToHandle(BaseStateInfo info)
    {
        return new AnimationStateHandle(this, info.Index, info.Playable);
    }
    
    protected override BaseStateHandle<AnimationStateInfo, AnimationStateInfoManager> CreateStateHandle(int index, Playable playable)
    {
        return new AnimationStateHandle(this, index, playable);
    }
}
