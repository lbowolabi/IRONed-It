using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadLevel(int _level)
    {
        SceneManager.LoadScene(string.Format("Level_" + _level.ToString()));
    }
}
