using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ActiveGene { None, viuA, irgA, hutA, fhuA };

[RequireComponent(typeof(Motile))]

public class Player : MonoBehaviour
{
    [HideInInspector] public bool horizontalMovement = true;
    [Header("Resources")]
    [SerializeField] private float fe3LossRateOverTime;
    [SerializeField] private float fe3PickupWorth;
    Image atpBarFill;
    //public float atpLossRateOverTime;
    [SerializeField] private float atpPickupWorth;
    [SerializeField] private float atpCostToActivateGene;
    TextMeshProUGUI lifeCount;
    Image fe3BarFill;
    IEnumerator atpShake;
    Vector3 atpBarStartPosition;
    IEnumerator geneDurationTimer;

    [Header("Player States")]
    public bool canViua = true;
    public bool canIrga = true, canHuta = true, canFhua = true;
    public ActiveGene activeGene { get; private set; } = ActiveGene.None;
    [HideInInspector] public bool expendingResources = true;
    [HideInInspector] public bool playerCanAct = true;

    [Header("Component References")]
    [SerializeField] ParticleSystem geneEndNotif;
    Motile motile;
    SpriteRenderer sr;
    Color defaultColor;
    Camera mainCam;
    [HideInInspector] public KeyCode viuaKey, irgaKey, hutaKey, fhuaKey;
    Dictionary<Transform, Vector3> buttonLocations = new Dictionary<Transform, Vector3>();

    public static Player instance;

    public float GetAtpPickupWorth()
    {
        return atpPickupWorth;
    }

    void Awake()
    {
        instance = this;
        motile = GetComponent<Motile>();
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
        horizontalMovement = true;
    }

    private void Start()
    {
        lifeCount = CanvasManager.instance.GetLifeCountText();
        fe3BarFill = CanvasManager.instance.GetFe3BarFill();
        atpBarFill = CanvasManager.instance.GetAtpBarFill();
        lifeCount.text = string.Format("lives: " + motile.lives.ToString());

        viuaKey = GameManager.instance.viuaKey;
        irgaKey = GameManager.instance.irgaKey;
        hutaKey = GameManager.instance.hutaKey;
        fhuaKey = GameManager.instance.fhuaKey;

        CanvasManager.instance.GetViuaButton().image.color = GameManager.instance.viuaColor;
        CanvasManager.instance.GetIrgaButton().image.color = GameManager.instance.irgaColor;
        CanvasManager.instance.GetHutaButton().image.color = GameManager.instance.hutaColor;
        CanvasManager.instance.GetFhuaButton().image.color = GameManager.instance.fhuaColor;

        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        if (expendingResources)
        {
            ChangeIronCount(-Time.deltaTime * fe3LossRateOverTime);
            if (CanvasManager.instance.GetFe3BarFill().fillAmount == 0 && motile.lives > 0) ChangeLifeCount(-3);
            //ChangeEnergyCount(-Time.deltaTime * atpLossRateOverTime);
        }

        if (Input.GetKeyDown(viuaKey))
        {
            ActivateViua();
        }
        else if (Input.GetKeyDown(irgaKey))
        {
            ActivateIrga();
        }
        else if (Input.GetKeyDown(hutaKey))
        {
            ActivateHuta();
        }
        else if (Input.GetKeyDown(fhuaKey))
        {
            ActivateFhua();
        }
    }

