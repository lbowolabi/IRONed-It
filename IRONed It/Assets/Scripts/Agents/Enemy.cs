using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Motile))]

public class Enemy : MonoBehaviour
{
    [SerializeField] ChelatedBy targetType;

    [Header("Component References")]
    Motile motile;
    SpriteRenderer sr;
    Color defaultColor;

    void Awake()
    {
        motile = GetComponent<Motile>();
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
    }

    private void OnEnable()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    void ChangeLifeCount(int amount)
    {
        if (amount > 0)
        {

        }
        else if (amount < 0)
        {
            StartCoroutine(Death());
        }

        motile.lives += amount;
        motile.lives = Mathf.Clamp(motile.lives, 0, 3);
    }

    IEnumerator Death()
    {
        motile.agentCanMove = false;
        motile.iFrames = true;
        if (motile.lives != 0)
        {
            StartCoroutine(Helpers.instance.Timer(revive => motile.agentCanMove = true, motile.deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => motile.iFrames = false, motile.deathDuration));
            while (!motile.agentCanMove)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(motile.deathFlashInterval);
            }
            sr.color = defaultColor;
        }
        else
        {
            while (motile.deathFlashInterval < 3)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(motile.deathFlashInterval);
                motile.deathFlashInterval *= 1.3f;
            }
            sr.color = Color.grey;
        }
    }
}
