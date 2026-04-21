using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class Berd : MonoBehaviour
{
    #region constants
    const float QUACKCOOLDOWN = 15.0f;
    private static readonly BoundVal SCALES      = new(0.3f,0.8f);
    private static readonly BoundVal SPEED       = new(0f, 10.0f);
    private static readonly BoundVal QUACKPITCH  = new(0.5f,1.5f);
    private static readonly BoundVal QUACKVOL    = new(0.5f,2f);
    private static readonly BoundVal RANDOMEVENT = new(15.0f,45.0f);
    #endregion
    private bool QuackTired = false;
    [field:SerializeField] public List<Transform> SpecialPositions{get;private set;}
    private Vector2 _localPosition = Vector2.zero;
    [
        SerializeField,
        Tooltip("The scale within the set bounds\n0=min size, 1=maxSize"),
        Range(0.0f,1.0f)
    ] private float scale = 1.0f;

    [SerializeField,Tooltip("Use this to make sure all the berds are of roughly equal size\nEXCEPT AZY. she be bigger :P")]
    private float ScaleMod = 1f;
    [SerializeField] private AudioClip quack;
    [SerializeField] private AudioClip quackSpecial;
    [SerializeField,Range(0.0f,1.0f)] private float specialChance = 0.1f;
    [SerializeField] private AudioSource quackHole;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] float WalkCycleTime;
    [SerializeField] List<Sprite> WalkCycle;
    private Coroutine Movement = null;
    private Coroutine Scaling = null;
    public bool IsDragging{get; private set;} = false;
    DateTime RandomEvent;

    private void Reset() {
        quackHole = GetComponent<AudioSource>();
    }

    void Awake(){
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
    }

    void OnEnable(){
        StartCoroutine(Walking());
        StartCoroutine(RandomEvents());
    }

    void OnDisable(){
        StopAllCoroutines();
    }

    void OnDestroy(){
        StopAllCoroutines();
    }

    void Start(){
        quackHole.clip = quack;
        transform.localScale = SCALES.Lerp(scale) * ScaleMod * Vector3.one;
        StartCoroutine(Spawning());
    }

    public void ResetBerd(){
        if(Movement != null)
            StopCoroutine(Movement);
        if(Scaling != null)
            StopCoroutine(Scaling);
        transform.localPosition = _localPosition;
        transform.localScale = scale * ScaleMod * Vector3.one;
    }

    IEnumerator Spawning(){
        float verPos = BerdInterface.SPAWNBOUND.VER.Random;
        transform.position = BerdInterface.SPAWNBOUND.Random3;
        _localPosition = BerdInterface.WALKBOUND.Random3;
        QuackTired = true;
        PlayQuack();
        MoveTo(_localPosition, UnityEngine.Random.Range(1,4));
        yield return Movement;
        PlayQuack();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        PlayQuack();
        yield return new WaitForSeconds(QUACKCOOLDOWN);
        QuackTired = false;
    }

    public Coroutine DespawnBerd() => StartCoroutine(Despawning());
    IEnumerator Despawning(){
        Vector2 MoveSpot = BerdInterface.DESPAWNBOUND.Random2;
        PlayQuack();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        PlayQuack();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        MoveTo(MoveSpot,UnityEngine.Random.Range(1,4),false);
        yield return Movement;
        Destroy(gameObject);
    }

    IEnumerator Walking(){
        int index = 0;
        float delay = WalkCycleTime/WalkCycle.Count;
        while (true){
            yield return new WaitForSeconds(delay);
            index = (index+1)%WalkCycle.Count;
            spriteRenderer.sprite = WalkCycle[index];
        }
    }

    IEnumerator RandomEvents(){
        while (true){
            yield return new WaitForSeconds((float)(RandomEvent - DateTime.Now).TotalSeconds);
            if(RandomEvent > DateTime.Now)
                continue;
            RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
            switch (UnityEngine.Random.Range(0f, 1f)){
                case < 0.2f:
                    Vector2 Direction = 
                        new(
                            UnityEngine.Random.Range(0f,1f),
                            UnityEngine.Random.Range(0f,1f)
                        );
                    Direction = Direction.normalized * UnityEngine.Random.Range(0.5f,2f);
                    MoveDir(Direction,SPEED.Random);
                    break;
                case < 0.4f:
                    Vector2 EndPos = BerdInterface.WALKBOUND.Random2;
                    MoveTo(EndPos,SPEED.Random);
                    break;
                case < 0.6f:
                    Scale(SCALES.Random,SPEED.Random);
                    break;
                case < 0.8f:
                    Quack();
                    break;
                default:
                    break;
            }
        }
    }

    private void PlayQuack() => PlayQuack(specialChance);
    private void PlayQuack(float SpecialOdds){
        if(quackSpecial != null)
            quackHole.clip = UnityEngine.Random.Range(0.0f,1.0f) > SpecialOdds ? quack : quackSpecial;
        quackHole.Play();
    }

    public void Quack(float pitch = 1.0f, float volume  = 1.0f,bool clampPitch = true, bool VolClamp = true) => StartCoroutine(Quacking(volume,pitch,clampPitch,VolClamp));
    public IEnumerator Quacking(float pitch = 1.0f, float volume = 1.0f,bool clampPitch = true, bool VolClamp = true){
        if(QuackTired)
            yield break;
        if(clampPitch)
            pitch = QUACKPITCH.Clamp(pitch);
        if(VolClamp)
            volume = QUACKVOL.Clamp(pitch);
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        quackHole.pitch = pitch;
        quackHole.volume = volume;
        PlayQuack();
        if(!BerdInterface.EnableQuackCooldown)
            yield break;
        QuackTired = true;
        yield return new WaitForSeconds(QUACKCOOLDOWN);
        QuackTired = false;
    }

    private float SpeedCheck(float speed, bool clamp = true)=> SpeedCheck(speed,SPEED,clamp);
    private float SpeedCheck(float speed, BoundVal bounds, bool clamp = true){
        if(clamp) speed = bounds.Clamp(speed);
        if(speed == 0) return bounds.Lower;
        return Mathf.Abs(speed);
    }

    public float MoveTo(Vector3 End, float speed = 1, bool PosClamp = true, bool SpeedClamp = true){
        if(PosClamp)
            End = BerdInterface.WALKBOUND.Clamp(End);
        End.z = End.y;
        speed = SpeedCheck(speed,SpeedClamp);

        float Duration = (End-transform.localPosition).magnitude/speed;
        if(Movement != null)
            StopCoroutine(Movement);

        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Movement = StartCoroutine(transform.AnimatingLocalPos(End,Duration));
        return Duration;
    }
    public float MoveTo (Vector2 End, float speed = 1, bool PosClamp = true, bool SpeedClamp = true)   => MoveTo(new Vector3(End.x,End.y,End.y), speed, PosClamp, SpeedClamp);
    public float MoveTo (Transform End, float speed = 1, bool PosClamp = true, bool SpeedClamp = true) => MoveTo(End.position, speed, PosClamp, SpeedClamp);
    public float MoveDir(Vector3 Dir, float speed = 1, bool PosClamp = true, bool SpeedClamp = true)   => MoveTo(Dir + transform.localPosition,  speed, PosClamp, SpeedClamp);
    public float MoveDir(Vector2 Dir, float speed = 1, bool PosClamp = true, bool SpeedClamp = true)   => MoveDir(new Vector3(Dir.x,Dir.y,Dir.y),speed, PosClamp, SpeedClamp);

    public float TryFollow(Berd ToFollow, string where = "", float speed = 1, bool PosClamp = true, bool SpeedClamp = true){
        if(ToFollow == null)
            return -1;
        if(where.StartsWith("!"))
            where = where[1..];
        Transform endPos = ToFollow.SpecialPositions.FirstOrDefault(pos => string.Equals(pos.name, where, StringComparison.OrdinalIgnoreCase));
        if(endPos == null)
            return -1;
        float duration = MoveTo(endPos,speed,PosClamp,SpeedClamp);
        Scale(endPos,duration);
        return duration;
    }
    public float TryFollow(GameObject ToFollow, string where = "", float speed = 1, bool PosClamp = true, bool SpeedClamp = true){
        if(ToFollow == null)
            return -1;
        Berd berd = ToFollow.GetComponent<Berd>();
        return TryFollow(berd, where, speed,PosClamp,SpeedClamp);
    }

    public void Scale(float endScale = 10.0f, float duration = 1.0f){
        endScale = SCALES.Lerp(endScale/10) * ScaleMod;
        duration = SpeedCheck(duration);

        if(Scaling != null)
            StopCoroutine(Scaling);

        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Scaling = StartCoroutine(transform.AnimatingLocalScale(Vector3.one*endScale,duration));
    }
    public void Scale(Transform endScale = null, float duration = 1.0f){
        if(endScale == null)
            return;
        duration = SpeedCheck(duration);

        if(Scaling != null)
            StopCoroutine(Scaling);

        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Scaling = StartCoroutine(transform.AnimatingLocalScale(endScale.lossyScale*ScaleMod,duration));
    }

    public void Wiggle(float Amplitude = 1, float speed = 1, bool speedClamp = true){
        if(Movement != null)
            StopCoroutine(Movement);
        speed = SpeedCheck(speed,speedClamp);

        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Movement = StartCoroutine(Wiggling(Amplitude,speed));
    }
    public IEnumerator Wiggling(float Amplitude = 1, float speed = 1){
        Vector3 startPos = transform.localPosition;
        Vector3 CurrentPos = startPos;
        speed = SPEED.Clamp(speed);
        Amplitude = Mathf.Clamp(Amplitude, -2f,2f) / 2;
        if(Amplitude == 0)
            Amplitude = 0.1f;
        float DurSeg = Amplitude/speed;
        AnimationCurve StartCurve = AnimationCurve.EaseInOut(0,0,0.25f,-1);
        AnimationCurve MidCurve   = AnimationCurve.EaseInOut(0.25f,-1,0.75f,1);
        AnimationCurve EndCurve   = AnimationCurve.EaseInOut(0.75f,1,1.0f,0.0f);

        float CurvePoint = 0;
        while(CurvePoint < 1){
            AnimationCurve workCurve = CurvePoint switch{
                < 0.25f => StartCurve,
                > 0.75f => EndCurve,
                _ => MidCurve
            };
            CurrentPos.x += workCurve.Evaluate(CurvePoint) * Amplitude;
            transform.localPosition = CurrentPos;

            yield return new WaitForEndOfFrame();
            CurvePoint += Time.deltaTime / DurSeg;

        }
        transform.localPosition = startPos;
    }

    public void StartDragging() => StartCoroutine(FollowCursor());
    IEnumerator FollowCursor(){
        Vector3 targetPos;
        QuackTired = false;
        IsDragging = true;
        Quack(2,UnityEngine.Random.Range(1.1f,1.5f));
        if(Movement != null)
            StopCoroutine(Movement);
        while (Input.GetMouseButton(0)){
            scale = SCALES.Clamp(scale+Input.mouseScrollDelta.y * 0.10f);
            transform.localScale = scale * ScaleMod * Vector3.one;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            targetPos = ray.origin;
            targetPos = BerdInterface.DRAGBOUND.Clamp(targetPos);
            targetPos.z = targetPos.y;
            transform.position = Vector3.Lerp(transform.position,targetPos,Time.deltaTime*10f);
            yield return null;
        }
        targetPos = transform.localPosition;
        if(!BerdInterface.WALKBOUND.VER.InBounds(targetPos.y)){
            targetPos.y = BerdInterface.WALKBOUND.VER.Random;
        }
        MoveTo(targetPos,10);
        yield return Movement;
        IsDragging = false;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Berd)),CanEditMultipleObjects]
