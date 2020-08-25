using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class BaseGraphController<T, U>  where T : BaseStateInfo, new() where U : BaseStateInfoManager<T>, new()
{
    protected PlayableGraph mGraph;
    string mGraphName;
    bool mInitialized;
    bool mIsPlaying;
    BasePlayable<T, U> mBasePlayable;
    
    public virtual bool IsPlaying => mBasePlayable.IsPlaying();

    protected BaseGraphController(string graphName)
    {
        mGraphName = graphName;
    }
    
    protected void Init()
    {
        if (mInitialized)
            return;
        
        mGraph = PlayableGraph.Create(mGraphName);
        mGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        
        mBasePlayable = CreatePlayable();
        mBasePlayable.onDone += OnPlayableDone;
        
        mInitialized = true;
    }

    protected abstract BasePlayable<T, U> CreatePlayable();

    public void RemoveState(string name)
    {
        mBasePlayable.RemoveState(name);
    }
    
    public bool Play(string stateName)
    {
        Kick();
        return mBasePlayable.Play(stateName);
    }
    
    public void PlayQueued(string stateName, QueueMode queueMode = QueueMode.CompleteOthers)
    {
        Kick();
        mBasePlayable.PlayQueued(stateName, queueMode);
    }
    
    public void StopAll()
    {
        mBasePlayable.StopAll();
    }

    public void Stop(string stateName)
    {
        mBasePlayable.Stop(stateName);
    }

    public void RewindAll()
    {
        Kick();
        mBasePlayable.RewindAll();
    }

    public void Rewind(string stateName)
    {
        Kick();
        mBasePlayable.Rewind(stateName);
    }
    
    public void Blend(string stateName, float targetWeight, float fadeLength)
    {
        Kick();
        mBasePlayable.Blend(stateName, targetWeight, fadeLength);
    }

    public void CrossFade(string stateName, float fadeLength)
    {
        Kick();
        mBasePlayable.Crossfade(stateName, fadeLength);
    }

    public void CrossFadeQueued(string stateName, float fadeLength, QueueMode queueMode)
    {
        Kick();
        mBasePlayable.CrossfadeQueued(stateName, fadeLength, queueMode);
    }
    
    public int GetStateCount()
    {
        return mBasePlayable.GetStateCount();
    }
    
    public bool IsStatePlaying(string stateName)
    {
        return mBasePlayable.IsPlaying(stateName);
    }

    public void ChangeAllStateSpeed(float speed)
    {
        mBasePlayable.ChangeAllStateSpeed(speed);
    }
    
    public void Reset()
    {
        OnDestroy();
        mInitialized = false;
    }
    
    public void OnEnable()
    {
        Init();
        mGraph.Play();
    }

    public void OnDisable()
    {
        if (mInitialized)
        {
            StopAll();
            mGraph.Stop();
        }
    }
    
    void OnPlayableDone()
    {
        mGraph.Stop();
        mIsPlaying = false;
    }

    public void OnDestroy()
    {
        if (mGraph.IsValid())
            mGraph.Destroy();
    }
    
    protected void Kick()
    {
        if (!mIsPlaying)
        {
            mGraph.Play();
            mIsPlaying = true;
        }
    }
}
