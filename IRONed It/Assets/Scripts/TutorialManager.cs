using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] float ironSpeed;

    LevelManager lm;
    Player player;
    CanvasManager cm;

    public ParticleSystem vibriobactin;

    // Start is called before the first frame update
    void Start()
    {
        lm = LevelManager.instance;
        player = Player.instance;
        cm = CanvasManager.instance;

        // turn off all resource spawns
        lm.SetDoxySpawnProbability(false);
        lm.SetEnergySpawnProbability(false);
        lm.SetIronSpawnProbability(false);

        player.expendingResources = false;
        vibriobactin.Stop();
        StartCoroutine(Helpers.instance.WaitOneFrame(decreaseEnergy => player.ChangeEnergyCount(-2 * player.GetAtpPickupWorth())));

        cm.GetFe3BarFill().transform.parent.gameObject.SetActive(false);
        cm.GetAtpBarFill().transform.parent.gameObject.SetActive(false);
        cm.GetLifeCountText().gameObject.SetActive(false);
        cm.GetGeneDisplay().SetActive(false);

        StartCoroutine(IronIntro());
    }

    IEnumerator UpdateTutorialText(string newText)
    {
        tutorialText.text = newText;
        yield return null;
    }

    IEnumerator MoveIron(GameObject iron)
    {
        while (iron.activeInHierarchy)
        {
            iron.transform.Translate(Vector2.left * Time.deltaTime * ironSpeed);
            yield return null;
        }
    }

    IEnumerator IronIntro() // all concrete numbers are guesstimates <.< will need to tweak
    {
        yield return null;
        StartCoroutine(UpdateTutorialText("Use the up and down arrows to move."));
        lm.SpawnIron(false, 0);
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
        iron.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = false; // player is not allowed to pick this one up
        iron.GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(MoveIron(iron));

        yield return new WaitUntil(() => iron.transform.position.x < 3);

        float defaultIronSpeed = ironSpeed;
        ironSpeed = 0;
        StartCoroutine(UpdateTutorialText("This is iron. Vibrio need iron to stay alive."));
        cm.GetFe3BarFill().transform.parent.gameObject.SetActive(true);
        // animate the iron bar, etc, something to draw attention to it
        yield return new WaitForSecondsRealtime(4);

        StartCoroutine(UpdateTutorialText("As your body uses up iron, your iron bar decreases."));
        yield return new WaitForSecondsRealtime(4);

        StartCoroutine(UpdateTutorialText("Vibrio can't always pick up free-floating iron."));
        yield return new WaitForSecondsRealtime(3.5f);

        ironSpeed = defaultIronSpeed;
        yield return new WaitForSeconds(.5f);

        StartCoroutine(UpdateTutorialText("Sometimes you'll get lucky!"));
        yield return new WaitUntil(() => iron.transform.position.x < CameraFollow.instance.GetComponent<Camera>().ViewportToWorldPoint(Vector2.zero).x);

        StartCoroutine(UpdateTutorialText("Not this time, though."));
        iron.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = true;
        iron.GetComponent<CircleCollider2D>().enabled = true;
        yield return new WaitForSeconds(2);

        StartCoroutine(ChelatedIronIntro());
    }

    IEnumerator ChelatedIronIntro()
    {
        StartCoroutine(UpdateTutorialText("To pick up iron efficiently, you need two things."));
        yield return new WaitForSeconds(3.5f);

        vibriobactin.Play();
        yield return new WaitForSeconds(.5f);

        StartCoroutine(UpdateTutorialText("The first thing is vibriobactin! It chelates iron and allows efficient uptake."));
        lm.SpawnIron(false, 0);
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
        StartCoroutine(MoveIron(iron));

        yield return new WaitUntil(() => iron.transform.position.x < 3);
        float defaultIronSpeed = ironSpeed;
        ironSpeed = 0;

        yield return new WaitUntil(() => iron.GetComponent<Iron>().chelatedBy != ChelatedBy.None);
        yield return new WaitForSeconds(2);
        StartCoroutine(UpdateTutorialText("But to pick up chelated iron, you must first activate your viuA gene. Press Q."));
        cm.GetGeneDisplay().SetActive(true);
        // active gene ui, animate it etc

        LevelManager.instance.SpawnCholera();
        yield return new WaitUntil(() => player.activeGene == ActiveGene.viuA);
        yield return new WaitForSeconds(3.5f);
        StartCoroutine(UpdateTutorialText("With the viuA gene, your chance of iron uptake is 100%."));
        ironSpeed = defaultIronSpeed;

        yield return new WaitUntil(() => !iron.activeInHierarchy);
        yield return new WaitForSeconds(4);
        StartCoroutine(EnergyIntro());
    }

    IEnumerator EnergyIntro()
    {
        StartCoroutine(UpdateTutorialText("Expressing the viuA gene depleted some of your energy."));

        yield return new WaitForSeconds(2);
        cm.GetAtpBarFill().transform.parent.gameObject.SetActive(true);
        // animate it etc

        yield return new WaitForSeconds(2);
        StartCoroutine(UpdateTutorialText("Collect ATP to replenish your energy stock."));

        float timer = .2f;
        while (timer > 0)
        {
            lm.SpawnEnergy();
            timer -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(ClampPlayerEnergy());

        yield return new WaitForSeconds(4);
        StartCoroutine(UpdateTutorialText("You don't need to do anything special to pick up ATP. Just swim into it."));

        yield return new WaitForSeconds(4);
        StartCoroutine(DoxyIntro());
    }

    IEnumerator ClampPlayerEnergy()
    {
        while (lm.energy.particleCount > 0)
        {
            cm.GetAtpBarFill().fillAmount = Mathf.Clamp(cm.GetAtpBarFill().fillAmount, 0, .9f);
            yield return null;
        }
    }

    IEnumerator DoxyIntro()
    {
        StartCoroutine(UpdateTutorialText("Be careful! You may also encounter antibiotics!"));
        yield return new WaitForSeconds(3);
        lm.SpawnDoxy();

        yield return new WaitForSeconds(1);
        StartCoroutine(UpdateTutorialText("This is doxycycline. It inhibits your protein production."));
        yield return new WaitForSeconds(4);

        float timer = .2f;
        while (timer > 0)
        {
            lm.SpawnDoxy();
            timer -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(UpdateTutorialText("Ouch!"));

        var playerMovemt = player.GetComponent<Motile>();
        yield return new WaitUntil(() => playerMovemt.agentCanMove);
        StartCoroutine(UpdateTutorialText("You're a cholera bacterium, so antibiotics are bad for you."));

        yield return new WaitForSeconds(4);
        StartCoroutine(UpdateTutorialText("Your movement is powered by your ability to make proteins. You can't move after doxycycline damages you."));

        yield return new WaitForSeconds(4);
        StartCoroutine(LifeIntro());
    }

    IEnumerator LifeIntro()
    {
        StartCoroutine(UpdateTutorialText("You have three lives. It's game over once you lose them all."));
        cm.GetLifeCountText().gameObject.SetActive(true);

        yield return new WaitForSeconds(4);
        StartCoroutine(UpdateTutorialText("But you can recover a lost life! Just fully replenish your energy."));

        while (cm.GetAtpBarFill().fillAmount < 1)
        {
            lm.SpawnEnergy();
            yield return null;
        }
        StartCoroutine(UpdateTutorialText("Hoorah!"));

        yield return new WaitForSeconds(2);
        StartCoroutine(UpdateTutorialText("That's the end of the tutorial."));

        yield return new WaitForSeconds(4);
    }
}