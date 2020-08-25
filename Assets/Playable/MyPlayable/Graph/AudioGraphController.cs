using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AudioGraphController : BaseGraphController<AudioStateInfo, AudioStateInfoManager>
{
    AudioSource mAudioSource;

    AudioSimplePlayable mSimplePlayable;
    public AudioSimplePlayable SimplePlayable => mSimplePlayable;
    
    float mVolume = 1;
    public float Volume
    {
        get => mVolume;
        set
        {
            mVolume = Mathf.Clamp01(value);
            mAudioSource.volume = mVolume;
        }
    }


    public AudioGraphController(string graphName, AudioSource audioSource) : base(graphName)
    {
        mAudioSource = audioSource;
        Init();
    }

    protected override BasePlayable<AudioStateInfo, AudioStateInfoManager> CreatePlayable()
    {
        AudioSimplePlayable template = new AudioSimplePlayable();
        var playable = ScriptPlayable<AudioSimplePlayable>.Create(mGraph, template, 1);
        mSimplePlayable = playable.GetBehaviour();
        AudioPlayableOutput output = AudioPlayableOutput.Create(mGraph, "AudioOutput", mAudioSource);
        output.SetSourcePlayable(playable);
        return mSimplePlayable;
    }

    public AudioStateInfo AddState(string name, AudioClip animClip, bool isLoop)
    {
        return mSimplePlayable.AddState(name, animClip, isLoop);
    }

    public void RemoveClip(AudioClip clip)
    {
        if (clip == null)
            throw new System.NullReferenceException("clip");

        mSimplePlayable.RemoveState(clip);
    }
}
