using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStateInfo : BaseStateInfo
{
    public void Init(string name, AudioClip audioClip, bool isAudioLoop)
    {
        mStateName = name;
        mClip = audioClip;
        mIsAudioLoop = isAudioLoop;
    }

    // BaseStateHandle<AudioStateInfo, AudioStateInfoManager> mParentState;
    // public BaseStateHandle<AudioStateInfo, AudioStateInfoManager> ParentState => mParentState;
    
    public override bool IsLoop => mIsAudioLoop;
    public override float Length => mClip == null ? 0 : mClip.length;
    
    AudioClip mClip;
    public AudioClip Clip => mClip;

    bool mIsAudioLoop;
    public bool IsAudioLoop => mIsAudioLoop;
    
    // public void SetAsCloneOf(BaseStateHandle<AudioStateInfo, AudioStateInfoManager>  handle)
    // {
    //     mParentState = handle;
    //     mIsClone = true;
    // }
}
