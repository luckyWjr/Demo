using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PlayableTest : MonoBehaviour
{
    public GameObject Player;
    public AudioSource BGAudioSource;
    public AudioSource SkillAudioSource;

    public AudioClip BGClip;
    public AudioClip AttackClip;

    public Button Skill1;
    public Button Skill2;
    public Button Skill3;
    public Button Auto;
    public Toggle SpeedToggle;
    
    AnimationGraphController mAnimationGraphController;
    AudioGraphController mBGAudioGraphController;
    AudioGraphController mSkillAudioGraphController;

    const string BGClipName = "BGClip";
    const string AttackClipName = "AttackClip";

    bool isCreate = false;
    
    void Start()
    {
        CreateAnimationGraph();
        CreateAudioGraph();
        
        Skill1.onClick.AddListener(Skill1OnClicked);
        Skill2.onClick.AddListener(Skill2OnClicked);
        Skill3.onClick.AddListener(Skill3OnClicked);
        Auto.onClick.AddListener(AutoOnClicked);
        SpeedToggle.onValueChanged.AddListener(OnSpeedChange);
    }

    void OnValidate()
    {
        if(isCreate) return;
        // CreateAnimationGraph();
        // CreateAudioGraph();
        isCreate = true;
    }

    void CreateAnimationGraph()
    {
        AnimationClipConfig config = Player.GetComponent<AnimationClipConfig>();
        mAnimationGraphController = new AnimationGraphController("TestAnimation", Player.GetComponent<Animator>());

        Dictionary<string, Action<object>> dic = new Dictionary<string, Action<object>>
        {
            ["TestEvent"] = PlaySkillAudio
        };

        foreach (var clipItem in config.List)
        {
            if (clipItem.Type == AnimationType.Skill1)
                mAnimationGraphController.AddState(clipItem.Name, clipItem.AnimationClip, dic);
            else if (clipItem.Type == AnimationType.Skill2)
            {
                AnimationStateInfo stateInfo = mAnimationGraphController.AddState(clipItem.Name, clipItem.AnimationClip);
                stateInfo.AddEvent(0.1f, Skill2Handle, "qwerty");
                stateInfo.OnStopCallback = Skill2Stop;
            }
            else
                mAnimationGraphController.AddState(clipItem.Name, clipItem.AnimationClip);
        }
        
        mAnimationGraphController.Play(AnimationType.Idle.ToString());
    }

    void PlaySkillAudio(object data)
    {
        AnimationEventData eventData = (AnimationEventData)data;
        Debug.Log("call TestEvent:" + eventData.StringData);
        mSkillAudioGraphController.Play(AttackClipName);
    }

    void Skill2Handle(object data)
    {
        Debug.Log("Skill2Handle:" + data);
    }
    
    void Skill2Stop()
    {
        Debug.Log("Skill2Stop:");
    }
    
    void CreateAudioGraph()
    {
        mBGAudioGraphController = new AudioGraphController("TestBGAudio", BGAudioSource) {Volume = 0.1f};
        mBGAudioGraphController.AddState(BGClipName, BGClip, true);
        mBGAudioGraphController.Play(BGClipName);
        
        mSkillAudioGraphController = new AudioGraphController("TestSkillAudio", SkillAudioSource);
        mSkillAudioGraphController.AddState(AttackClipName, AttackClip, false);
    }

    void Skill1OnClicked()
    {
        mAnimationGraphController.Play(AnimationType.Skill1.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Idle.ToString());
    }
    
    void Skill2OnClicked()
    {
        mAnimationGraphController.Play(AnimationType.Skill2.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Idle.ToString());
        
        mSkillAudioGraphController.Play(AttackClipName);
    }
    
    void Skill3OnClicked()
    {
        mAnimationGraphController.Play(AnimationType.Skill3.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Idle.ToString());
        
        // mPlayerSimpleAnimation.Play(AnimationType.Run.ToString());
        // mPlayerSimpleAnimation.Blend(AnimationType.Skill3.ToString(), 1, 5);
        mSkillAudioGraphController.Play(AttackClipName);
    }
    
    void AutoOnClicked()
    {
        mAnimationGraphController.Play(AnimationType.Skill1.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill2.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill3.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill1.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill2.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill3.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill1.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill2.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Skill3.ToString());
        mAnimationGraphController.PlayQueued(AnimationType.Idle.ToString());
    }
    
    void OnSpeedChange(bool isSelected)
    {
        mAnimationGraphController.ChangeAllStateSpeed(isSelected ? 2 : 1);
    }

    void OnApplicationQuit()
    {
        mAnimationGraphController.OnDestroy();
        mBGAudioGraphController.OnDestroy();
        mSkillAudioGraphController.OnDestroy();
    }
}
