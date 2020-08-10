using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    protected bool mActived;
    public bool Active
    {
        get { return mActived; }
        set
        {
            mActived = value;
        }
    }

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {

    }


    protected virtual void Update()
    {

    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void OnDestroy()
    {

    }
}