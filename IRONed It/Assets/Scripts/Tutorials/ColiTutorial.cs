using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpdateText))]

public class ColiTutorial : MonoBehaviour
{
    [SerializeField] private Transform coliPool;
    [SerializeField] int coliSpawnProbability;

    float wallSpeedSmoothing;

    UpdateText ut;

    void Awake()
    {
        ut = GetComponent<UpdateText>();
    }

    // Start is called before the first frame update
    void Start()
    {
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(false);
        StartCoroutine(WaitToStartTutorial());
    }

    IEnumerator WaitToStartTutorial()
    {
        float initialLevelLength = LevelManager.instance.levelLengthInSeconds;
        yield return new WaitUntil(() => LevelManager.instance.levelLengthInSeconds <= initialLevelLength / 2);
        LevelManager.instance.PauseLevelTimer();
        LevelManager.instance.StopAllResourceSpawns();
        Player.instance.expendingResources = false;
        yield return new WaitForSeconds(6);
        StartCoroutine(ColiIntro());
    }

    IEnumerator ColiIntro()
    {
        Player.instance.horizontalMovement = false;
        while (Player.instance.transform.position.x < -.5f || Player.instance.transform.position.x > .5f)
        {
            Player.instance.GetComponent<Motile>().SetMovementVector(new Vector2(Player.instance.transform.position.x < -.5f ? 1 : -1, Input.GetAxisRaw("Vertical")));
            yield return null;
        }
        float initialWallSpeed = LevelManager.instance.wallSpeed;
        LevelManager.instance.SpawnColi(0);
        yield return new WaitUntil(() => coliPool.childCount > 0);
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Here comes <i>E. Coli</i>! [Click]"));

        Transform coli = coliPool.GetChild(0);
        coli.GetComponent<TranslateSpeed>().StopMovement();
        while (coli.position.x > 3)
        {
            coli.Translate(Vector2.left * Time.deltaTime * 3);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, 2, ref wallSpeedSmoothing, 1);
            yield return null;
        }

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("It looks like <i>E. Coli</i> secretes linear enterobactin. [Click]"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Linear enterobactin chelates the same type of iron that I need to survive! [Click]"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Unfortunately, I can't pick up iron chelated by linear enterobactin using my trusty viuA gene."));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Instead, we'll have to express the irgA gene. [Click]"));
        CanvasManager.instance.GetIrgaButton().gameObject.SetActive(true);

        yield return new WaitUntil(() => ut.hasClicked);
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(false);
        Player.instance.canIrga = true;
        Player.instance.horizontalMovement = true;

        while (coli.gameObject.activeInHierarchy)
        {
            coli.transform.Translate(Vector2.left * Time.deltaTime * 10);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, initialWallSpeed, ref wallSpeedSmoothing, 1);
            yield return null;
        }

        Mutagen.instance.Spawn(coliSpawnProbability);
    }
}
