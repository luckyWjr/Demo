using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationClipConfig : MonoBehaviour
{
    public List<AnimationClipItem> List;

    #region AnimationEventReceiver

    public void TestEvent()
    {
        
    }
    
    public void SkillEvent()
    {

    }

    #endregion
}

[Serializable]
public class AnimationClipItem
{
    public AnimationType Type;
    public AnimationClip AnimationClip;
    public WrapMode Mode;
    
    public string Name => Type.ToString();
}

public enum AnimationType
{
    Idle,
    Walk,
    Run,
    Skill1,
    Skill2,
    Skill3,
    Damage,
    Die,
    Ready
}
