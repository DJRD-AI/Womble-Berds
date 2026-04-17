using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct BoundVal{
    [field:SerializeField] public float Upper;
    [field:SerializeField] public float Lower;
    public readonly float Difference => Upper-Lower;
    public readonly float Random => UnityEngine.Random.Range(Upper,Lower);

    public BoundVal(float Val1 = 0, float Val2 = 1){
        Upper = math.max(Val1,Val2);
        Lower = math.min(Val1,Val2);
    }
    public BoundVal(int Val1 = 0, int Val2 = 1){
        Upper = math.max(Val1,Val2);
        Lower = math.min(Val1,Val2);
    }
    public BoundVal(double Val1 = 0, double Val2 = 1){
        Upper = math.max((float)Val1,(float)Val2);
        Lower = math.min((float)Val1,(float)Val2);
    }

    public readonly float LerpUnclamped(float Place = 0.0f){
        return Lower + (Upper-Lower)*Place;
    }
    public readonly float Lerp(float Place = 0.0f){
        Place = Mathf.Clamp(Place,0,1);
        return Lower + (Upper-Lower)*Place;
    }
    public readonly float Clamp(float value) => Mathf.Clamp(value,Lower,Upper);
    public readonly bool InBounds(float value) => Lower <= value && value <= Upper;
}

[Serializable]
public class BoundPos{
    [field:SerializeField] public BoundVal HOR{get;private set;} = new();
    [field:SerializeField] public BoundVal VER{get;private set;} = new();
    [field:SerializeField] public bool PosDraw{get;private set;} = new();
    public float LEFT => HOR.Lower;
    public float RIGHT {get => HOR.Upper;}
    public float UP {get => VER.Upper;}
    public float DOWN {get => VER.Lower;}
    public Vector2 UPLEFT    => new(LEFT,UP);
    public Vector2 UPRIGHT   => new(RIGHT,UP);
    public Vector2 DOWNLEFT  => new(LEFT,DOWN);
    public Vector2 DOWNRIGHT => new(RIGHT,DOWN);
    public Vector2 Random2 => new(HOR.Random,VER.Random);
    public Vector3 Random3
    {
        get
        {
            float verpos = VER.Random;
            return new(HOR.Random,verpos,verpos);
        }
    }
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
    public BoundPos(BoundVal HOR, BoundVal VER)
    {
        this.HOR = HOR;
        this.VER = VER;
    }
    public Vector2 Clamp(Vector2 value)
    {
        value.x = HOR.Clamp(value.x);
        value.y = VER.Clamp(value.y);
        return value;
    }
    public Vector3 Clamp(Vector3 value)
    {
        value.x = HOR.Clamp(value.x);
        value.y = VER.Clamp(value.y);
        value.z = VER.Clamp(value.z);
        return value;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BoundPos))]
public class BoundPosDrawer: PropertyDrawer {
    protected const float LineSpacer = 2f;
    protected const float ActorSpacer = 1f;
    protected float lineHeight = EditorGUIUtility.singleLineHeight + LineSpacer;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect;
        if (!property.isExpanded) {
            foldoutRect = new(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect,property.isExpanded,label,true);
            EditorGUI.EndProperty();
            return;
        }

        SerializedProperty positionalProp = property.FindPropertyRelative("<PosDraw>k__BackingField");

        foldoutRect = new(position.x, position.y, position.width/2, lineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect,property.isExpanded,label,true);
        Rect ButtonRect = new(position.x+position.width/2, position.y, position.width/2, lineHeight);
        if(GUI.Button(ButtonRect, "Switch Draw Style")){
            positionalProp.boolValue = !positionalProp.boolValue;
        }

        position.y += lineHeight;

        if(positionalProp.boolValue)
            DrawPositional(position,property);
        else{
            position.y += DrawRegular(position,property, "VER", "Upper");
            position.y += DrawRegular(position,property, "VER", "Lower");
            position.y += DrawRegular(position,property, "HOR", "Upper");
            position.y += DrawRegular(position,property, "HOR", "Lower");
        }

        EditorGUI.EndProperty();
    }

    private float DrawRegular(Rect position, SerializedProperty property, string axis, string which)
    {
        Rect Drawrect = new(position.x,position.y,position.width,lineHeight);
        SerializedProperty work = property.FindPropertyRelative($"<{axis}>k__BackingField.{which}");
        string ValueLabel = (axis,which) switch
        {
            ("VER", "Upper") => "upper",
            ("VER", "Lower") => "lower",
            ("HOR", "Upper") => "right",
            ("HOR", "Lower") => "left",
            _ => null
        };
        if(ValueLabel == null)
            return 0;
        work.floatValue = EditorGUI.FloatField(Drawrect, new GUIContent(ValueLabel), work.floatValue);
        return lineHeight;
    }

    private void DrawPositional(Rect position, SerializedProperty property){
        Rect full = new (position.x,position.y,position.width,lineHeight*3);
        SerializedProperty Work;
        Work = property.FindPropertyRelative("<VER>k__BackingField.Upper");
        Work.floatValue = DrawFloatSegment(full,Work.floatValue,3,3,1,0);

        Work = property.FindPropertyRelative("<VER>k__BackingField.Lower");
        Work.floatValue = DrawFloatSegment(full,Work.floatValue,3,3,1,2);

        Work = property.FindPropertyRelative("<HOR>k__BackingField.Upper");
        Work.floatValue = DrawFloatSegment(full,Work.floatValue,3,3,0,1);

        Work = property.FindPropertyRelative("<HOR>k__BackingField.Lower");
        Work.floatValue = DrawFloatSegment(full,Work.floatValue,3,3,2,1);
        position.y += 3*lineHeight;
    }
    private float DrawFloatSegment(Rect full, float val, int XCount, int YCount, int Xpos = 0, int Ypos = 0){
        XCount = math.max(1,XCount);
        YCount = math.max(1,YCount);
        Xpos = math.clamp(Xpos,0,XCount-1);
        Ypos = math.clamp(Ypos,0,YCount-1);
        Vector2 Size = new(
            full.width/XCount,
            full.height/YCount
        ); 
        Vector2 StartPos = new(
            full.x + Size.x * Xpos,
            full.y + Size.y * Ypos
        );
        Rect work = new(StartPos,Size);
        return EditorGUI.FloatField(work, new GUIContent(), val);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if(!property.isExpanded)
            return lineHeight;
        int multi = 4;
        SerializedProperty positionalProp = property.FindPropertyRelative("<PosDraw>k__BackingField");
        if(!positionalProp.boolValue)
            multi += 1;
        return multi*lineHeight;
    }
}
#endif