using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{

    public static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    public static string Shuffle(string _list)  {
        UnityEngine.Random.InitState(_list.Length);
        string result = "";
        int rngIndex;
        int test = 0;
        List<char> chars = new List<char>(_list.ToCharArray());

        while (chars.Count > 0 && test < 100) {  
            test++;
            rngIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, chars.Count));
            result += chars[rngIndex];
            chars.RemoveAt(rngIndex);
        } 
        return result;
    }

    ///<summary>
    /// Returns true if the angle between two vectors is on the left or right side
    ///</summary>
    public static float AngleDir(Vector3 fwd, Vector3 targetDir,Vector3 up) {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
    
        if (dir > 0.0f)
            return 1.0f;
        else if (dir < 0.0f)
            return -1.0f;
        else
            return 0.0f;
    }

    ///<summary>
    /// Remaps the flaot values to scale it.
    ///</summary>
    public static float Map(float s, float a1, float a2, float b1, float b2){
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }

    ///<summary>
    /// returns thrue if two numbers are nearly equal to eahc other based on the offset parameter
    ///</summary>
    public static bool NearlyEqual(this float a , float b, float offset){
        return Mathf.Abs(a - b) <= offset;
    }

    /// <summary>
    /// Returns the vector where all axis are clamped to the min and max value
    /// </summary>
    /// <param name="vector3">Base vector to be clamped</param>
    /// <param name="min">minimum value</param>
    /// <param name="max">maximum value</param>
    public static Vector3 ClampAxis(this Vector3 vector3, float min = -1.0f, float max = 1.0f) {
        vector3.x = Mathf.Clamp(vector3.x,min,max);
        vector3.y = Mathf.Clamp(vector3.y,min,max);
        vector3.z = Mathf.Clamp(vector3.z,min,max);
        return vector3;
    }
    /// <summary>
    /// Returns the vector where all axis are clamped to the min and max value
    /// </summary>
    /// <param name="vector2">Base vector to be clamped</param>
    /// <param name="min">minimum value</param>
    /// <param name="max">maximum value</param>
    public static Vector2 ClampAxis(this Vector2 vector2, float min = 0.0f, float max = 0.0f) {
        vector2.x = Mathf.Clamp(vector2.x,min,max);
        vector2.y = Mathf.Clamp(vector2.y,min,max);
        return vector2;
    }
    /// <summary>
    /// Returns the vector where all absolute axis values are either:<br/>
    /// - >= threshold <br/>
    /// - zero
    /// </summary>
    /// <param name="vector3">Base Vector</param>
    /// <param name="threshold">Absolute threshold</param>
    public static Vector3 ThresholdAxis(this Vector3 vector3, float threshold = 0.1f) {
        vector3.x = Mathf.Abs(vector3.x) >= threshold ? vector3.x : 0;
        vector3.y = Mathf.Abs(vector3.y) >= threshold ? vector3.y : 0;
        vector3.z = Mathf.Abs(vector3.z) >= threshold ? vector3.z : 0;
        return vector3;
    }
    /// <summary>
    /// Returns the vector where all absolute axis values are either:<br/>
    /// - Greater or equal to the threshold value<br/>
    /// - zero
    /// </summary>
    /// <param name="vector3">Base Vector</param>
    /// <param name="threshold">Absolute threshold</param>
    public static Vector2 ThresholdAxis(this Vector2 vector2, float threshold = 0.1f) {
        vector2.x = Mathf.Abs(vector2.x) >= threshold ? vector2.x : 0;
        vector2.y = Mathf.Abs(vector2.y) >= threshold ? vector2.y : 0;
        return vector2;
    }
    public static Vector3 RandomVector(float maxValue) {
        return new Vector3(
            UnityEngine.Random.Range(-maxValue, maxValue),
            UnityEngine.Random.Range(-maxValue, maxValue),
            UnityEngine.Random.Range(-maxValue, maxValue));
    }
    public static Vector2 RandomVector2(float maxValue) {
        return new Vector2(
            UnityEngine.Random.Range(-maxValue, maxValue),
            UnityEngine.Random.Range(-maxValue, maxValue));
    }

    public static Vector3 CalculateLinearBezierPoint(float t, Vector3 p0, Vector3 p1) {
        return p0 + t * (p1 - p0);
    }

    public static IEnumerator FadeSFXVolume(this AudioSource audioSource, float endVal,  AnimationCurve curve, float duration = .5f, float delay = 0f) {
        yield return new WaitForSeconds(delay);
        float timePassed = 0f;
        float begin = audioSource.volume;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(begin, endVal , curve.Evaluate(timePassed / duration));
        }
        audioSource.volume = endVal;
    }

    public static IEnumerator AnimateLightIntensity(this Light light, float endVal,  AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        float begin = light.intensity;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            light.intensity = Mathf.Lerp(begin, endVal , curve.Evaluate(timePassed / duration));
        }
        light.intensity = endVal;
    }
    
    public static  IEnumerator AnimatingFieldOfView(this Camera camera, float endview,  AnimationCurve curve, float duration = .5f) {
        float timePassed = 0f;
        float beginView = camera.fieldOfView;
        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
            camera.fieldOfView = Mathf.LerpUnclamped(beginView, endview , curve.Evaluate(timePassed / duration));
        }
        camera.fieldOfView = endview;
    }

    public static List<T> SetAllComponentsActive<T>(this GameObject go, bool active, List<T> exclude) {
        if (exclude == default(List<T>)) exclude = new List<T>();
        List<T> result = new List<T>();
        foreach (T childCompnent in go.GetComponentsInChildren<T>())
        {
            if (childCompnent  is Renderer)
            {
                if((childCompnent as Renderer).enabled == false && active == false) {
                    // Debug.Log("child components is false enabled: " + (childCompnent as Renderer).enabled);
                    result.Add(childCompnent); //TODO: Check why this doesnt work
                }
                if(!exclude.Contains(childCompnent)) (childCompnent as Renderer).enabled = active;
            }
            if (childCompnent  is Light)
                (childCompnent as Light).enabled = active;
            if (childCompnent  is Collider)
                (childCompnent as Collider).enabled = active;
            if (childCompnent  is Rigidbody)
                (childCompnent as Rigidbody).isKinematic = !active;
            if (childCompnent  is ParticleSystem)
                if (active && (childCompnent as ParticleSystem).main.loop) (childCompnent as ParticleSystem).Play();
                else (childCompnent as ParticleSystem).Stop();
        }
        return result;
    }

    public static IEnumerator AnimateCallBack(float begin, float end, AnimationCurve curve, Action<float> callback, float animationDuration) {
        callback(begin);
        float index = 0f;
        while (index < animationDuration) {
            yield return new WaitForEndOfFrame();
            index += Time.unscaledDeltaTime;
            callback(Mathf.LerpUnclamped(begin, end, curve.Evaluate(index / animationDuration)));
        }
        callback(end);
    }
    public static IEnumerator AnimateCallBack(Color begin, Color end, AnimationCurve curve, Action<Color> callback, float animationDuration) {
        callback(begin);
        float index = 0f;
        while (index < animationDuration) {
            yield return new WaitForEndOfFrame();
            index += Time.unscaledDeltaTime;
            callback(Color.LerpUnclamped(begin, end, curve.Evaluate(index / animationDuration)));
        }
        callback(end);
    }
    public static IEnumerator AnimateCallBack(Vector3 begin, Vector3 end, AnimationCurve curve, Action<Vector3> callback, float animationDuration) {
        callback(begin);
        float index = 0f;
        while (index < animationDuration) {
            yield return new WaitForEndOfFrame();
            index += Time.unscaledDeltaTime;
            callback(Vector3.LerpUnclamped(begin, end, curve.Evaluate(index / animationDuration)));
        }
        callback(end);
    }
    public static IEnumerator AnimateCallBack(Vector2 begin, Vector2 end, AnimationCurve curve, Action<Vector2> callback, float animationDuration) {
        callback(begin);
        float index = 0f;
        while (index < animationDuration) {
            yield return new WaitForEndOfFrame();
            index += Time.unscaledDeltaTime;
            callback(Vector2.LerpUnclamped(begin, end, curve.Evaluate(index / animationDuration)));
        }
        callback(end);
    }
    public static IEnumerator AnimateCallBack(Quaternion begin, Quaternion end, AnimationCurve curve, Action<Quaternion> callback, float animationDuration) {
        callback(begin);
        float index = 0f;
        while (index < animationDuration) {
            yield return new WaitForEndOfFrame();
            index += Time.unscaledDeltaTime;
            callback(Quaternion.SlerpUnclamped(begin, end, curve.Evaluate(index / animationDuration)));
        }
        callback(end);
    }



    public static  IEnumerator AnimatingDissolveMaterial(this Material mat, float beginVal, float endVal,  AnimationCurve curve, float duration = .5f, float edgeWidth = .05f) {
        mat.SetFloat("EdgeWidth", edgeWidth);
        mat.SetFloat("Dissolve", beginVal);
        float timePassed = 0f;

        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.unscaledDeltaTime;
            mat.SetFloat("Dissolve", Mathf.LerpUnclamped(beginVal, endVal , curve.Evaluate(timePassed / duration)));
        }
        mat.SetFloat("Dissolve", endVal);
        mat.SetFloat("EdgeWidth", 0);
    }
    public static  IEnumerator AnimatingSnowMaterial(this Material mat, float beginVal, float endVal,  AnimationCurve curve, float duration = .5f) {
        mat.SetFloat("Opacity", beginVal);
        float timePassed = 0f;

        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.unscaledDeltaTime;
            mat.SetFloat("Opacity", Mathf.LerpUnclamped(beginVal, endVal , curve.Evaluate(timePassed / duration)));
        }
        mat.SetFloat("Opacity", endVal);
    }

    public static IEnumerator AnimatingNumberPropertyMaterial(this Material mat, string key,  float beginVal, float endVal,  AnimationCurve curve, float duration = .5f, float delay = 0) {
        yield return new WaitForSeconds(delay);
        mat.SetFloat(key, beginVal);
        float timePassed = 0f;

        while (timePassed < duration) {
            yield return new WaitForEndOfFrame();
            timePassed += Time.unscaledDeltaTime;
            mat.SetFloat(key, Mathf.LerpUnclamped(beginVal, endVal , curve.Evaluate(timePassed / duration)));
        }
        mat.SetFloat(key, endVal);
    }

    public static void SetNearClipPlane(this Camera reflectionCamera, Transform transform, Camera mainCamera) {
        Transform clipPlane = transform; 
        int dot = Math.Sign(Vector3.Dot(clipPlane.forward, clipPlane.position - reflectionCamera.transform.position));

        Vector3 cameraSpacePos = reflectionCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        int revert = clipPlane.position.y < mainCamera.transform.position.y ? 1 : -1;
        Vector3 cameraSpaceNormal = reflectionCamera.worldToCameraMatrix.MultiplyVector(clipPlane.up * revert) * dot;
        float camSpaceDst = -Vector3.Dot(cameraSpacePos, cameraSpaceNormal);
        Vector4 clipPlaneCameraSpace = new Vector4(cameraSpaceNormal.x, cameraSpaceNormal.y, cameraSpaceNormal.z, camSpaceDst);


        reflectionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    public static bool IncameraRange(this Renderer renderer, Camera mainCamera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if(GeometryUtility.TestPlanesAABB(planes, renderer.bounds)){
            return true;
        } else {
            return false;   
        }
    }

    public static RaycastHit LerpWithBezier(this RaycastHit a,RaycastHit b, RaycastHit mid,  float interval) {
        RaycastHit temp = a;
        temp.point = CalculateQuadraticBezierPoint(interval, a.point, mid.point, b.point);
        // temp.point = Vector3.LerpUnclamped(a.point, b.point, interval);
        temp.normal = Vector3.SlerpUnclamped(a.normal, b.normal, interval);
        return temp;
    }
    public static RaycastHit LerpUnclamped(this RaycastHit a,RaycastHit b,  float interval) {
        RaycastHit temp = a;
        temp.point = Vector3.LerpUnclamped(a.point, b.point, interval);
        temp.normal = Vector3.SlerpUnclamped(a.normal, b.normal, interval);
        return temp;
    }


    public static Vector3 MouseToWorldPosition(this Canvas m_Canvas) {
        Plane m_CanvasPlane = new Plane();
        m_CanvasPlane.Set3Points (
            m_Canvas.transform.TransformPoint (new Vector3 (0, 0)), 
            m_Canvas.transform.TransformPoint (new Vector3 (0, 1)),
            m_Canvas.transform.TransformPoint (new Vector3 (1, 0))
        );
        // Raycast from the camera to the plane, to get the screen position on the canvas
        Ray ray = Camera.main.ScreenPointToRay (new Vector3(Screen.width * .5f, Screen.height * .5f, 0));
        Vector3 worldPosOnCanvas = Vector3.zero;
        float rayHitDistance= 20f;
        if (m_CanvasPlane.Raycast (ray, out rayHitDistance)) {
            //RESULT: Here is what you what (in world space coordinate)
            worldPosOnCanvas = ray.GetPoint (rayHitDistance * 0.9f);
        }
        return worldPosOnCanvas;
    }

}
