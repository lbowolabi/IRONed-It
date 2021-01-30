using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public KeyCode viuaKey, irgaKey, hutaKey, fhuaKey;
    public Color viuaColor = Color.cyan, irgaColor = Color.blue, hutaColor = Color.white, fhuaColor = new Color(111, 255, 107, 1);

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null) // if unique
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // don't destroy on load
            Debug.LogError("This message will make the console appear in Development Builds");
        }
        else // if not unique
        {
            Destroy(gameObject); // destroy 
            return;
        }
    }

    public void UpdateKeyBinding(string gene, KeyCode newKey)
    {
        switch (gene)
        {
            case "viua":
                viuaKey = newKey;
                if (Player.instance != null) Player.instance.viuaKey = newKey;
                break;

            case "irga":
                irgaKey = newKey;
                if (Player.instance != null) Player.instance.irgaKey = newKey;
                break;

            case "huta":
                hutaKey = newKey;
                if (Player.instance != null) Player.instance.hutaKey = newKey;
                break;

            case "fhua":
                fhuaKey = newKey;
                if (Player.instance != null) Player.instance.fhuaKey = newKey;
                break;
        }
    }
}
