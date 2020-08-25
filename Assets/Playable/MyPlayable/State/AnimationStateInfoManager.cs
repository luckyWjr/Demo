using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateInfoManager : BaseStateInfoManager<AnimationStateInfo>
{
    protected override bool IsCloneOf(int potentialCloneIndex, int originalIndex)
    {
        AnimationStateInfo potentialClone = mStateInfoList[potentialCloneIndex];
        // return potentialClone.IsClone && potentialClone.ParentState.index == originalIndex;
        return potentialClone.IsClone && potentialClone.OriginalIndex == originalIndex;
    }
    
    public bool RemoveClip(AnimationClip clip)
    {
        bool removed = false;
        for (int i = 0; i < mStateInfoList.Count; i++)
        {
            AnimationStateInfo state = mStateInfoList[i];
            if (state != null && state.Clip == clip)
            {
                RemoveState(i);
                removed = true;
            }
        }

        return removed;
    }
    
    public override float GetStateLength(int index)
    {
        AnimationClip clip = mStateInfoList[index].Clip;
        if (clip == null)
            return 0f;
        float speed = mStateInfoList[index].speed;
        if (speed == 0f)
            return Mathf.Infinity;

        return clip.length / speed;
    }
    
    public override float GetClipLength(int index)
    {
        AnimationClip clip = mStateInfoList[index].Clip;
        if (clip == null)
            return 0f;

        return clip.length;
    }
    
    public AnimationClip GetStateClip(int index)
    {
        return mStateInfoList[index].Clip;
    }

    public WrapMode GetStateWrapMode(int index)
    {
        return mStateInfoList[index].WrapMode;
    }
}
