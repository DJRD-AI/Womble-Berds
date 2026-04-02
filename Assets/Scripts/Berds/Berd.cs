using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using System.Linq;





#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class Berd : MonoBehaviour
{
    const float QUACKCOOLDOWN = 15.0f;
    private static readonly BoundVal SCALES      = new(0.3f,0.8f);
    private static readonly BoundVal SPEED       = new(1.0f, 10.0f);
    private static readonly BoundVal QUACKPITCH  = new(0.5f,1.5f);
    private static readonly BoundVal QUACKVOL    = new(0.5f,2f);
    private static readonly BoundVal RANDOMEVENT = new(15.0f,45.0f);
    private static readonly BoundPos POS         = new(-7.8,8.5,-4.96f,-3.56f);
    bool QuackTired = false;
    private Vector2 _localPosition = Vector2.zero;
    [
        SerializeField,
        Tooltip("The scale within the set bounds\n0=min size, 1=maxSize"),
        Range(0.0f,1.0f)
    ] private float scale = 1.0f;

    [SerializeField,Tooltip("Use this to make sure all the berds are of roughly equal size\nEXCEPT AZY. she be bigger :P")]
    private float ScaleMod = 1f;
    [SerializeField] private AudioClip quack;
    [SerializeField] private AudioSource quackHole;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] float WalkCycleTime;
    [SerializeField] List<Sprite> WalkCycle;
    private Coroutine Movement = null;
    private Coroutine Scaling = null;
    DateTime RandomEvent;

    private void Reset() {
        quackHole = GetComponent<AudioSource>();
        quackHole = quackHole != null ? quackHole : gameObject.AddComponent<AudioSource>();
        float verpos = POS.VER.Random;
        transform.localPosition = new(POS.HOR.Random,verpos,verpos);
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
        float verPos = POS.VER.Random;
        QuackTired = true;
        transform.position = new(POS.RIGHT+2,verPos,verPos);
        _localPosition = new (POS.HOR.Random, POS.VER.Random);

        quackHole.Play();
        QuackTired = false;
        MoveTo(_localPosition, UnityEngine.Random.Range(1,4));
        yield return Movement;
        quackHole.Play();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        QuackTired = false;
        yield return Quacking();
    }

    public Coroutine DespawnBerd() => StartCoroutine(Despawning());

    IEnumerator Despawning(){
        Vector2 MoveSpot = new(POS.LEFT-2,transform.localPosition.y);
        quackHole.Play();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        quackHole.Play();
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
                    Vector2 EndPos = new(POS.HOR.Random,POS.VER.Random);
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

    public void MoveTo(Vector3 End, float speed = 1, bool PosClamp = true, bool SpeedClamp = true){
        if(PosClamp){
            End.x = POS.HOR.Clamp(End.x);
            End.y = POS.VER.Clamp(End.y);
        }
        End.z = End.y;
        if(SpeedClamp)
            speed = SPEED.Clamp(speed);
        float Duration = (End-transform.localPosition).magnitude/speed;
        if(Movement != null)
            StopCoroutine(Movement);
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Movement = StartCoroutine(transform.AnimatingLocalPos(End,AnimationCurve.EaseInOut(0,0,1,1),Duration));
    }
    public void MoveTo (Vector2 End, float speed = 1, bool PosClamp = true, bool SpeedClamp = true) => MoveTo(new Vector3(End.x,End.y,End.y), speed, PosClamp, SpeedClamp);
    public void MoveDir(Vector3 Dir, float speed = 1, bool PosClamp = true, bool SpeedClamp = true) => MoveTo(Dir + transform.localPosition,  speed, PosClamp, SpeedClamp);
    public void MoveDir(Vector2 Dir, float speed = 1, bool PosClamp = true, bool SpeedClamp = true) => MoveDir(new Vector3(Dir.x,Dir.y,Dir.y),speed, PosClamp, SpeedClamp);
    public void Scale(float endScale, float duration = 1.0f){
        endScale = SCALES.Lerp(endScale/10) * ScaleMod;
        duration = SPEED.Clamp(duration);

        if(Scaling != null)
            StopCoroutine(Scaling);

        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Scaling = StartCoroutine(transform.AnimatingLocalScale(Vector3.one*endScale,AnimationCurve.EaseInOut(0,0,1,1),duration));
    }

    public void Wiggle(float Amplitude = 1, float speed = 1){
        if(Movement != null)
            StopCoroutine(Movement);
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Movement = StartCoroutine(Wiggling(Amplitude,speed));
    }
    public IEnumerator Wiggling(float Amplitude = 1, float speed = 1){
        Vector3 startPos = transform.localPosition;
        Vector3 CurrentPos = startPos;
        speed = SPEED.Clamp(speed);
        Amplitude = Mathf.Clamp(Amplitude, -2f,2f);
        Amplitude /= 2;
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
            CurrentPos.x = startPos.x + workCurve.Evaluate(CurvePoint) * Amplitude;
            transform.localPosition = CurrentPos;

            yield return new WaitForEndOfFrame();
            CurvePoint += Time.deltaTime / DurSeg;

        }
        transform.localPosition = startPos;
    }
    public void Quack(float volume = 1.0f, float pitch = 1.0f,bool clampPitch = true, bool VolClamp = true) => StartCoroutine(Quacking(volume,pitch,clampPitch,VolClamp));
    public IEnumerator Quacking(float volume = 1.0f, float pitch = 1.0f,bool clampPitch = true, bool VolClamp = true){
        if(QuackTired)
            yield break;
        if(clampPitch)
            pitch = QUACKPITCH.Clamp(pitch);
        if(VolClamp)
            volume = QUACKVOL.Clamp(pitch);
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        quackHole.pitch = pitch;
        quackHole.Play();
        QuackTired = true;
        yield return new WaitForSeconds(QUACKCOOLDOWN);
        QuackTired = false;
    }

    public void StartDraggin(){
        StartCoroutine(FollowCursor());
    }

    IEnumerator FollowCursor(){
        Vector3 targetPos;
        QuackTired = false;
        Quack(2,UnityEngine.Random.Range(1.1f,1.5f));
        if(Movement != null)
            StopCoroutine(Movement);
        while (Input.GetMouseButton(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            targetPos = ray.origin;
            targetPos.y = Mathf.Max(POS.DOWN,targetPos.y);
            targetPos.z = targetPos.y;
            transform.position = Vector3.Lerp(transform.position,targetPos,Time.deltaTime*2f);
            yield return new WaitForEndOfFrame();
        }
        targetPos = transform.localPosition;
        if(!POS.VER.InBounds(targetPos.y)){
            Debug.Log("Find New Pos");
            targetPos.y = POS.VER.Random;
        }
        MoveTo(targetPos,10);
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
    bool _spriteFold = true;
    SerializedProperty _walkTime;
    SerializedProperty _walkCycle;
    SerializedProperty _renderer;
    GUIStyle centeredStyle;

    void OnEnable(){
        if(target == null || serializedObject == null)
            return;
        _startScale = serializedObject.FindProperty("scale");
        _scaleMod = serializedObject.FindProperty("ScaleMod");
        _quack = serializedObject.FindProperty("quack");
        _walkTime = serializedObject.FindProperty("WalkCycleTime");
        _walkCycle = serializedObject.FindProperty("WalkCycle");
        _renderer = serializedObject.FindProperty("spriteRenderer");
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
            EditorGUI.indentLevel--;
        }
        Event evt = Event.current;
        _spriteFold = EditorGUILayout.Foldout(_spriteFold,"Sprite settings");
        if (_spriteFold){
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_walkTime);
            EditorGUILayout.PropertyField(_walkCycle);
            EditorGUILayout.PropertyField(_renderer);
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

        for (int i = 0; i < sprites.Count; i++)
            _walkCycle.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

        berd.gameObject.name = DragAndDrop.objectReferences[0].name;
        Debug.Log($"Loaded {sprites.Count} sprites into WalkCycle");
        return true;
    }
}
#endif
