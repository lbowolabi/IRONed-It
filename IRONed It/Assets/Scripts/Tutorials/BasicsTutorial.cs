﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    IEnumerator IronIntro() // all concrete numbers are guesstimates <.< will need to tweak
    {
        yield return null;
        CanvasManager.instance.GetTutorialText().gameObject.SetActive(true);
        StartCoroutine(ut.UpdateTutorialText("Use the up and down arrows or W and S to move.<br>(Click anywhere to continue.)"));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("This is iron. Vibrio need iron to stay alive."));
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

        StartCoroutine(ut.UpdateTutorialText("As your body uses up iron, your iron bar decreases."));
        cm.GetFe3BarFill().transform.parent.parent.gameObject.SetActive(true);
        yield return new WaitUntil(() => ut.hasClicked);

        StartCoroutine(ut.UpdateTutorialText("Vibrio can't always pick up free-floating iron."));
        yield return new WaitUntil(() => ut.hasClicked);

        ironSpeed = defaultIronSpeed;
        yield return new WaitForSeconds(.2f);

        StartCoroutine(ut.UpdateTutorialText("Sometimes you'll get lucky!"));
        yield return new WaitUntil(() => iron.transform.position.x < Player.instance.transform.position.x - 5);

        StartCoroutine(ut.UpdateTutorialText("Not this time, though."));
        iron.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = true;
        iron.GetComponent<CircleCollider2D>().enabled = true;
        yield return new WaitUntil(() => ut.hasClicked);

        StartCoroutine(ChelatedIronIntro());
    }

    IEnumerator ChelatedIronIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("To pick up iron efficiently, you need two things."));
        yield return new WaitUntil(() => ut.hasClicked);

        vibriobactin.Play();
        yield return new WaitForSeconds(.2f);

        StartCoroutine(ut.UpdateTutorialText("The first thing is vibriobactin! It chelates iron and allows efficient uptake."));
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
            if (Input.GetMouseButtonDown(0)) break;
            yield return null;
        }
        string s = GameManager.instance.viuaKey.ToString();
        if (s.Contains("Alpha")) s = s.Remove(0, 5);
        StartCoroutine(ut.UpdateTutorialText("But to pick up chelated iron, you must first express your viuA gene. Press " + s + "."));
        cm.GetGeneDisplay().SetActive(true);
        Player.instance.canViua = true;
        // active gene ui, animate it etc

        yield return new WaitUntil(() => player.activeGene == ActiveGene.viuA);
        yield return new WaitForSeconds(.5f);
        StartCoroutine(ut.UpdateTutorialText("With the viuA gene, your chance of vibriobactin chelated-iron uptake is 100%."));
        ironSpeed = defaultIronSpeed;

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(EnergyIntro());
    }

    IEnumerator EnergyIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("Expressing the viuA gene depleted some of your energy."));
        cm.GetAtpBarFill().transform.parent.parent.gameObject.SetActive(true);
        // animate it etc

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Collect ATP to replenish your energy stock."));

        float timer = .2f;
        while (timer > 0)
        {
            lm.SpawnEnergy();
            timer -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(ClampPlayerEnergy());

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("You don't need to do anything special to pick up ATP. Just swim into it."));

        yield return new WaitForSeconds(3);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        StartCoroutine(DoxyIntro());
    }

    IEnumerator ClampPlayerEnergy()
    {
        float timer = 3;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            cm.GetAtpBarFill().fillAmount = Mathf.Clamp(cm.GetAtpBarFill().fillAmount, 0, .9f);
            yield return null;
        }
    }

    IEnumerator DoxyIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("Be careful! You may also encounter antibiotics!"));
        yield return new WaitUntil(() => ut.hasClicked);
        lm.SpawnDoxy();

        yield return new WaitForSeconds(.5f);
        StartCoroutine(ut.UpdateTutorialText("This is doxycycline. It inhibits protein production."));

        yield return new WaitForSeconds(3);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        float timer = .2f;
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
        StartCoroutine(ut.UpdateTutorialText("You're a cholera bacterium, so antibiotics are bad for you."));

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("Your movement is powered by your ability to make proteins. You can't move after doxycycline damages you."));
        
        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(LifeIntro());
    }

    IEnumerator LifeIntro()
    {
        StartCoroutine(ut.UpdateTutorialText("You have three attempts. It's game over once you lose them all."));
        cm.GetLifeCountText().gameObject.SetActive(true);

        yield return new WaitUntil(() => ut.hasClicked);
        StartCoroutine(ut.UpdateTutorialText("But you can recover a lost life! Just fully replenish your energy."));

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
            if (Input.GetMouseButtonDown(0)) break;
            yield return null;
        }
        StartCoroutine(ut.UpdateTutorialText("That's the end of the tutorial. Press ESC to open the pause menu and exit."));
    }
}