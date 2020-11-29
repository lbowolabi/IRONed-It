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
    IEnumerator geneDurationTimer;

    // player states
    [HideInInspector] public bool expendingResources = true;
    public ActiveGene activeGene { get; private set; } = ActiveGene.None;

    [Header("Component References")]
    Motile motile;
    SpriteRenderer sr;
    Color defaultColor;

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
        lifeCount.text = motile.lives.ToString();
    }

    void Update()
    {
        if (expendingResources)
        {
            ChangeIronCount(-Time.deltaTime * fe3LossRateOverTime);
            //ChangeEnergyCount(-Time.deltaTime * atpLossRateOverTime);
        }
    }

    private void FixedUpdate()
    {
        motile.SetMovementVector(Vector2.up * Input.GetAxisRaw("Vertical"));
    }

    public void ActivateViua()
    {
        if (fe3BarFill.fillAmount > 0)
        {
            if (activeGene != ActiveGene.viuA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                activeGene = ActiveGene.viuA;
                ChangeEnergyCount(-atpCostToActivateGene);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => activeGene = ActiveGene.None, 10);
                StartCoroutine(geneDurationTimer);
            }
        }
    }

    public void ActiveIrga()
    {
        if (fe3BarFill.fillAmount > 0)
        {
            if (activeGene != ActiveGene.irgA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                activeGene = ActiveGene.irgA;
                ChangeEnergyCount(-atpCostToActivateGene);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => activeGene = ActiveGene.None, 10);
                StartCoroutine(geneDurationTimer);
            }
        }
    }

    public void ActivateHuta()
    {
        if (fe3BarFill.fillAmount > 0)
        {
            if (activeGene != ActiveGene.hutA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                activeGene = ActiveGene.hutA;
                ChangeEnergyCount(-atpCostToActivateGene);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => activeGene = ActiveGene.None, 10);
                StartCoroutine(geneDurationTimer);
            }
        }
    }

    public void ActivateFhua()
    {
        if (fe3BarFill.fillAmount > 0)
        {
            if (activeGene != ActiveGene.fhuA)
            {
                if (geneDurationTimer != null) StopCoroutine(geneDurationTimer);
                activeGene = ActiveGene.fhuA;
                ChangeEnergyCount(-atpCostToActivateGene);
                geneDurationTimer = Helpers.instance.Timer(deactivateGene => activeGene = ActiveGene.None, 10);
                StartCoroutine(geneDurationTimer);
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
                LevelManager.instance.SetEnergySpawnProbability(false); // turn off energy spawn if player at max energy
            }
        }
        else if (amount < 0)
        {
            if (atpBarFill.fillAmount == 1)
            {
                LevelManager.instance.SetEnergySpawnProbability(true); // turn energy spawn back on
            }
        }

        atpBarFill.fillAmount += amount;
    }

    public void EnergyPickup() // gets called by motile
    {
        ChangeEnergyCount(atpPickupWorth);
    }

    public void ChangeLifeCount(int amount) // also gets called by motile
    {
        if (amount > 0)
        {

        }
        else if (amount < 0)
        {
            StartCoroutine(Death());
        }

        motile.lives += amount;
        motile.lives = Mathf.Clamp(motile.lives, 0, 3);
        lifeCount.text = motile.lives.ToString();
    }

    IEnumerator Death()
    {
        motile.agentCanMove = false;
        expendingResources = false;
        motile.iFrames = true;
        GetComponent<CapsuleCollider2D>().isTrigger = true;

        if (motile.lives != 0)
        {
            StartCoroutine(Helpers.instance.Timer(revive => motile.agentCanMove = true, motile.deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => expendingResources = true, motile.deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => motile.iFrames = false, motile.deathDuration));
            StartCoroutine(Helpers.instance.Timer(revive => GetComponent<CapsuleCollider2D>().isTrigger = false, motile.deathDuration));
            while (!motile.agentCanMove)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(motile.deathFlashInterval);
            }
            sr.color = defaultColor;
        }
        else
        {
            while (motile.deathFlashInterval < 3)
            {
                sr.color = sr.color == Color.grey ? defaultColor : Color.grey;
                yield return new WaitForSecondsRealtime(motile.deathFlashInterval);
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
                    ChangeIronCount(fe3PickupWorth);
                }
                else if (activeGene == ActiveGene.irgA && currentIron.chelatedBy == ChelatedBy.Coli)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    ChangeIronCount(fe3PickupWorth);
                }
                else if (activeGene == ActiveGene.hutA && currentIron.chelatedBy == ChelatedBy.Heme)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    ChangeIronCount(fe3PickupWorth);
                }
                else if (activeGene == ActiveGene.fhuA && currentIron.chelatedBy == ChelatedBy.Sphaerogena)
                {
                    c.transform.parent.gameObject.SetActive(false);
                    ChangeIronCount(fe3PickupWorth);
                }
            }
        }
    }
}
