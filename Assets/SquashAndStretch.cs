using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SquashAndStretch
{
    public static void ApplySquashAndStretch(this MonoBehaviour obj, float scale, float time)
    {
        obj.StartCoroutine(DoSquashAndStretch(obj, scale, time));
    }
    public static IEnumerator DoSquashAndStretch(MonoBehaviour obj, float scale, float time)
    {
        Vector3 originalScale = obj.transform.localScale;
        // Define the squash scale (decrease y, increase x and z)
        Vector3 squashScale = new Vector3(originalScale.x * scale, originalScale.y / scale, originalScale.z * scale);
        // Define the stretch scale (increase y, decrease x and z)
        Vector3 stretchScale = new Vector3(originalScale.x / scale, originalScale.y * scale, originalScale.z / scale);

        // Part 1: Squash
        yield return obj.StartCoroutine(ScaleOverTime(obj, squashScale, time / 3));

        // Part 2: Stretch
        yield return obj.StartCoroutine(ScaleOverTime(obj, stretchScale, time / 3));

        // Part 3: Return to original scale
        yield return obj.StartCoroutine(ScaleOverTime(obj, originalScale, time / 3));
    }

    public static IEnumerator ScaleOverTime(MonoBehaviour obj, Vector3 targetScale, float duration)
    {
        Vector3 startScale = obj.transform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            obj.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        obj.transform.localScale = targetScale; // Ensure target scale is set exactly
    }
}
