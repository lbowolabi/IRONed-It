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
    public GameObject timer;

    [Header("Tutorials")]
    [SerializeField] TextMeshProUGUI tutorialText;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    [SerializeField] TextMeshProUGUI[] viuaKeyText, irgaKeyText, hutaKeyText, fhuaKeyText;
    Dictionary<string, bool> changeBindings = new Dictionary<string, bool>() {
        { "viua", false },
        { "irga", false },
        { "huta", false },
        { "fhua", false }
    };

    [Header("Level End")]
    public GameObject endMenu;

    [Header("Death Screen")]
    public GameObject deathMenu;

    [Header("Endless Title")]
    public GameObject endlessMenu;

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
        geneButtons[0].onClick.AddListener(() => Player.instance.ActivateViua());
        geneButtons[1].onClick.AddListener(() => Player.instance.ActivateIrga());
        geneButtons[2].onClick.AddListener(() => Player.instance.ActivateHuta());
        geneButtons[3].onClick.AddListener(() => Player.instance.ActivateFhua());

        for (int i = 0; i < viuaKeyText.Length; i++)
        {
            viuaKeyText[i].text = GameManager.instance.viuaKey.ToString();
            if (viuaKeyText[i].text.Contains("Alpha")) viuaKeyText[i].text = viuaKeyText[i].text.Remove(0, 5);
        }
        GetViuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("viuA (" + viuaKeyText[0].text + ")");

        for (int i = 0; i < irgaKeyText.Length; i++)
        {
            irgaKeyText[i].text = GameManager.instance.irgaKey.ToString();
            if (irgaKeyText[i].text.Contains("Alpha")) irgaKeyText[i].text = irgaKeyText[i].text.Remove(0, 5);
        }
        GetIrgaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("irgA (" + irgaKeyText[0].text + ")");

        for (int i = 0; i < hutaKeyText.Length; i++)
        {
            hutaKeyText[i].text = GameManager.instance.hutaKey.ToString();
            if (hutaKeyText[i].text.Contains("Alpha")) hutaKeyText[i].text = hutaKeyText[i].text.Remove(0, 5);
        }
        GetHutaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("hutA (" + hutaKeyText[0].text + ")");

        for (int i = 0; i < fhuaKeyText.Length; i++)
        {
            fhuaKeyText[i].text = GameManager.instance.fhuaKey.ToString();
            if (fhuaKeyText[i].text.Contains("Alpha")) fhuaKeyText[i].text = fhuaKeyText[i].text.Remove(0, 5);
        }
        GetFhuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("fhuA (" + fhuaKeyText[0].text + ")");

        if (SceneManager.GetActiveScene().name == "Endless")
        {
            Time.timeScale = 0;
            endlessMenu.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !endMenu.activeInHierarchy && !deathMenu.activeInHierarchy && !endlessMenu.activeInHierarchy && SceneManager.GetActiveScene().name != "Settings")
        {
            Player.instance.playerCanAct = pauseMenu.activeInHierarchy ? true : false;
            genes.SetActive(pauseMenu.activeInHierarchy);
            Time.timeScale = pauseMenu.activeInHierarchy ? 1 : 0;
            pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
            ChangeKeyBinding();
        }
        else
        {
            if (pauseMenu.activeInHierarchy || deathMenu.activeInHierarchy)
            {
                foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(k))
                    {
                        if (changeBindings["viua"])
                        {
                            GameManager.instance.UpdateKeyBinding("viua", k);
                            for (int i = 0; i < viuaKeyText.Length; i++)
                            {
                                viuaKeyText[i].text = GameManager.instance.viuaKey.ToString();
                                if (viuaKeyText[i].text.Contains("Alpha")) viuaKeyText[i].text = viuaKeyText[i].text.Remove(0, 5);
                            }
                            GetViuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("viuA (" + viuaKeyText[0].text + ")");
                            ChangeKeyBinding();
                        }
                        else if (changeBindings["irga"])
                        {
                            GameManager.instance.UpdateKeyBinding("irga", k);
                            for (int i = 0; i < irgaKeyText.Length; i++)
                            {
                                irgaKeyText[i].text = GameManager.instance.irgaKey.ToString();
                                if (irgaKeyText[i].text.Contains("Alpha")) irgaKeyText[i].text = irgaKeyText[i].text.Remove(0, 5);
                            }
                            GetIrgaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("irgA (" + irgaKeyText[0].text + ")");
                            ChangeKeyBinding();
                        }
                        else if (changeBindings["huta"])
                        {
                            GameManager.instance.UpdateKeyBinding("huta", k);
                            for (int i = 0; i < hutaKeyText.Length; i++)
                            {
                                hutaKeyText[i].text = GameManager.instance.hutaKey.ToString();
                                if (hutaKeyText[i].text.Contains("Alpha")) hutaKeyText[i].text = hutaKeyText[i].text.Remove(0, 5);
                            }
                            GetHutaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("hutA (" + hutaKeyText[0].text + ")");
                            ChangeKeyBinding();
                        }
                        else if (changeBindings["fhua"])
                        {
                            GameManager.instance.UpdateKeyBinding("fhua", k);
                            for (int i = 0; i < fhuaKeyText.Length; i++)
                            {
                                fhuaKeyText[i].text = GameManager.instance.fhuaKey.ToString();
                                if (fhuaKeyText[i].text.Contains("Alpha")) fhuaKeyText[i].text = fhuaKeyText[i].text.Remove(0, 5);
                            }
                            GetFhuaButton().GetComponentInChildren<TextMeshProUGUI>().text = string.Format("fhuA (" + fhuaKeyText[0].text + ")");
                            ChangeKeyBinding();
                        }
                        break;
                    }
                }
            }
        }
    }

    public void Unpause()
    {
        Player.instance.playerCanAct = true;
        genes.SetActive(true);
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        ChangeKeyBinding();
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
                    for (int i = 0; i < viuaKeyText.Length; i++)
                    {
                        viuaKeyText[i].gameObject.SetActive(false);
                    }
                    break;

                case "irga":
                    for (int i = 0; i < irgaKeyText.Length; i++)
                    {
                        irgaKeyText[i].gameObject.SetActive(false);
                    }
                    break;

                case "huta":
                    for (int i = 0; i < hutaKeyText.Length; i++)
                    {
                        hutaKeyText[i].gameObject.SetActive(false);
                    }
                    break;

                case "fhua":
                    for (int i = 0; i < fhuaKeyText.Length; i++)
                    {
                        fhuaKeyText[i].gameObject.SetActive(false);
                    }
                    break;
            }
        }
        else
        {
            for (int i = 0; i < viuaKeyText.Length; i++)
            {
                viuaKeyText[i].gameObject.SetActive(true);
                irgaKeyText[i].gameObject.SetActive(true);
                hutaKeyText[i].gameObject.SetActive(true);
                fhuaKeyText[i].gameObject.SetActive(true);
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
    }

    public void LoadLevel(string nextLevel)
    {
        SceneManager.LoadScene(nextLevel);
    }
}
