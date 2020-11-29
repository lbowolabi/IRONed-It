using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Motile))]

public class Enemy : MonoBehaviour
{
    [SerializeField] ChelatedBy targetType;
    Transform target;
    CircleCollider2D targetCollider;
    float maxSpeed;
    float currentSpeed;

    [Header("Component References")]
    Motile motile;
    SpriteRenderer sr;
    Color defaultColor;
    int defaultLayer;

    void Awake()
    {
        motile = GetComponent<Motile>();
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
        defaultLayer = LayerMask.NameToLayer("Default");
    }

    //private void OnEnable()
    //{
    //    // temporary speed pick

    //maxSpeed = currentSpeed = Random.Range(LevelManager.instance.choleraSpeedRange.x, LevelManager.instance.choleraSpeedRange.y);
    //}

    void Start()
    {
        maxSpeed = currentSpeed = -Random.Range(LevelManager.instance.choleraSpeedRange.x, LevelManager.instance.choleraSpeedRange.y);
    }

    void Update()
    {
        if (target == null) target = FindNearestIron();
        else if (target.position.x > transform.position.x || !target.parent.gameObject.activeInHierarchy)
        {
            if (LevelManager.instance.targetedIron.Contains(target.parent)) LevelManager.instance.targetedIron.Remove(target.parent);
            target = null;
        }
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.left * currentSpeed * Time.fixedDeltaTime);
        if (target != null)
        {
            if (Mathf.Abs(target.position.y - transform.position.y) <= targetCollider.bounds.size.x / 2 )
            {
                motile.SetMovementVector(Vector2.zero);
            }
            else
            {
                motile.SetMovementVector(new Vector2(0, target.position.y - transform.position.y));
            }
        }
        else
        {
            motile.SetMovementVector(Vector2.zero);
        }
    }

    Transform FindNearestIron()
    {
        GameObject[] iron = GameObject.FindGameObjectsWithTag("Iron");
        float nearestX = -100;
        Transform nearestIron = null;
        foreach (GameObject go in iron)
        {
            Iron ironScript = go.transform.parent.GetComponent<Iron>();
            if (go.layer == defaultLayer && (ironScript.chelatedBy == ChelatedBy.None || ironScript.chelatedBy == targetType) &&
                !LevelManager.instance.targetedIron.Contains(go.transform.parent) && go.transform.position.x < transform.position.x)
            {
                if (go.transform.position.x > nearestX)
                {
                    nearestX = go.transform.position.x;
                    nearestIron = go.transform;
                }
            }
        }
        if (nearestIron != null)
        {
            LevelManager.instance.targetedIron.Add(nearestIron.transform.parent);
            targetCollider = nearestIron.GetComponent<CircleCollider2D>();
        }
        return nearestIron;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        target = null;
        sr.color = defaultColor;
        motile.lives = 3;
    }

    void ChangeLifeCount(int amount) // gets called by motile
    {
        if (amount > 0)
        {
            Debug.Log("Enemy gained a life. That shouldn't happen.");
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

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.transform == target)
        {
            if (LevelManager.instance.targetedIron.Contains(target.parent)) LevelManager.instance.targetedIron.Remove(target.parent);
            target = null;
        }
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        if (c.CompareTag("Iron"))
        {
            var currentIron = c.transform.parent.GetComponent<Iron>();
            if (currentIron.chelatedBy == targetType)
            {
                c.transform.parent.gameObject.SetActive(false);
                target = null;
            }
        }
    }
}
