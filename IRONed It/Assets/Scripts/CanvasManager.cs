using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public Image fe3BarFill;
    public Image atpBarFill;
    public TextMeshProUGUI lifeCount;
    public Button viua;
    public GameObject genes;

    public static CanvasManager instance;

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
