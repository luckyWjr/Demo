using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityComponent
{
    void Awake();
    void Start();
    void Update(float deltaTime);
    void FixedUpdate();
}

