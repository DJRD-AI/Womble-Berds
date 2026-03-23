using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berd : MonoBehaviour
{
    [SerializeField] Vector3 LocalPos = Vector3.zero;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float WalkCycleTime;
    [SerializeField] Sprite[] WalkCycle;
    // Start is called before the first frame update
    IEnumerator Start(){
        transform.localPosition = LocalPos;
        int index = 0;
        float delay = WalkCycleTime/WalkCycle.Length;
        while (true){
            yield return new WaitForSeconds(delay);
            index = (index+1)%WalkCycle.Length;
            spriteRenderer.sprite = WalkCycle[index];
        }
    }
    public void MoveBerd(bool left = true, float distance = 1, float speed = 1){
        float duration = Mathf.Abs(distance)/speed;
        if(left)
            distance *= -1;
        Vector3 endpos = transform.localPosition;
        endpos.x += distance;
        StartCoroutine(transform.AnimatingLocalPos(endpos,AnimationCurve.EaseInOut(0,0,1,1),duration));
    }
    public void Moveleft(float distance = 1)  => MoveBerd(true,distance);
    public void MoveRight(float distance = 1) => MoveBerd(false,distance);
}