    private void FixedUpdate()
    {
        if (!horizontalMovement) motile.SetMovementVector(Vector2.up * Input.GetAxisRaw("Vertical"));
        else
        {
            motile.SetMovementVector(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
            if (motile.agentCanMove)
            {
                if (transform.position.x > mainCam.ViewportToWorldPoint(new Vector2(1, 0)).x)
                {
                    transform.position = new Vector2(mainCam.ViewportToWorldPoint(new Vector2(1, 0)).x, transform.position.y);
                }
                else if (transform.position.x < mainCam.ViewportToWorldPoint(new Vector2(0, 0)).x)
                {
                    transform.position = new Vector2(mainCam.ViewportToWorldPoint(new Vector2(0, 0)).x, transform.position.y);
                }
            }
        }
    }

    void ChangeActiveGene(ActiveGene newGene)
    {
        switch (activeGene)
        {
            case ActiveGene.None:
                break;
            case ActiveGene.viuA:
                CanvasManager.instance.GetViuaButton().image.color = canViua ? GameManager.instance.viuaColor : Color.black;
                CanvasManager.instance.GetViuaButton().transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
                break;
            case ActiveGene.irgA:
                CanvasManager.instance.GetIrgaButton().image.color = canIrga ? GameManager.instance.irgaColor : Color.black;
                CanvasManager.instance.GetIrgaButton().transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
                break;
            case ActiveGene.hutA:
                CanvasManager.instance.GetHutaButton().image.color = canHuta ? GameManager.instance.hutaColor : Color.black;
                CanvasManager.instance.GetHutaButton().transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
                break;
            case ActiveGene.fhuA:
                CanvasManager.instance.GetFhuaButton().image.color = canFhua ? GameManager.instance.fhuaColor : Color.black;
                CanvasManager.instance.GetFhuaButton().transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
                break;
        }

        switch (newGene)
        {
            case ActiveGene.None:
                break;
            case ActiveGene.viuA:
                StartCoroutine(GeneTimerIndicator(CanvasManager.instance.GetViuaButton().transform.GetChild(1).GetComponent<Image>()));
                break;
            case ActiveGene.irgA:
                StartCoroutine(GeneTimerIndicator(CanvasManager.instance.GetIrgaButton().transform.GetChild(1).GetComponent<Image>()));
                break;
            case ActiveGene.hutA:
                StartCoroutine(GeneTimerIndicator(CanvasManager.instance.GetHutaButton().transform.GetChild(1).GetComponent<Image>()));
                break;
            case ActiveGene.fhuA:
                StartCoroutine(GeneTimerIndicator(CanvasManager.instance.GetFhuaButton().transform.GetChild(1).GetComponent<Image>()));
                break;
        }

        activeGene = newGene;
    }

    IEnumerator GeneTimerIndicator(Image geneFill)
    {
        if (!buttonLocations.ContainsKey(geneFill.transform.parent)) buttonLocations.Add(geneFill.transform.parent, geneFill.transform.parent.localPosition);
        geneFill.fillAmount = 1;
        while (geneFill.fillAmount > 0)
        {
            geneFill.transform.parent.localPosition = buttonLocations[geneFill.transform.parent];
            geneFill.fillAmount -= Time.deltaTime / 10;
            yield return null;
        }
        var m = geneEndNotif.main;
        m.startColor = geneFill.transform.parent.GetComponent<Image>().color;
        geneEndNotif.Play();
        StartCoroutine(Helpers.instance.Shake(geneFill.transform.parent, .08f, 1.6f));
    }

    public void ActivateViua()
    {
        if (atpBarFill.fillAmount > atpCostToActivateGene && canViua && playerCanAct)
        {
            if (activeGene != ActiveGene.viuA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                ChangeActiveGene(ActiveGene.viuA);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => ChangeActiveGene(ActiveGene.None), 10);
                StartCoroutine(geneDurationTimer);
                ChangeEnergyCount(-atpCostToActivateGene);
            }
        }
    }

