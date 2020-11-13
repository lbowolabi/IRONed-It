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

    public IEnumerator Shake(Transform t, float trauma = 1, float traumaExponent = 1, float recoverySpeed = 1, float frequency = 25, float maximumTranslationShakeMulitiplier = 1, float maximumAngularShakeMultiplier = 15)
    {
        Vector2 maximumTranslationShake = Vector2.one * maximumTranslationShakeMulitiplier;
        Vector2 maximumAngularShake = Vector2.one * maximumTranslationShakeMulitiplier;
        float seed = UnityEngine.Random.value;
        while (trauma > 0)
        {
            // Taking trauma to an exponent allows the ability to smoothen
            // out the transition from shaking to being static.
            float shake = Mathf.Pow(trauma, traumaExponent);

            // This x value of each Perlin noise sample is fixed,
            // allowing a vertical strip of noise to be sampled by each dimension
            // of the translational and rotational shake.
            // PerlinNoise returns a value in the 0...1 range; this is transformed to
            // be in the -1...1 range to ensure the shake travels in all directions.
            transform.localPosition = new Vector3(
                maximumTranslationShake.x * (Mathf.PerlinNoise(seed, Time.time * frequency) * 2 - 1),
                maximumTranslationShake.y * (Mathf.PerlinNoise(seed + 1, Time.time * frequency) * 2 - 1)
                //maximumTranslationShake.z * (Mathf.PerlinNoise(seed + 2, Time.time * frequency) * 2 - 1)
            ) * shake;

            transform.localRotation = Quaternion.Euler(new Vector3(
                maximumAngularShake.x * (Mathf.PerlinNoise(seed + 3, Time.time * frequency) * 2 - 1),
                maximumAngularShake.y * (Mathf.PerlinNoise(seed + 4, Time.time * frequency) * 2 - 1)
                //maximumAngularShake.z * (Mathf.PerlinNoise(seed + 5, Time.time * frequency) * 2 - 1)
            ) * shake);

            trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);
            yield return null;
        }
    }
}
