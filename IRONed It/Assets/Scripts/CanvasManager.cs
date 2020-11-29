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
    [SerializeField] Button viua;
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
        return viua;
    }

    private void Awake()
    {
        instance = this;
    }

    //private void Start()
    //{
    //    viua.onClick.AddListener(() => Player.instance.ActivateGene());
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
