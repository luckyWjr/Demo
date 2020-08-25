using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AudioStateHandle : BaseStateHandle<AudioStateInfo, AudioStateInfoManager>
{
    AudioSimplePlayable mAudioSimplePlayable;
    public AudioStateHandle(AudioSimplePlayable playable, int index, Playable target) : base(playable, index, target)
    {
        mAudioSimplePlayable = playable;
    }

    public AudioClip audioClip
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mAudioSimplePlayable.AudioStateInfoManager.GetStateClip(mIndex);
        }
    }
    
    public bool isAudioLoop
    {
        get
        {
            if (!IsValid())
                throw new System.InvalidOperationException("This StateHandle is not valid");
            return mAudioSimplePlayable.AudioStateInfoManager.GetIsAudioLoop(mIndex);
        }
    }
}
