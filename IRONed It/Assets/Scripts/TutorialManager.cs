using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public float ironSpeed;

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
        player.fe3LossRateOverTime = 0;
        vibriobactin.Stop();
        StartCoroutine(Helpers.instance.WaitOneFrame(decreaseEnergy => player.ChangeEnergyCount(-2 * player.atpPickupWorth)));

        cm.fe3BarFill.transform.parent.gameObject.SetActive(false);
        cm.atpBarFill.transform.parent.gameObject.SetActive(false);
        cm.lifeCount.gameObject.SetActive(false);
        cm.genes.SetActive(false);

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
        StartCoroutine(UpdateTutorialText("This is iron."));
        lm.SpawnIron();
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
        iron.GetComponent<CircleCollider2D>().enabled = false; // player is not allowed to pick this one up
        StartCoroutine(MoveIron(iron));

        yield return new WaitUntil(() => iron.transform.position.x < 3);

        float defaultIronSpeed = ironSpeed;
        ironSpeed = 0;
        StartCoroutine(UpdateTutorialText("Collect iron to replenish your iron bar!"));
        cm.fe3BarFill.transform.parent.gameObject.SetActive(true);
        // animate the iron bar, etc, something to draw attention to it
        yield return new WaitForSecondsRealtime(4);

        StartCoroutine(UpdateTutorialText("Your iron bar automatically depletes over time. (You're safe in this tutorial, though.)"));
        yield return new WaitForSecondsRealtime(4);

        StartCoroutine(UpdateTutorialText("But you can't usually pick up free-floating iron while it's in this form."));
        yield return new WaitForSecondsRealtime(3.5f);

        ironSpeed = defaultIronSpeed;
        yield return new WaitForSeconds(.5f);

        StartCoroutine(UpdateTutorialText("Sometimes you'll get lucky!"));
        yield return new WaitUntil(() => !iron.activeInHierarchy);

        StartCoroutine(UpdateTutorialText("Not this time, though."));
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

        StartCoroutine(UpdateTutorialText("These are vibriobactin! When they touch iron, they create chelated iron."));
        lm.SpawnIron();
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

        yield return new WaitUntil(() => iron.GetComponent<Iron>().chelated);
        yield return new WaitForSeconds(1);
        StartCoroutine(UpdateTutorialText("To pick up chelated iron, first activate your viuA gene."));
        cm.genes.SetActive(true);
        // active gene ui, animate it etc

        yield return new WaitUntil(() => player.viuaActive);
        yield return new WaitForSeconds(.5f);
        StartCoroutine(UpdateTutorialText("You have a 100% chance of picking up chelated iron while your viuA gene is active."));
        ironSpeed = defaultIronSpeed;

        yield return new WaitUntil(() => !iron.activeInHierarchy);
        yield return new WaitForSeconds(4);
        StartCoroutine(EnergyIntro());
    }

    IEnumerator EnergyIntro()
    {
        StartCoroutine(UpdateTutorialText("Activating your viuA gene depleted some of your energy."));

        yield return new WaitForSeconds(2);
        cm.atpBarFill.transform.parent.gameObject.SetActive(true);
        // animate it etc

        yield return new WaitForSeconds(2);
        StartCoroutine(UpdateTutorialText("Collect ATP to replenish some energy."));

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
            cm.atpBarFill.fillAmount = Mathf.Clamp(cm.atpBarFill.fillAmount, 0, .9f);
            yield return null;
        }
    }

    IEnumerator DoxyIntro()
    {
        StartCoroutine(UpdateTutorialText("You'll also encounter antibiotics!"));
        yield return new WaitForSeconds(3);
        lm.SpawnDoxy();

        yield return new WaitForSeconds(1);
        StartCoroutine(UpdateTutorialText("This is doxycycline. It kills you."));
        yield return new WaitForSeconds(4);

        float timer = .2f;
        while (timer > 0)
        {
            lm.SpawnDoxy();
            timer -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(UpdateTutorialText("Ouch!"));

        yield return new WaitUntil(() => player.playerCanMove);
        StartCoroutine(UpdateTutorialText("You're a cholera bacterium, so antibiotics are bad for you."));

        yield return new WaitForSeconds(4);
        StartCoroutine(LifeIntro());
    }

    IEnumerator LifeIntro()
    {
        StartCoroutine(UpdateTutorialText("You have three lives. It's game over once you lose them all."));
        cm.lifeCount.gameObject.SetActive(true);

        yield return new WaitForSeconds(4);
        StartCoroutine(UpdateTutorialText("But you can recover a lost life! Just fully replenish your energy."));

        while (cm.atpBarFill.fillAmount < 1)
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