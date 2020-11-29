﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iron : MonoBehaviour
{
    // parent is on Iron layer, not trigger, tagged anything, has Iron script
    // child is on Default layer, isTrigger, tagged Iron
    // child collides with player, parent collides with particles

    public ChelatedBy chelatedBy { get; private set; } = ChelatedBy.None;
    float speed;

    CircleCollider2D cc;
    SpriteRenderer childSR;

    private void Awake()
    {
        cc = GetComponent<CircleCollider2D>();
        childSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
        childSR.enabled = false;
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.left * speed * Time.fixedDeltaTime);
    }

    private void OnParticleCollision(GameObject p)
    {
        if (p.name == "Vibriobactin")
        {
            chelatedBy = ChelatedBy.Cholera;
        }
        cc.isTrigger = true; // this object can't collide with particles again
        childSR.transform.localScale *= 1.5f;
        childSR.enabled = true;
    }

    void OnDisable() // reset values for reuse in next spawn
    {
        chelatedBy = ChelatedBy.None;
        cc.isTrigger = false;
        childSR.enabled = false;
        childSR.transform.localScale = Vector3.one;
        if (LevelManager.instance.targetedIron.Contains(transform)) LevelManager.instance.targetedIron.Remove(transform);
    }

    private void OnEnable()
    {
        speed = Random.Range(LevelManager.instance.ironSpeedRange.x, LevelManager.instance.ironSpeedRange.y);
    }
}
