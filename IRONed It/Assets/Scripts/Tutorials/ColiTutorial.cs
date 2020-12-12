using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(UpdateText))]

public class ColiTutorial : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Transform coliPool;
    [SerializeField] int coliSpawnProbability;

    float wallSpeedSmoothing;

    UpdateText ut;

    void Awake()
    {
        ut = GetComponent<UpdateText>();
        ut.SetTutorialText(tutorialText);
        tutorialText.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitToStartTutorial());
    }

    IEnumerator WaitToStartTutorial()
    {
        float initialLevelLength = LevelManager.instance.levelLengthInSeconds;
        yield return new WaitUntil(() => LevelManager.instance.levelLengthInSeconds <= initialLevelLength / 2);
        LevelManager.instance.levelProgressing = false;
        LevelManager.instance.StopAllResourceSpawns();
        Player.instance.expendingResources = false;
        yield return new WaitForSeconds(6);
        StartCoroutine(ColiIntro());
    }

    IEnumerator ColiIntro()
    {
        float initialWallSpeed = LevelManager.instance.wallSpeed;
        LevelManager.instance.SpawnColi();
        yield return new WaitUntil(() => coliPool.childCount > 0);
        tutorialText.gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Here comes an <i>E. Coli</i> bacterium!"));

        Transform coli = coliPool.GetChild(0);
        coli.GetComponent<TranslateSpeed>().StopMovement();
        while (coli.position.x > 3)
        {
            coli.transform.Translate(Vector2.left * Time.deltaTime * 3);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, 2, ref wallSpeedSmoothing, 1);
            yield return null;
        }

        yield return new WaitForSeconds(4);
        StartCoroutine(ut.UpdateTutorialText("Unlike you and your vibriobactin, <i>E. Coli</i> secrete linear enterobactin."));

        yield return new WaitForSeconds(6);
        StartCoroutine(ut.UpdateTutorialText("Linear enterobactin is a ferric iron chelator Fe(III)."));

        yield return new WaitForSeconds(5);
        StartCoroutine(ut.UpdateTutorialText("You can't pick up iron chelated by linear enterobactin with viuA."));

        yield return new WaitForSeconds(4);
        StartCoroutine(ut.UpdateTutorialText("Instead, you'll have to express your irgA gene."));
        CanvasManager.instance.GetIrgaButton().gameObject.SetActive(true);

        yield return new WaitForSeconds(6);
        tutorialText.gameObject.SetActive(false);
        LevelManager.instance.SetAllResourceSpawnsToDefault();
        LevelManager.instance.SetColiSpawnProbability(false, coliSpawnProbability);
        LevelManager.instance.SetCholeraSpawnProbability(false, 7000);
        LevelManager.instance.levelProgressing = true;
        Player.instance.canIrga = true;
        Player.instance.expendingResources = true;

        while (coli.gameObject.activeInHierarchy)
        {
            coli.transform.Translate(Vector2.left * Time.deltaTime * 10);
            LevelManager.instance.wallSpeed = Mathf.SmoothDamp(LevelManager.instance.wallSpeed, initialWallSpeed, ref wallSpeedSmoothing, 1);
            yield return null;
        }
    }
}