public class BerdEditor : Editor{
    bool _scalerFold = false;
    SerializedProperty _startScale;
    SerializedProperty _scaleMod;
    bool _audioFold = false;
    SerializedProperty _quack;
    SerializedProperty _quackSpecial;
    SerializedProperty _specialChance;
    bool _spriteFold = true;
    SerializedProperty _walkTime;
    SerializedProperty _walkCycle;
    SerializedProperty _SpecialPos;
    GUIStyle centeredStyle;

    void OnEnable(){
        if(target == null || serializedObject == null)
            return;
        _startScale     = serializedObject.FindProperty("scale");
        _scaleMod       = serializedObject.FindProperty("ScaleMod");

        _quack          = serializedObject.FindProperty("quack");
        _quackSpecial   = serializedObject.FindProperty("quackSpecial");
        _specialChance  = serializedObject.FindProperty("specialChance");

        _walkTime       = serializedObject.FindProperty("WalkCycleTime");
        _walkCycle      = serializedObject.FindProperty("WalkCycle");

        _SpecialPos     = serializedObject.FindProperty("<SpecialPositions>k__BackingField");
    }
    public override void OnInspectorGUI(){
        if(target == null || serializedObject == null)
            return;
        centeredStyle ??= new GUIStyle(GUI.skin.box){
            alignment = TextAnchor.MiddleCenter 
        };

        _scalerFold = EditorGUILayout.Foldout(_scalerFold,"Scale Modifiers");
        if (_scalerFold){
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_startScale);
            EditorGUILayout.PropertyField(_scaleMod);
            EditorGUI.indentLevel--;
        }

