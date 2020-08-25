using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class AnimationStateInfo : BaseStateInfo
{
    // BaseStateHandle<AnimationStateInfo, AnimationStateInfoManager> mParentState;
    // public BaseStateHandle<AnimationStateInfo, AnimationStateInfoManager> ParentState => mParentState;
    
    public override bool IsLoop => mWrapMode == WrapMode.Loop;
    public override float Length => mClip == null ? 0 : mClip.length;

    AnimationClip mClip;
    public AnimationClip Clip => mClip;
    
    WrapMode mWrapMode;
    public WrapMode WrapMode => mWrapMode;
    
    public void Init(string name, AnimationClip animationClip, WrapMode wrapMode, Dictionary<string, Action<object>> eventHandleDic = null)
    {
        mStateName = name;
        mClip = animationClip;
        mWrapMode = wrapMode;

        if (animationClip.events != null && eventHandleDic != null)
            foreach (var e in animationClip.events)
            {
                if (eventHandleDic.ContainsKey(e.functionName))
                {
                    AnimationEventData data = new AnimationEventData
                    {
                        FloatData = e.floatParameter,
                        IntData = e.intParameter,
                        StringData = e.stringParameter,
                        ObjectData = e.objectReferenceParameter
                    };
                    AddEvent(e.time, eventHandleDic[e.functionName], data);
                }
            }
    }
    
    // public void SetAsCloneOf(BaseStateHandle<AnimationStateInfo, AnimationStateInfoManager>  handle)
    // {
    //     mParentState = handle;
    //     mIsClone = true;
    // }
}

public struct AnimationEventData
{
    public float FloatData;
    public int IntData;
    public string StringData;
    public Object ObjectData;
}
