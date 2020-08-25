using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStateInfoManager : BaseStateInfoManager<AudioStateInfo>
{
    protected override bool IsCloneOf(int potentialCloneIndex, int originalIndex)
    {
        AudioStateInfo potentialClone = mStateInfoList[potentialCloneIndex];
        return potentialClone.IsClone && potentialClone.OriginalIndex == originalIndex;
    }
    
    public bool RemoveClip(AudioClip clip)
    {
        bool removed = false;
        for (int i = 0; i < mStateInfoList.Count; i++)
        {
            AudioStateInfo state = mStateInfoList[i];
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
        AudioClip clip = mStateInfoList[index].Clip;
        if (clip == null)
            return 0f;
        float speed = mStateInfoList[index].speed;
        if (speed == 0f)
            return Mathf.Infinity;

        return clip.length / speed;
    }
    
    public override float GetClipLength(int index)
    {
        AudioClip clip = mStateInfoList[index].Clip;
        if (clip == null)
            return 0f;

        return clip.length;
    }
    
    public AudioClip GetStateClip(int index)
    {
        return mStateInfoList[index].Clip;
    }
    
    public bool GetIsAudioLoop(int index)
    {
        return mStateInfoList[index].IsAudioLoop;
    }
}
