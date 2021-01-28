using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpdateText))]

public class SphaerogenaTutorial : MonoBehaviour
{
    [SerializeField] private Transform sphaerogenaPool;
    [SerializeField] int sphaerogenaSpawnProbability;
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
        StartCoroutine(SphaerogenaIntro());
    }

    IEnumerator SphaerogenaIntro()
    {
        Player.instance.horizontalMovement = false;
        while (Player.instance.transform.position.x < -.5f || Player.instance.transform.position.x > .5f)
        {
            Player.instance.GetComponent<Motile>().SetMovementVector(new Vector2(Player.instance.transform.position.x < -.5f ? 1 : -1, Input.GetAxisRaw("Vertical")));
            yield return null;
        }
        float initialWallSpeed = LevelManager.instance.wallSpeed;
        LevelManager.instance.SpawnSphaerogena(0);
        yield return new WaitUntil(() => sphaerogenaPool.childCount > 0);
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Here comes a <i>U. sphaerogena</i> particle!"));

        Transform sph = sphaerogenaPool.GetChild(0);
        sph.GetComponent<TranslateSpeed>().StopMovement();
        while (sph.position.x > 3)
        {
            sph.Translate(Vector2.left * Time.deltaTime * 3);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, 2, ref wallSpeedSmoothing, 1);
            yield return null;
        }

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Unlike you and your vibriobactin, <i>U. sphaerogena</i> secrete ferrichrome."));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Like linear enterobactin, ferrichrome is a ferric iron chelator Fe(III)."));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("To pick up ferrichrome-chelated iron, you'll need to express your fhuA gene."));
        CanvasManager.instance.GetFhuaButton().gameObject.SetActive(true);
        CanvasManager.instance.GetFhuaButton().image.color = Color.yellow;

        yield return new WaitUntil(() => ut.hasClicked);
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(false);
        LevelManager.instance.SetAllResourceSpawnsToDefault();
        LevelManager.instance.SetSphaerogenaSpawnProbability(false, sphaerogenaSpawnProbability);
        LevelManager.instance.UnpauseLevelTimer();
        Player.instance.canFhua = true;
        CanvasManager.instance.GetFhuaButton().image.color = Color.white;
        Player.instance.horizontalMovement = true;
        Player.instance.expendingResources = true;

        while (sph.gameObject.activeInHierarchy)
        {
            sph.Translate(Vector2.left * Time.deltaTime * 10);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, initialWallSpeed, ref wallSpeedSmoothing, 1);
            yield return null;
        }
    }
}
