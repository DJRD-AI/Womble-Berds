using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static IEnumerator AnimatingLocalScale(this Transform transform, Vector3 endScale, AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        Vector3 beginScale = transform.localScale;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(beginScale, endScale , curve.Evaluate(timePassed / duration));
        }
        transform.localScale = endScale;
    }

    public static  IEnumerator AnimatingLocalPos(this Transform transform, Vector3 endLocalPos, AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        Vector3 beginPos = transform.localPosition;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            transform.localPosition = Vector3.LerpUnclamped(beginPos, endLocalPos , curve.Evaluate(timePassed / duration));
        }
        transform.localPosition = endLocalPos;
    }
    public static  IEnumerator AnimatingPos(this Transform transform, Vector3 endPos,  AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        Vector3 beginPos = transform.position;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            transform.position = Vector3.LerpUnclamped(beginPos, endPos , curve.Evaluate(timePassed / duration));
        }
        transform.position = endPos;
    }

    public static  IEnumerator AnimatingPosBounce(this Transform transform, float amplitude,  AnimationCurve curve, float duration = 5f) {
        float timePassed = 0f;
        Vector3 beginPos = transform.position;
        float currentHeight = 0;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            currentHeight = Mathf.Sin(Mathf.PI * (curve.Evaluate(timePassed / duration))) * amplitude;
            transform.position = beginPos + new Vector3(0,currentHeight, 0);
            // transform.position = Vector3.LerpUnclamped(beginPos, endPos , curve.Evaluate(timePassed / duration));
        }
        transform.position = beginPos;
    }

    public static  IEnumerator AnimatingPosBezierCurve(this Transform transform, Vector3 end, Vector3 mid, AnimationCurve curve, float duration = 5f) {
        float index = 0f;
        Vector3 begin = transform.position;
        while (index < duration) {
            yield return new WaitForEndOfFrame();
            index += Time.deltaTime;
            transform.position = Extensions.CalculateQuadraticBezierPoint(curve.Evaluate(index / duration),  begin, mid, end);
        }
        transform.position = end;
    }


    public static  IEnumerator AnimatingRotation(this Transform transform, Quaternion endrotation,  AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        Quaternion beginrotation = transform.rotation;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            transform.rotation = Quaternion.SlerpUnclamped(beginrotation, endrotation , curve.Evaluate(timePassed / duration));
        }
        yield return new WaitForEndOfFrame();
        transform.rotation = endrotation;
    }

    public static  IEnumerator AnimatingLocalRotation(this Transform transform, Quaternion endrotation,  AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        Quaternion beginrotation = transform.localRotation;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            transform.localRotation = Quaternion.SlerpUnclamped(beginrotation, endrotation , curve.Evaluate(timePassed / duration));
        }
        transform.localRotation = endrotation;
    }

    public static IEnumerator ShakeZRotation(this Transform transform, float magnitude, float frequence, float duration = .5f, Action callback = default(Action)) {
        float timePassed = 0f;
        Quaternion start = transform.localRotation;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            float currentMagnitude = Mathf.Sin(Mathf.PI * (timePassed / duration)) * magnitude;
            // transform.localRotation = start;
            transform.Rotate( new Vector3(
                0,0,
                Mathf.Sin((timePassed * frequence) * (Mathf.PI * 2)) * currentMagnitude
            ));
        }
        transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, start.z);
        if (callback != default(Action)) callback();
    }
    public static IEnumerator ShakeLocalYPos(this Transform transform, float magnitude, float frequence, float duration = .5f) {
        float timePassed = 0f;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            float currentMagnitude = Mathf.Sin(Mathf.PI * (timePassed / duration)) * magnitude;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                Mathf.Sin((timePassed * frequence) * (Mathf.PI * 2)) * currentMagnitude,
                transform.localPosition.z
            );
        }
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            0,
            transform.localPosition.z
        );
    }

    public static Quaternion Clamp (Quaternion q, float angle = 90f) {
        Vector3 v = new Vector3(
            q.eulerAngles.x,
            q.eulerAngles.y,
            q.eulerAngles.z);
        v.x = Mathf.Clamp(v.x, -angle, angle);
        v.y = Mathf.Clamp(v.y, -angle, angle);
        v.z = Mathf.Clamp(v.z, -angle, angle);
        return Quaternion.Euler(v);
    }

    public static Quaternion RandomRotation (float amplitude = 1f) {
        return Quaternion.Euler(UnityEngine.Random.Range(0.0f, 360.0f * amplitude), UnityEngine.Random.Range(0.0f, 360.0f * amplitude), UnityEngine.Random.Range(0.0f, 360.0f * amplitude));
    }
    public static Vector3 RandomVector (float amplitude = 1f) {
        return new Vector3(UnityEngine.Random.Range(-amplitude, amplitude), UnityEngine.Random.Range(-amplitude, amplitude), UnityEngine.Random.Range(-amplitude, amplitude));
    }


}