        _audioFold = EditorGUILayout.Foldout(_audioFold,"Audio settings");
        if (_audioFold){
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_quack);
            if(_quack.objectReferenceValue != null){
                EditorGUILayout.PropertyField(_quackSpecial);
                if(_quackSpecial.objectReferenceValue != null)
                    EditorGUILayout.PropertyField(_specialChance);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_SpecialPos);
        Event evt = Event.current;
        _spriteFold = EditorGUILayout.Foldout(_spriteFold,"Sprite settings");
        if (_spriteFold){
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_walkTime);
            EditorGUILayout.PropertyField(_walkCycle);
            if(targets.Length == 1 && DrawSpriteDrop(ref evt))
                evt.Use();
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }

    bool DrawSpriteDrop(ref Event evt){
        EditorGUILayout.Space(10);
        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag Sprite Sheet Here",centeredStyle);
        Berd berd = (Berd)target;

        if(evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return false;
        if (!dropArea.Contains(evt.mousePosition))
            return false;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if(evt.type != EventType.DragPerform)
            return true;

        DragAndDrop.AcceptDrag();

        if(DragAndDrop.objectReferences.Length != 1){
            Debug.LogError("Must assign 1 item at a time");
            return true;
        }

        string path = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> sprites = new();
        foreach (UnityEngine.Object obj in assets){
            if (obj is Sprite sprite)
                sprites.Add(sprite);
        }

        if(sprites.Count == 0){
            Debug.Log($"No Sprites found for {DragAndDrop.objectReferences[0]}");
            return true;
        }

        _walkCycle.arraySize = sprites.Count;
        for (int i = 0; i < sprites.Count; i++)
            _walkCycle.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

        berd.gameObject.name = DragAndDrop.objectReferences[0].name;
        Debug.Log($"Loaded {sprites.Count} sprites into WalkCycle");
        return true;
    }
}
#endif
