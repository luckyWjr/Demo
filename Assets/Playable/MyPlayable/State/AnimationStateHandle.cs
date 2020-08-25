using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AnimationStateHandle : BaseStateHandle<AnimationStateInfo, AnimationStateInfoManager>
{
    AnimationSimplePlayable mAnimationSimplePlayable;
    public AnimationStateHandle(AnimationSimplePlayable playable, int index, Playable target) : base(playable, index, target)
    {
        mAnimationSimplePlayable = playable;
    }

    public AnimationClip clip
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mAnimationSimplePlayable.AnimationStateInfoManager.GetStateClip(mIndex);
        }
    }
    
    public WrapMode wrapMode
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mAnimationSimplePlayable.AnimationStateInfoManager.GetStateWrapMode(mIndex);
        }
    }
}
