using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpdateText))]

public class BasicsTutorial : MonoBehaviour
{
    [SerializeField] float ironSpeed;

    LevelManager lm;
    Player player;
    CanvasManager cm;
    UpdateText ut;

    public ParticleSystem vibriobactin;

    void Awake()
    {
        ut = GetComponent<UpdateText>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lm = LevelManager.instance;
        player = Player.instance;
        cm = CanvasManager.instance;

        lm.StopAllResourceSpawns();

        player.expendingResources = false;
        vibriobactin.Stop();

        cm.GetFe3BarFill().transform.parent.parent.gameObject.SetActive(false);
        cm.GetAtpBarFill().transform.parent.parent.gameObject.SetActive(false);
        cm.GetLifeCountText().gameObject.SetActive(false);
        cm.GetGeneDisplay().SetActive(false);

        StartCoroutine(IronIntro());
    }

    IEnumerator MoveIron(GameObject iron)
    {
        while (iron.activeInHierarchy)
        {
            iron.transform.Translate(Vector2.left * Time.deltaTime * ironSpeed);
            yield return null;
        }
    }

    IEnumerator IronIntro()
    {
        yield return null;
        CanvasManager.instance.GetTutorialText().transform.parent.gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Hi! I'm Vibrio Cholerae, but my friends call me Vibrio! It seems I've found myself in someone's intestine. Use your arrows keys or WASD keys to help me move.<br>[Click anywhere to continue.]"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("[During tutorials, horizontal movement is temporarily disabled.] [Click.]"));
        player.horizontalMovement = false;
        while (player.transform.position.x < -.5f || player.transform.position.x > .5f)
        {
            player.GetComponent<Motile>().SetMovementVector(new Vector2(player.transform.position.x < -.5f ? 1 : -1, Input.GetAxisRaw("Vertical")));
            yield return null;
        }
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) && !CanvasManager.instance.pauseMenu.activeInHierarchy);
        StartCoroutine(ut.UpdateTutorialText("This is iron and I need it to stay alive."));
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
        // animate the iron bar, etc, something to draw attention to it
        yield return new WaitUntil(() => ut.hasClicked);

        StartCoroutine(ut.UpdateTutorialText("As my body uses up iron, the iron status bar decreases. [Click]"));
        cm.GetFe3BarFill().transform.parent.parent.gameObject.SetActive(true);
        yield return new WaitUntil(() => ut.hasClicked);

        StartCoroutine(ut.UpdateTutorialText("Sadly, I can't always pick up free-floating iron. [Click]"));
        yield return new WaitUntil(() => ut.hasClicked);

        ironSpeed = defaultIronSpeed;
        yield return new WaitForSeconds(.2f);

        StartCoroutine(ut.UpdateTutorialText("Sometimes I get lucky!"));
        yield return new WaitUntil(() => iron.transform.position.x < Player.instance.transform.position.x - 5);

        StartCoroutine(ut.UpdateTutorialText("Not this time, though. [Click]"));
        iron.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = true;
        iron.GetComponent<CircleCollider2D>().enabled = true;
        yield return new WaitUntil(() => ut.hasClicked);

        StartCoroutine(ChelatedIronIntro());
    }

    IEnumerator ChelatedIronIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("To help me pick up iron efficiently, I need two things. [Click]"));
        yield return new WaitUntil(() => ut.hasClicked);

        vibriobactin.Play();
        yield return new WaitForSeconds(.2f);

        StartCoroutine(ut.UpdateTutorialText("The first is vibriobactin! It chelates iron and allows for efficient uptake."));
        lm.SpawnIron(false, 0);
        //yield return null;
        GameObject[] ironSearch = GameObject.FindGameObjectsWithTag("Iron");
        GameObject iron = null;
        while (ironSearch == null)
        {
            ironSearch = GameObject.FindGameObjectsWithTag("Iron");
            yield return null;
        }
        foreach (GameObject i in ironSearch)
        {
            if (LayerMask.LayerToName(i.layer) == "Iron" && i.transform.position.x > 0)
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
        float timer = 6;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (Input.GetMouseButtonDown(0) && !CanvasManager.instance.pauseMenu.activeInHierarchy) break;
            yield return null;
        }
        string s = GameManager.instance.viuaKey.ToString();
        if (s.Contains("Alpha")) s = s.Remove(0, 5);
        StartCoroutine(ut.UpdateTutorialText("But to pick up chelated iron, you must help me express my viuA gene. [Press " + s + ".]"));
        cm.GetGeneDisplay().SetActive(true);
        Player.instance.canViua = true;
        // active gene ui, animate it etc

        yield return new WaitUntil(() => player.activeGene == ActiveGene.viuA);
        yield return new WaitForSeconds(.5f);
        StartCoroutine(ut.UpdateTutorialText("With the viuA gene, my chances of vibriobactin-iron uptake is 100%! [Click]"));
        ironSpeed = defaultIronSpeed;

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(EnergyIntro());
    }

    IEnumerator EnergyIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("Oh goodness, Expressing the viuA gene depleted some of your energy. [Click]"));
        cm.GetAtpBarFill().transform.parent.parent.gameObject.SetActive(true);
        // animate it etc

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Help me collect ATP to replenish my energy stock. [Click]"));

        float timer = 1f;
        StartCoroutine(ClampPlayerEnergy());
        while (timer > 0)
        {
            lm.SpawnEnergy();
            timer -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("We don't need to do anything special to pick up ATP. We just swim into it! [Click]"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(DoxyIntro());
    }

    IEnumerator ClampPlayerEnergy()
    {
        float timer = 5;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            cm.GetAtpBarFill().fillAmount = Mathf.Clamp(cm.GetAtpBarFill().fillAmount, 0, .9f);
            yield return null;
        }
    }

    IEnumerator DoxyIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("Be careful! I've heard of scary things like antibiotics. [Click]"));
        yield return new WaitUntil(() => ut.hasClicked);
        lm.SpawnDoxy();

        yield return new WaitForSeconds(.5f);
        StartCoroutine(ut.UpdateTutorialText("There's one now: Doxyxycline! It inhibits my ability to make proteins. [Click]"));

        yield return new WaitUntil(() => ut.hasClicked);

        float timer = .4f;
        while (timer > 0)
        {
            lm.SpawnDoxy();
            timer -= Time.deltaTime;
            yield return null;
        }
        var playerMovemt = player.GetComponent<Motile>();
        yield return new WaitUntil(() => !playerMovemt.agentCanMove);
        StartCoroutine(ut.UpdateTutorialText("Ouch!"));

        yield return new WaitUntil(() => playerMovemt.agentCanMove);
        StartCoroutine(ut.UpdateTutorialText("My life and my ability to move are supported by the proteins I make. [Click]"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("When doxycycline hits me, I can't move for a bit and if dozycycline hits me multiple times, I'll die. [Click]"));
        
        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(LifeIntro());
    }

    IEnumerator LifeIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("Looks like we've got three attemps in total. It's game over after that. [Click]"));
        cm.GetLifeCountText().gameObject.SetActive(true);

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("But wait, we can recover a lost life! We only need to fully replenish my energy!"));

        while (cm.GetAtpBarFill().fillAmount < 1)
        {
            lm.SpawnEnergy();
            yield return null;
        }
        StartCoroutine(ut.UpdateTutorialText("Hoorah!"));

        float timer = 3;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (Input.GetMouseButtonDown(0) && !CanvasManager.instance.pauseMenu.activeInHierarchy) break;
            yield return null;
        }
        StartCoroutine(ut.UpdateTutorialText("That's the end of the tutorial. Press ESC to open the pause menu and exit."));
    }
}