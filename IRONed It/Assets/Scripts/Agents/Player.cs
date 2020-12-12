using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ActiveGene { None, viuA, irgA, hutA, fhuA };

[RequireComponent(typeof(Motile))]

public class Player : MonoBehaviour
{
    [Header("Resources")]
    TextMeshProUGUI lifeCount;
    Image fe3BarFill;
    [SerializeField] private float fe3LossRateOverTime;
    [SerializeField] private float fe3PickupWorth;
    Image atpBarFill;
    //public float atpLossRateOverTime;
    [SerializeField] private float atpPickupWorth;
    [SerializeField] private float atpCostToActivateGene;
    IEnumerator atpShake;
    Vector3 atpBarStartPosition;
    IEnumerator geneDurationTimer;

    [Header("Player States")]
    [HideInInspector] public bool expendingResources = true;
    [HideInInspector] public bool playerCanAct = true;
    public ActiveGene activeGene { get; private set; } = ActiveGene.None;
    public bool canViua = true, canIrga = true, canHuta = true, canFhua = true;

    [Header("Component References")]
    Motile motile;
    SpriteRenderer sr;
    Color defaultColor;
    [HideInInspector] public KeyCode viuaKey, irgaKey, hutaKey, fhuaKey;

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
    }

    void Update()
    {
        if (expendingResources)
        {
            ChangeIronCount(-Time.deltaTime * fe3LossRateOverTime);
            if (CanvasManager.instance.GetFe3BarFill().fillAmount == 0 && motile.lives > 0) ChangeLifeCount(-3);
            //ChangeEnergyCount(-Time.deltaTime * atpLossRateOverTime);
        }

        if (Input.GetKeyDown(viuaKey)) // to make configurable later
        {
            if (playerCanAct) ActivateViua();
        }
        else if (Input.GetKeyDown(irgaKey))
        {
            if (playerCanAct) ActivateIrga();
        }
        else if (Input.GetKeyDown(hutaKey))
        {
            if (playerCanAct) ActivateHuta();
        }
        else if (Input.GetKeyDown(fhuaKey))
        {
            if (playerCanAct) ActivateFhua();
        }
    }

    private void FixedUpdate()
    {
        motile.SetMovementVector(Vector2.up * Input.GetAxisRaw("Vertical"));
    }

    void ChangeActiveGene(ActiveGene newGene)
    {
        switch (activeGene)
        {
            case ActiveGene.None:
                break;
            case ActiveGene.viuA:
                CanvasManager.instance.GetViuaButton().image.color = canViua ? Color.white : Color.black;
                break;
            case ActiveGene.irgA:
                CanvasManager.instance.GetIrgaButton().image.color = canIrga ? Color.white : Color.black;
                break;
            case ActiveGene.hutA:
                CanvasManager.instance.GetHutaButton().image.color = canHuta ? Color.white : Color.black;
                break;
            case ActiveGene.fhuA:
                CanvasManager.instance.GetFhuaButton().image.color = canFhua ? Color.white : Color.black;
                break;
        }

        switch (newGene)
        {
            case ActiveGene.None:
                break;
            case ActiveGene.viuA:
                CanvasManager.instance.GetViuaButton().image.color = Color.grey;
                break;
            case ActiveGene.irgA:
                CanvasManager.instance.GetIrgaButton().image.color = Color.grey;
                break;
            case ActiveGene.hutA:
                CanvasManager.instance.GetHutaButton().image.color = Color.grey;
                break;
            case ActiveGene.fhuA:
                CanvasManager.instance.GetFhuaButton().image.color = Color.grey;
                break;
        }

        activeGene = newGene;
    }

    public void ActivateViua()
    {
        if (atpBarFill.fillAmount > atpCostToActivateGene && canViua)
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
        if (atpBarFill.fillAmount > atpCostToActivateGene && canIrga)
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
        if (atpBarFill.fillAmount > atpCostToActivateGene && canHuta)
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
        if (atpBarFill.fillAmount > atpCostToActivateGene && canFhua)
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
            while (motile.deathFlashInterval < 3)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSeconds(motile.deathFlashInterval);
                motile.deathFlashInterval *= 1.3f;
            }
            sr.color = Color.grey;
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
                if (activeGene == ActiveGene.viuA && currentIron.chelatedBy == ChelatedBy.Cholera)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                    ChangeIronCount(fe3PickupWorth);
                }
                else if (activeGene == ActiveGene.irgA && currentIron.chelatedBy == ChelatedBy.Coli)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                    ChangeIronCount(fe3PickupWorth);
                }
                else if (activeGene == ActiveGene.hutA && currentIron.chelatedBy == ChelatedBy.Heme)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                    ChangeIronCount(fe3PickupWorth);
                }
                else if (activeGene == ActiveGene.fhuA && currentIron.chelatedBy == ChelatedBy.Sphaerogena)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    LevelManager.instance.RemoveFromActiveObjects(c.transform.parent.gameObject);
                    ChangeIronCount(fe3PickupWorth);
                }
            }
        }
    }
}
