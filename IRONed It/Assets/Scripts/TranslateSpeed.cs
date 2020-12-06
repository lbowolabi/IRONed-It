using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateSpeed : MonoBehaviour
{
    float currentSpeed, defaultSpeed, speedSmoothing;
    IEnumerator speedDampening;

    // Start is called before the first frame update
    private void FixedUpdate()
    {
        transform.Translate(Vector2.left * currentSpeed * Time.fixedDeltaTime);
    }

    public void MatchSpeedToPlayer(bool canPlayerMove)
    {
        if (speedDampening != null) StopCoroutine(speedDampening);
        speedDampening = SpeedDampening(canPlayerMove);
        StartCoroutine(speedDampening);
    }

    IEnumerator SpeedDampening(bool canPlayerMove)
    {
        yield return null;
        float duration = Motile.playerInstance.deathDuration;
        if (canPlayerMove)
        {
            while (currentSpeed < defaultSpeed - .05f || currentSpeed > defaultSpeed + .05f)
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, defaultSpeed, ref speedSmoothing, duration);
                yield return null;
            }
        }
        else
        {
            float multiplier = LevelManager.instance.playerDeathSpeedMultiplier;
            while (currentSpeed < defaultSpeed * multiplier - .05f || currentSpeed > defaultSpeed * multiplier + .05f)
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, defaultSpeed * multiplier, ref speedSmoothing, duration);
                yield return null;
            }
        }
    }

    private void OnEnable()
    {
        if (gameObject.CompareTag("Iron"))
        {
            defaultSpeed = Random.Range(LevelManager.instance.ironSpeedRange.x, LevelManager.instance.ironSpeedRange.y);
        }
        else if (gameObject.name == "Cholera(Clone)")
        {
            defaultSpeed = Random.Range(LevelManager.instance.choleraSpeedRange.x, LevelManager.instance.choleraSpeedRange.y);
        }
        else if (gameObject.name == "Coli(Clone)")
        {
            defaultSpeed = Random.Range(LevelManager.instance.coliSpeedRange.x, LevelManager.instance.coliSpeedRange.y);
        }
        else if (gameObject.name == "Sphaerogena(Clone)")
        {
            defaultSpeed = Random.Range(LevelManager.instance.sphaerogenaSpeedRange.x, LevelManager.instance.sphaerogenaSpeedRange.y);
        }
        if (Motile.playerInstance.agentCanMove) currentSpeed = defaultSpeed;
        else currentSpeed = defaultSpeed * LevelManager.instance.playerDeathSpeedMultiplier;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
