using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;



#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class Berd : MonoBehaviour
{
    const float QUACKCOOLDOWN = 15.0f;
    private static readonly BoundValues SCALES = new(0.3f,0.8f);
    private static readonly BoundValues SPEED = new(1.0f, 10.0f);
    private static readonly BoundValues POSX = new(-7.8,8.5);
    private static readonly BoundValues POSY = new(-4.4f,-3.0f);
    private static readonly BoundValues RANDOMEVENT = new(15.0f,45.0f);
    bool QuackTired = false;
    private Vector2 _localPosition = Vector2.zero;
    [SerializeField] private float scale = 0.77f;
    [SerializeField] private float ScaleMod = 1f;
    [SerializeField] private AudioClip quack;
    [SerializeField] private AudioSource quackHole;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] float WalkCycleTime;
    [SerializeField] Sprite[] WalkCycle;
    private Coroutine Movement = null;
    private Coroutine Scaling = null;
    DateTime RandomEvent;

    private void Reset() {
        quackHole = GetComponent<AudioSource>();
        quackHole = quackHole != null ? quackHole : gameObject.AddComponent<AudioSource>();
    }

    void OnEnable(){
        StartCoroutine(Walking());
    }

    void OnDisable(){
        StopAllCoroutines();
    }

    void OnDestroy(){
        StopAllCoroutines();
    }
    void Start(){
        quackHole.clip = quack;
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
        float verPos = UnityEngine.Random.Range(POSY.Lower,POSY.Upper);
        QuackTired = true;
        transform.position = new (POSX.Upper+2, verPos, verPos);
        _localPosition = new (POSX.Random, POSY.Random);

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
        Vector2 MoveSpot = new(POSX.Lower-2,transform.localPosition.y);
        quackHole.Play();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        quackHole.Play();
        yield return new WaitUntil(() => !quackHole.isPlaying);
        MoveTo(MoveSpot,UnityEngine.Random.Range(1,4),false);
        yield return Movement;
        Destroy(gameObject);
    }

    IEnumerator Walking(){
        transform.localScale = scale * ScaleMod * Vector3.one;
        int index = 0;
        float delay = WalkCycleTime/WalkCycle.Length;
        while (true){
            yield return new WaitForSeconds(delay);
            index = (index+1)%(WalkCycle.Length-1);
            spriteRenderer.sprite = WalkCycle[index];
        }
    }

    IEnumerator RandomEvents()
    {
        while (true){
            yield return new WaitForSeconds((float)(RandomEvent - DateTime.Now).TotalSeconds);
            if(RandomEvent >= DateTime.Now)
                continue;
            switch (UnityEngine.Random.Range(0f, 1f))
            {
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
                    Vector2 EndPos = new(POSX.Random,POSY.Random);
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
            RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        }
    }

    public void MoveTo(Vector3 Endpos, float speed = 1,bool clamped = true){
        if(clamped){
            Endpos.x = POSX.Clamp(Endpos.x);
            Endpos.y = POSY.Clamp(Endpos.y);
        }
        Endpos.z = Endpos.y;
        speed = SPEED.Clamp(speed);
        float Duration = (Endpos-transform.localPosition).magnitude/speed;
        if(Movement != null)
            StopCoroutine(Movement);
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Movement = StartCoroutine(transform.AnimatingLocalPos(Endpos,AnimationCurve.EaseInOut(0,0,1,1),Duration));
    }
    public void MoveTo(Vector2 Endpos, float speed = 1, bool clamped = true) => MoveTo(new Vector3(Endpos.x,Endpos.y,Endpos.y), speed, clamped);
    public void MoveDir(Vector3 Dir, float speed = 1, bool clamped = true)   => MoveTo(Dir + transform.localPosition,speed, clamped);
    public void MoveDir(Vector2 Dir, float speed = 1, bool clamped = true)   => MoveDir(new Vector3(Dir.x,Dir.y,Dir.y),speed, clamped);
    public void Scale(float endScale, float duration = 1.0f){
        endScale = SCALES.Lerp(endScale/10) * ScaleMod;
        duration = SPEED.Clamp(duration);

        if(Scaling != null)
            StopCoroutine(Scaling);

        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        Scaling = StartCoroutine(transform.AnimatingLocalScale(Vector3.one*endScale,AnimationCurve.EaseInOut(0,0,1,1),duration));
    }

    public void Wiggle(float Amplitude = 1, float speed = 1)
    {
        if(Movement != null)
            StopCoroutine(Movement);
        Movement = StartCoroutine(Wiggling(Amplitude,speed));
    }
    public IEnumerator Wiggling(float Amplitude = 1, float speed = 1){
        Vector3 startPos = transform.localPosition;
        Vector3 CurrentPos = startPos;
        speed = SPEED.Clamp(speed);
        Amplitude = Mathf.Clamp(Amplitude, -2f,2f);
        if(Amplitude == 0)
            Amplitude = 1;
        float DurSeg = Amplitude/speed;
        Amplitude /= 2;
        AnimationCurve StartCurve = AnimationCurve.EaseInOut(0,0,0.25f,-Amplitude);
        AnimationCurve MidCurve   = AnimationCurve.EaseInOut(0.25f,-Amplitude,0.75f,Amplitude);
        AnimationCurve EndCurve   = AnimationCurve.EaseInOut(0.75f,Amplitude,1.0f,0.0f);

        float CurvePoint = 0;
        Debug.Log($"Amplitude: {Amplitude}, Speed: {speed}, DurSeg: {DurSeg}");
        while(CurvePoint < 1){
            AnimationCurve workCurve = CurvePoint switch{
                < 0.25f => StartCurve,
                > 0.75f => EndCurve,
                _ => MidCurve
            };
            Debug.Log($"CurvePoint:  {CurvePoint}, Value : {workCurve.Evaluate(CurvePoint)}");
            CurrentPos.x = startPos.x + workCurve.Evaluate(CurvePoint) * Amplitude;
            transform.localPosition = CurrentPos;

            yield return new WaitForEndOfFrame();
            CurvePoint += Time.deltaTime / DurSeg;
            Debug.Log($"CurvePoint 2: {CurvePoint}, change: {Time.deltaTime / DurSeg}");
            
        }
        transform.localPosition = startPos;
    }
    public Coroutine Quack() => StartCoroutine(Quacking());
    public IEnumerator Quacking(){
        if(QuackTired)
            yield break;
        RandomEvent = DateTime.Now.AddMinutes(RANDOMEVENT.Random);
        quackHole.Play();
        QuackTired = true;
        yield return new WaitForSeconds(QUACKCOOLDOWN);
        QuackTired = false;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Berd)),CanEditMultipleObjects]
