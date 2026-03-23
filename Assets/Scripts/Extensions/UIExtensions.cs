using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIExtensions
{
    public static IEnumerator FadeCanvasGroup( this CanvasGroup group, float end, float duration, float delay = 0) {
        yield return new WaitForSecondsRealtime(delay);
        float index = 0;
        float begin = group.alpha;
         while (index < duration) {
             yield return new WaitForEndOfFrame();
             index += Time.unscaledDeltaTime;
             group.alpha = Mathf.Lerp(begin, end, index / duration);
         }
        group.alpha = end;
    }
    public static IEnumerator AnimateTextAlpha( this TMP_Text text, float end, float duration, float delay = 0) {
        yield return new WaitForSecondsRealtime(delay);
        float index = 0;
        float begin = text.alpha;
         while (index < duration) {
             yield return new WaitForEndOfFrame();
             index += Time.unscaledDeltaTime;
             text.alpha = Mathf.Lerp(begin, end, index / duration);
         }
        text.alpha = end;
    }

    public static IEnumerator AnimateLocalRotation( this RectTransform rect, Quaternion end, float duration, float delay = 0) {
        yield return new WaitForSeconds(delay);
        float index = 0;
        Quaternion begin = rect.localRotation;
         while (index < duration) {
             yield return new WaitForEndOfFrame();
             index += Time.unscaledDeltaTime;
             rect.localRotation = Quaternion.SlerpUnclamped(begin, end, index / duration);
         }
        rect.localRotation = end;
    }
    public static IEnumerator AnimateLocalPosition( this RectTransform rect, Vector3 end, float duration, float delay = 0) {
        yield return new WaitForSeconds(delay);
        float index = 0;
        Vector3 begin = rect.localPosition;
         while (index < duration) {
             yield return new WaitForEndOfFrame();
             index += Time.unscaledDeltaTime;
             rect.localPosition = Vector3.LerpUnclamped(begin, end, index / duration);
         }
        rect.localPosition = end;
    }
    public static IEnumerator AnimateLocalScale( this RectTransform rect, Vector3 end, float duration, float delay = 0) {
        yield return new WaitForSeconds(delay);
        float index = 0;
        Vector3 begin = rect.localScale;
         while (index < duration) {
             yield return new WaitForEndOfFrame();
             index += Time.unscaledDeltaTime;
             rect.localScale = Vector3.LerpUnclamped(begin, end, index / duration);
         }
        rect.localScale = end;
    }
    public static  IEnumerator AnimatingLocalRotation(this RectTransform rect, Vector3 beginrotation, Vector3 endrotation,  AnimationCurve curve, float duration = .5f, float delay = 0) {
        yield return new WaitForSeconds(delay);
        float index = 0f;
        while (index < duration) {
            yield return new WaitForEndOfFrame();
            index += Time.deltaTime;
            rect.rotation = Quaternion.Euler( Vector3.LerpUnclamped(beginrotation, endrotation , curve.Evaluate(index / duration)));
        }
        rect.rotation = Quaternion.Euler(endrotation);
    }


}
