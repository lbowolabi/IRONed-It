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
        new WaitForSeconds(6);
        Player.instance.horizontalMovement = false;
        while (Player.instance.transform.position.x < -.5f || Player.instance.transform.position.x > .5f)
        {
            Player.instance.GetComponent<Motile>().SetMovementVector(new Vector2(Player.instance.transform.position.x < -.5f ? 1 : -1, Input.GetAxisRaw("Vertical")));
            yield return null;
        }
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
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Hey look, that's heme-chelated iron! [Click]"));
        iron.GetComponent<TranslateSpeed>().StopMovement();
        while (iron.transform.position.x > 4)
        {
            iron.transform.Translate(Vector2.left * Time.deltaTime * 3);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, 2, ref wallSpeedSmoothing, 1);
            yield return null;
        }

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Heme comes from the person or animal that we're floating in. [Click]"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("To pick up heme, you'll have to express my hutA gene. [Click]"));
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
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(false);
        LevelManager.instance.UnpauseLevelTimer();
        Player.instance.expendingResources = true;
        Player.instance.horizontalMovement = true;
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
