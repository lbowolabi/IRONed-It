using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] Image fe3BarFill;
    [SerializeField] Image atpBarFill;
    [SerializeField] TextMeshProUGUI lifeCount;
    [SerializeField] List<Button> geneButtons;
    [SerializeField] GameObject genes;

    public static CanvasManager instance;

    public Image GetFe3BarFill()
    {
        return fe3BarFill;
    }

    public Image GetAtpBarFill()
    {
        return atpBarFill;
    }

    public TextMeshProUGUI GetLifeCountText()
    {
        return lifeCount;
    }

    public GameObject GetGeneDisplay()
    {
        return genes;
    }

    public Button GetViuaButton()
    {
        return geneButtons[0];
    }

    private void Awake()
    {
        instance = this;
    }

    //private void Start()
    //{
    //    geneButtons[0].onClick.AddListener(() => Player.instance.ActivateViua());
    //    geneButtons[1].onClick.AddListener(() => Player.instance.ActivateIrga());
    //    geneButtons[2].onClick.AddListener(() => Player.instance.ActivateHuta());
    //    geneButtons[3].onClick.AddListener(() => Player.instance.ActivateFhua());
    //}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = Time.timeScale == 1 ? 0 : 1;
        }
    }
}