    public void ActivateIrga()
    {
        if (atpBarFill.fillAmount > atpCostToActivateGene && canIrga && playerCanAct)
        {
            if (activeGene != ActiveGene.irgA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                ChangeActiveGene(ActiveGene.irgA);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => ChangeActiveGene(ActiveGene.None), 10);
                StartCoroutine(geneDurationTimer);
                ChangeEnergyCount(-atpCostToActivateGene);
            }
        }
    }

    public void ActivateHuta()
    {
        if (atpBarFill.fillAmount > atpCostToActivateGene && canHuta && playerCanAct)
        {
            if (activeGene != ActiveGene.hutA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                ChangeActiveGene(ActiveGene.hutA);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => ChangeActiveGene(ActiveGene.None), 10);
                StartCoroutine(geneDurationTimer);
                ChangeEnergyCount(-atpCostToActivateGene);
            }
        }
    }

    public void ActivateFhua()
    {
        if (atpBarFill.fillAmount > atpCostToActivateGene && canFhua && playerCanAct)
        {
            if (activeGene != ActiveGene.fhuA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                ChangeActiveGene(ActiveGene.fhuA);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => ChangeActiveGene(ActiveGene.None), 10);
                StartCoroutine(geneDurationTimer);
                ChangeEnergyCount(-atpCostToActivateGene);
            }
        }
    }

    void ChangeIronCount(float amount)
    {
        if (amount > 0)
        {

        }
        else if (amount < 0)
        {

        }

        fe3BarFill.fillAmount += amount;
    }

    public void IronPickup() // gets called by motile
    {
        ChangeIronCount(fe3PickupWorth);
    }

    public void ChangeEnergyCount(float amount)
    {
        if (amount > 0)
        {
            if (atpBarFill.fillAmount + amount >= 1)
            {
                if (motile.lives < 3 && atpBarFill.fillAmount != 1)
                {
                    ChangeLifeCount(1);
                }
            }
        }
        else if (amount < 0)
        {
            if (atpShake != null)
            {
                StopCoroutine(atpShake);
                atpBarFill.transform.parent.position = atpBarStartPosition;
            }
            else
            {
                atpBarStartPosition = atpBarFill.transform.parent.position;
            }
            atpShake = Helpers.instance.Shake(atpBarFill.transform.parent, .1f, 2.5f);
            StartCoroutine(atpShake);
        }

        atpBarFill.fillAmount += amount;
    }

    public void EnergyPickup()
    {
        ChangeEnergyCount(atpPickupWorth);
    }

    public void ChangeLifeCount(int amount)
    {
        motile.lives += amount;
        motile.lives = Mathf.Clamp(motile.lives, 0, 3);
        lifeCount.text = string.Format("lives: " + motile.lives.ToString());
        if (amount > 0)
        {

        }
        else if (amount <= 0)
        {
            StartCoroutine(Death());
        }
    }

    IEnumerator Death()
    {
        motile.agentCanMove = false;
        expendingResources = false;
        playerCanAct = false;
        motile.iFrames = true;
        GetComponent<CapsuleCollider2D>().isTrigger = true;
        LevelManager.instance.PlayerCanMove(false);

        if (motile.lives != 0)
        {
            StartCoroutine(Helpers.instance.Timer(revive => motile.agentCanMove = playerCanAct = expendingResources = true, motile.deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => GetComponent<CapsuleCollider2D>().isTrigger = false, motile.deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => motile.iFrames = false, motile.deathDuration + .2f));
            while (!motile.agentCanMove)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSeconds(motile.deathFlashInterval);
            }
            sr.color = defaultColor;
            LevelManager.instance.PlayerCanMove(true);
        }
        else
        {
            LevelManager.instance.PauseLevelTimer();
            while (motile.deathFlashInterval < 3)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSeconds(motile.deathFlashInterval);
                motile.deathFlashInterval *= 1.3f;
            }
            sr.color = Color.grey;
            yield return new WaitForSeconds(1);
            CanvasManager.instance.deathMenu.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Victory"))
        {
            motile.agentCanMove = false;
            expendingResources = false;
            GetComponent<CapsuleCollider2D>().isTrigger = true;
            LevelManager.instance.EndLevel();
        }
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        if (c.CompareTag("Iron"))
        {
            var currentIron = c.transform.parent.GetComponent<Iron>();
            if (currentIron.chelatedBy != ChelatedBy.None)
            {
                if ((activeGene == ActiveGene.viuA && currentIron.chelatedBy == ChelatedBy.Cholera) ||
                    (activeGene == ActiveGene.irgA && currentIron.chelatedBy == ChelatedBy.Coli) ||
                    (activeGene == ActiveGene.hutA && currentIron.chelatedBy == ChelatedBy.Heme) ||
                    (activeGene == ActiveGene.fhuA && currentIron.chelatedBy == ChelatedBy.Sphaerogena))
                {
                    c.transform.parent.gameObject.SetActive(false);
                    LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                    ChangeIronCount(fe3PickupWorth);
                }
            }
        }
    }
}
