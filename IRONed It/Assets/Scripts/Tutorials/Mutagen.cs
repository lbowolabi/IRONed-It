using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mutagen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CanvasManager.instance.GetTutorialText().gameObject.SetActive(false);
        StartCoroutine(WaitToSpawn());
    }

    IEnumerator WaitToSpawn()
    {
        float initialLevelLength = LevelManager.instance.levelLengthInSeconds;
        yield return new WaitUntil(() => LevelManager.instance.levelLengthInSeconds <= initialLevelLength * .4f);
        while (gameObject.activeInHierarchy)
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
            gameObject.SetActive(false);
        }
    }
}
