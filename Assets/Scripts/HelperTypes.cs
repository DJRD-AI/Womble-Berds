using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct BoundValues{
    private readonly float _upper;
    private readonly float _lower;
    public readonly float Upper => _upper;
    public readonly float Lower => _lower;
    public readonly float Difference => _upper-_lower;

    public BoundValues(float Val1 = 0, float Val2 = 1)
    {
        _upper = math.max(Val1,Val2);
        _lower = math.min(Val1,Val2);
    }
    public BoundValues(int Val1 = 0, int Val2 = 1)
    {
        _upper = math.max(Val1,Val2);
        _lower = math.min(Val1,Val2);
    }
    public BoundValues(double Val1 = 0, double Val2 = 1){
        _upper = math.max((float)Val1,(float)Val2);
        _lower = math.min((float)Val1,(float)Val2);
    }

    public readonly float LerpUnclamped(float Place = 0.0f){
        return Lower + (Upper-Lower)*Place;
    }
    public readonly float Lerp(float Place = 0.0f){
        Place = Mathf.Clamp(Place,0,1);
        return Lower + (Upper-Lower)*Place;
    }
    public readonly float Clamp(float value) => Mathf.Clamp(value,Lower,Upper);
    public readonly float Random => UnityEngine.Random.Range(Upper,Lower);
}