public class BerdEditor : Editor
{
    public override void OnInspectorGUI()
    {

        Berd berd = (Berd)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Sprite Sheet Loader", EditorStyles.boldLabel);

        // Drag & drop area
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag Sprite Sheet Here");

        DrawDefaultInspector();

        Event evt = Event.current;
        if(evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;
        if (!dropArea.Contains(evt.mousePosition))
                return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if(evt.type != EventType.DragPerform){
            evt.Use();
            return;
        }

        DragAndDrop.AcceptDrag();

        foreach (UnityEngine.Object dragged in DragAndDrop.objectReferences){
            string path     = AssetDatabase.GetAssetPath(dragged);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            var sprites = new List<Sprite>();

            foreach (UnityEngine.Object obj in assets){
                if (obj is Sprite sprite)
                    sprites.Add(sprite);
            }


            // Assign to WalkCycle
            SerializedProperty walkCycleProp = serializedObject.FindProperty("WalkCycle");
            walkCycleProp.arraySize = sprites.Count;

            for (int i = 0; i < sprites.Count; i++)
                walkCycleProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

            // ✅ Rename GameObject to spritesheet name
            string sheetName = System.IO.Path.GetFileNameWithoutExtension(path);
            berd.gameObject.name = sheetName;

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"Loaded {sprites.Count} sprites into WalkCycle");
        }

        evt.Use();
    }
}
#endif
