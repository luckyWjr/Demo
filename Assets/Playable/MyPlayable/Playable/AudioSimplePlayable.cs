using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AudioSimplePlayable : BasePlayable<AudioStateInfo, AudioStateInfoManager>
{
    AudioStateInfoManager mAudioStateInfoManager;
    public AudioStateInfoManager AudioStateInfoManager => mAudioStateInfoManager;
    
    public AudioSimplePlayable() : base()
    {
        mAudioStateInfoManager = mStateInfoManager;
    }
    
    protected override void CreateMixerPlayable()
    {
        mMixerPlayable = AudioMixerPlayable.Create(mGraph, 1, true);
    }
    
    AudioStateInfo DoAddState(string name, AudioClip clip, bool isLoop)
    {
        AudioStateInfo newState = mAudioStateInfoManager.InsertState();
        newState.Init(name, clip, isLoop);
        
        int index = newState.Index;
        if (index == mMixerPlayable.GetInputCount())
            mMixerPlayable.SetInputCount(index + 1);

        var clipPlayable = AudioClipPlayable.Create(mGraph, clip, isLoop);
        if (!isLoop)
            clipPlayable.SetDuration(clip.length);
        
        newState.SetPlayable(clipPlayable);
        newState.Pause();

        if (mIsKeepStoppedPlayablesConnected)
            ConnectInput(newState.Index);
        
        return newState;
    }
    
    public AudioStateInfo AddState(string name, AudioClip clip, bool isLoop)
    {
        if (mAudioStateInfoManager.FindState(name) != null)
        {
            Debug.LogError($"Cannot add state with name {name}, because a state with that name already exists");
            return null;
        }

        AudioStateInfo info = DoAddState(name, clip, isLoop);
        UpdateDoneStatus();
        
        return info;
    }
    
    public bool RemoveState(AudioClip clip)
    {
        return mAudioStateInfoManager.RemoveClip(clip);
    }
    
    protected override BaseStateInfo CloneState(int index)
    {
        AudioStateInfo original = mAudioStateInfoManager[index];
        string newName = original.StateName + "Queued Clone";
        AudioStateInfo clone = DoAddState(newName, original.Clip, original.IsAudioLoop);
        clone.Clone(original);
        // clone.SetAsCloneOf(CreateStateHandle(original.Index, original.Playable));
        return clone;
    }
    
    // protected override int GetStateInfoParentIndex(int index)
    // {
    //     AudioStateInfo otherState = mStateInfoManager[index];
    //     return otherState.ParentState.index;
    // }
    
    protected override BaseStateHandle<AudioStateInfo, AudioStateInfoManager> BaseStateInfoToHandle(BaseStateInfo info)
    {
        return new AudioStateHandle(this, info.Index, info.Playable);
    }
    
    protected override BaseStateHandle<AudioStateInfo, AudioStateInfoManager> CreateStateHandle(int index, Playable playable)
    {
        return new AudioStateHandle(this, index, playable);
    }
}
