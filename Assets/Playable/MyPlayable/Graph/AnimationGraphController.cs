using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AnimationGraphController : BaseGraphController<AnimationStateInfo, AnimationStateInfoManager>
{
    Animator mAnimator;
    
    AnimationSimplePlayable mSimplePlayable;
    public AnimationSimplePlayable SimplePlayable => mSimplePlayable;

    bool mAnimatePhysics = false;
    public bool AnimatePhysics
    {
        get => mAnimatePhysics;
        set
        {
            mAnimatePhysics = value;
            mAnimator.updateMode = mAnimatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
        }
    }

    AnimatorCullingMode mCullingMode = AnimatorCullingMode.CullUpdateTransforms;
    public AnimatorCullingMode CullingMode
    {
        get => mCullingMode;
        set
        {
            mCullingMode = value;
            mAnimator.cullingMode = mCullingMode;
        }
    }

    public AnimationGraphController(string graphName, Animator animator) : base(graphName)
    {
        mAnimator = animator;
        Init();
    }
    
    protected override BasePlayable<AnimationStateInfo, AnimationStateInfoManager> CreatePlayable()
    {
        AnimationSimplePlayable template = new AnimationSimplePlayable();
        var playable = ScriptPlayable<AnimationSimplePlayable>.Create(mGraph, template, 1);
        mSimplePlayable = playable.GetBehaviour();
        AnimationPlayableUtilities.Play(mAnimator, mSimplePlayable.SelfPlayable, mGraph);
        return mSimplePlayable;
    }

    public AnimationStateInfo AddState(string name, AnimationClip animClip, Dictionary<string, Action<object>> eventDic = null)
    {
        LegacyClipCheck(animClip);
        return mSimplePlayable.AddState(name, animClip, eventDic);
    }

    public void RemoveState(AnimationClip clip)
    {
        if (clip == null)
            throw new System.NullReferenceException("clip");

        mSimplePlayable.RemoveState(clip);
    }
    
    public bool Play(AnimationType animationType)
    {
        Kick();
        return mSimplePlayable.Play(animationType.ToString());
    }

    public void Play(AnimationType animationType, AnimationType queuedAnimationType)
    {
        Play(animationType);
        PlayQueued(queuedAnimationType.ToString(), QueueMode.CompleteOthers);
    }
    static void LegacyClipCheck(AnimationClip clip)
    {
        if (clip && clip.legacy)
            throw new ArgumentException(string.Format("Legacy clip {0} cannot be used in this component. Set .legacy property to false before using this clip", clip));
    }

    void InvalidLegacyClipError(string clipName, string stateName)
    {
        Debug.LogErrorFormat("Animation clip {0} in state {1} is Legacy. Set clip.legacy to false, or reimport as Generic to use it with SimpleAnimationComponent", clipName, stateName);
    }
}
