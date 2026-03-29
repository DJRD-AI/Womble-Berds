using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Berd : MonoBehaviour
{
    [SerializeField] Vector3 _localPosition = Vector3.zero;
    [SerializeField] Vector3 _localScale = Vector3.zero;
    public Vector3 LocalPosition {get => _localPosition;}
    public Vector3 LocalScale {get => _localScale;}
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float WalkCycleTime;
    [SerializeField] Sprite[] WalkCycle;
    private Coroutine movement;
    private Coroutine Scaling;
    // Start is called before the first frame update
    IEnumerator Start(){
        transform.localPosition = _localPosition;
        transform.localScale = _localScale;
        int index = 0;
        float delay = WalkCycleTime/WalkCycle.Length;
        while (true){
            yield return new WaitForSeconds(delay);
            index = (index+1)%(WalkCycle.Length-1);
            spriteRenderer.sprite = WalkCycle[index];
        }
    }

    void OnDrawGizmos()
    {
        if(spriteRenderer.sprite == null)
            spriteRenderer.sprite = WalkCycle[0];
        if(_localPosition == Vector3.zero)
            _localPosition = transform.position;
        _localScale = transform.localScale;
    }
    public void Move(bool left = true, float distance = 1, float speed = 1){
        float duration = Mathf.Abs(distance)/speed; 
        if(left)
            distance *= -1;
        Vector3 endpos = transform.localPosition;
        endpos.x += distance;
        endpos.x = Mathf.Clamp(endpos.x,-10,10);
        movement = StartCoroutine(transform.AnimatingLocalPos(endpos,AnimationCurve.EaseInOut(0,0,1,1),duration));
        
    }

    public void ResetBerd()
    {
        if(movement != null)
            StopCoroutine(movement);
        if(Scaling != null)
            StopCoroutine(Scaling);
        transform.localPosition = LocalPosition;
        transform.localScale = LocalScale;
    }

    public void ChangePriority(bool up)
    {
        spriteRenderer.sortingOrder += up ? 1 : -1;
    }

    public void Scale(float endScale, float duration = 1.0f)
    {
        endScale = Mathf.Clamp(endScale,0.1f, 0.8f);
        Scaling = StartCoroutine(transform.AnimatingLocalScale(Vector3.one*endScale,AnimationCurve.EaseInOut(0,0,1,1),duration));
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

        foreach (Object dragged in DragAndDrop.objectReferences){
            string path     = AssetDatabase.GetAssetPath(dragged);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            var sprites = new List<Sprite>();

            foreach (Object obj in assets){
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
