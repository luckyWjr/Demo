using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    bool IsValid();

    bool Enabled { get; set; }

    float Time { get; set; }

    float NormalizedTime { get; set; }

    float Speed { get; set; }

    string Name { get; }

    float Weight { get; set; }

    float Length { get; }
}