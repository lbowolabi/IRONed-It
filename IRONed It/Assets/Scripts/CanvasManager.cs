using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    [Header("In-game UI")]
    [SerializeField] Image fe3BarFill;
    [SerializeField] Image atpBarFill;
    [SerializeField] TextMeshProUGUI lifeCount;
    [SerializeField] Button[] geneButtons;
    [SerializeField] GameObject genes;

    [Header("Tutorials")]
    [SerializeField] TextMeshProUGUI tutorialText;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    [SerializeField] TextMeshProUGUI viuaKeyText, irgaKeyText, hutaKeyText, fhuaKeyText;
    Dictionary<string, bool> changeBindings = new Dictionary<string, bool>() {
        { "viua", false },
        { "irga", false },
        { "huta", false },
        { "fhua", false }
    };

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

    public Button GetIrgaButton()
    {
        return geneButtons[1];
    }

    public Button GetHutaButton()
    {
        return geneButtons[2];
    }

    public Button GetFhuaButton()
    {
        return geneButtons[3];
    }

    public TextMeshProUGUI GetTutorialText()
    {
        return tutorialText;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
        geneButtons[0].onClick.AddListener(() => Player.instance.ActivateViua());
        geneButtons[1].onClick.AddListener(() => Player.instance.ActivateIrga());
        geneButtons[2].onClick.AddListener(() => Player.instance.ActivateHuta());
        geneButtons[3].onClick.AddListener(() => Player.instance.ActivateFhua());

        viuaKeyText.text = GameManager.instance.viuaKey.ToString();
        if (viuaKeyText.text.Contains("Alpha")) viuaKeyText.text = viuaKeyText.text.Remove(0, 5);
        GetViuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("viuA ("+ viuaKeyText.text + ")");

        irgaKeyText.text = GameManager.instance.irgaKey.ToString();
        if (irgaKeyText.text.Contains("Alpha")) irgaKeyText.text = irgaKeyText.text.Remove(0, 5);
        GetIrgaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("irgA (" + irgaKeyText.text + ")");

        hutaKeyText.text = GameManager.instance.hutaKey.ToString();
        if (hutaKeyText.text.Contains("Alpha")) hutaKeyText.text = hutaKeyText.text.Remove(0, 5);
        GetHutaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("hutA (" + hutaKeyText.text + ")");

        fhuaKeyText.text = GameManager.instance.fhuaKey.ToString();
        if (fhuaKeyText.text.Contains("Alpha")) fhuaKeyText.text = fhuaKeyText.text.Remove(0, 5);
        GetFhuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("fhuA (" + fhuaKeyText.text + ")");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.instance.playerCanAct = pauseMenu.activeInHierarchy ? true : false;
            genes.SetActive(pauseMenu.activeInHierarchy);
            Time.timeScale = pauseMenu.activeInHierarchy ? 1 : 0;
            pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
            ChangeKeyBinding();
        }
        else
        {
            if (pauseMenu.activeInHierarchy)
            {
                foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(k))
                    {
                        if (changeBindings["viua"])
                        {
                            GameManager.instance.UpdateKeyBinding("viua", k);
                            viuaKeyText.text = k.ToString();
                            if (viuaKeyText.text.Contains("Alpha")) viuaKeyText.text = viuaKeyText.text.Remove(0, 5);
                            GetViuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("viuA (" + viuaKeyText.text + ")");
                            ChangeKeyBinding();
                        }
                        else if (changeBindings["irga"])
                        {
                            GameManager.instance.UpdateKeyBinding("irga", k);
                            irgaKeyText.text = k.ToString();
                            if (irgaKeyText.text.Contains("Alpha")) irgaKeyText.text = irgaKeyText.text.Remove(0, 5);
                            GetIrgaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("irgA (" + irgaKeyText.text + ")");
                            ChangeKeyBinding();
                        }
                        else if (changeBindings["huta"])
                        {
                            GameManager.instance.UpdateKeyBinding("huta", k);
                            hutaKeyText.text = k.ToString();
                            if (hutaKeyText.text.Contains("Alpha")) hutaKeyText.text = hutaKeyText.text.Remove(0, 5);
                            GetHutaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("hutA (" + hutaKeyText.text + ")");
                            ChangeKeyBinding();
                        }
                        else if (changeBindings["fhua"])
                        {
                            GameManager.instance.UpdateKeyBinding("fhua", k);
                            fhuaKeyText.text = k.ToString();
                            if (fhuaKeyText.text.Contains("Alpha")) fhuaKeyText.text = fhuaKeyText.text.Remove(0, 5);
                            GetFhuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("fhuA (" + fhuaKeyText.text + ")");
                            ChangeKeyBinding();
                        }
                        break;
                    }
                }
            }
        }
    }
    public void ChangeKeyBinding(string s = "")
    {
        s = s.ToLower();
        changeBindings["viua"] = false;
        changeBindings["irga"] = false;
        changeBindings["huta"] = false;
        changeBindings["fhua"] = false;
        if (s != "")
        {
            changeBindings[s] = true;
            switch (s)
            {
                case "viua":
                    viuaKeyText.gameObject.SetActive(false);
                    break;

                case "irga":
                    irgaKeyText.gameObject.SetActive(false);
                    break;

                case "huta":
                    hutaKeyText.gameObject.SetActive(false);
                    break;

                case "fhua":
                    fhuaKeyText.gameObject.SetActive(false);
                    break;
            }
        }
        else
        {
            viuaKeyText.gameObject.SetActive(true);
            irgaKeyText.gameObject.SetActive(true);
            hutaKeyText.gameObject.SetActive(true);
            fhuaKeyText.gameObject.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
