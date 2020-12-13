using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mutagen : MonoBehaviour
{
    int coliSpawnProbability;

    SpriteRenderer sr;
    BoxCollider2D bc;

    public static Mutagen instance;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
        instance = this;
    }

    public void Spawn(int _coliSpawnProbability)
    {
        coliSpawnProbability = _coliSpawnProbability;
        StartCoroutine(MoveMutagen());
    }

    IEnumerator MoveMutagen()
    {
        while (sr.enabled)
        {
            transform.Translate(Vector2.left * Time.deltaTime * 20);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player"))
        {
            CanvasManager.instance.GetViuaButton().image.color = Color.black;
            Player.instance.canViua = false;
            Player.instance.ChangeLifeCount(0);
            Player.instance.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            sr.enabled = false;
            bc.enabled = false;
            transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            StartCoroutine(Helpers.instance.Timer(x => RestartResourceSpawns(), 3));
        }
    }

    void RestartResourceSpawns()
    {
        LevelManager.instance.SetAllResourceSpawnsToDefault();
        LevelManager.instance.SetColiSpawnProbability(false, coliSpawnProbability);
        LevelManager.instance.SetCholeraSpawnProbability(false, 7000);
        LevelManager.instance.levelProgressing = true;
        Player.instance.expendingResources = true;
    }
}
