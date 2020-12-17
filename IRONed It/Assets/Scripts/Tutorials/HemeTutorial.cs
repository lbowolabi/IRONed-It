using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpdateText))]

public class HemeTutorial : MonoBehaviour
{
    float wallSpeedSmoothing;
    bool running;

    UpdateText ut;

    void Awake()
    {
        ut = GetComponent<UpdateText>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HemeIntro());   
    }

    IEnumerator HemeIntro()
    {
        running = true;
        StartCoroutine(NoSpawns());
        LevelManager.instance.PauseLevelTimer();
        Player.instance.expendingResources = false;
        float initialWallSpeed = LevelManager.instance.wallSpeed;
        yield return null;
        LevelManager.instance.SpawnIron(true, 0);
        GameObject[] ironSearch = GameObject.FindGameObjectsWithTag("Iron");
        GameObject iron = null;
        while (ironSearch == null)
        {
            ironSearch = GameObject.FindGameObjectsWithTag("Iron");
            yield return null;
        }
        foreach (GameObject i in ironSearch)
        {
            if (LayerMask.LayerToName(i.layer) == "Iron")
            {
                iron = i;
                break;
            }
        }
        CanvasManager.instance.GetTutorialText().gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Here comes some heme-chelated iron!"));
        iron.GetComponent<TranslateSpeed>().StopMovement();
        while (iron.transform.position.x > 4)
        {
            iron.transform.Translate(Vector2.left * Time.deltaTime * 3);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, 2, ref wallSpeedSmoothing, 1);
            yield return null;
        }

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Sometimes you'll come across heme-chelated iron floating around."));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("To pick it up, you'll have to express your hutA gene."));
        CanvasManager.instance.GetHutaButton().gameObject.SetActive(true);
        Player.instance.canHuta = true;

        while (!ut.hasClicked)
        {
            if (Player.instance.activeGene == ActiveGene.hutA)
            {
                break;
            }
            yield return null;
        }
        CanvasManager.instance.GetTutorialText().gameObject.SetActive(false);
        LevelManager.instance.UnpauseLevelTimer();
        Player.instance.expendingResources = true;
        running = false;

        while (iron.activeInHierarchy)
        {
            iron.transform.Translate(Vector2.left * Time.deltaTime * 10);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, initialWallSpeed, ref wallSpeedSmoothing, 1);
            yield return null;
        }
    }

    IEnumerator NoSpawns()
    {
        while (running)
        {
            LevelManager.instance.StopAllResourceSpawns();
            yield return null;
        }
        LevelManager.instance.SetAllResourceSpawnsToDefault();
    }
}
