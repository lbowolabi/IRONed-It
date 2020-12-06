using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Helpers : MonoBehaviour
{
    public static Helpers instance;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator Timer(Action<bool> assigner, float timer)
    {
        yield return new WaitForSeconds(timer);
        assigner(true);
    }

    public IEnumerator WaitOneFrame(Action<bool> assigner)
    {
        yield return null;
        assigner(true);
    }

    public IEnumerator Shake(Transform t, float timeToShake, float shakeMagnitude)
    {
        Vector3 startPosition = t.position;
        while (timeToShake > 0)
        {
            float shakingOffsetX = UnityEngine.Random.value * shakeMagnitude * 2 - shakeMagnitude;
            float shakingOffsetY = UnityEngine.Random.value * shakeMagnitude * 2 - shakeMagnitude;
            Vector3 intermediatePosition = t.position;
            intermediatePosition.x += shakingOffsetX;
            intermediatePosition.y += shakingOffsetY;
            t.position = intermediatePosition;
            timeToShake -= Time.deltaTime;
            yield return null;
        }
        t.position = startPosition;
    }

    private void OnLevelWasLoaded(int level)
    {
        StopAllCoroutines();
    }
}
