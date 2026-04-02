using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public readonly struct BoundVal{
    private readonly float _upper;
    private readonly float _lower;
    public readonly float Upper => _upper;
    public readonly float Lower => _lower;
    public readonly float Difference => _upper-_lower;

    public BoundVal(float Val1 = 0, float Val2 = 1){
        _upper = math.max(Val1,Val2);
        _lower = math.min(Val1,Val2);
    }
    public BoundVal(int Val1 = 0, int Val2 = 1){
        _upper = math.max(Val1,Val2);
        _lower = math.min(Val1,Val2);
    }
    public BoundVal(double Val1 = 0, double Val2 = 1){
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
    public readonly bool InBounds(float value) => _lower <= value && value <= _upper; 
    public readonly float Random => UnityEngine.Random.Range(Upper,Lower);
}

public class BoundPos{
    public readonly BoundVal HOR = new(-7.8,8.5);
    public readonly BoundVal VER = new(-4.4f,-3.0f);
    public float LEFT {get => HOR.Lower;}
    public float RIGHT {get => HOR.Upper;}
    public float UP {get => VER.Upper;}
    public float DOWN {get => VER.Lower;}

    public BoundPos(float Val1 = 0, float Val2 = 1, float Val3 = 0, float Val4 = 1)
    {
        HOR = new(Val1,Val2);
        VER = new(Val3,Val4);
    }
    public BoundPos(int Val1 = 0, int Val2 = 1, int Val3 = 0, int Val4 = 1)
    {
        HOR = new(Val1,Val2);
        VER = new(Val3,Val4);
    }
    public BoundPos(double Val1 = 0, double Val2 = 1, double Val3 = 0, double Val4 = 1){
        HOR = new(Val1,Val2);
        VER = new(Val3,Val4);
    }
    public BoundPos(BoundVal HOR, float val1 = 0, float val2 = 1)
    {
        this.HOR = HOR;
        VER = new (val1,val2);
    }
    public BoundPos(BoundVal HOR, int val1 = 0, int val2 = 1)
    {
        this.HOR = HOR;
        VER = new (val1,val2);
    }
    public BoundPos(BoundVal HOR, double val1 = 0, double val2 = 1)
    {
        this.HOR = HOR;
        VER = new (val1,val2);
    }
}
