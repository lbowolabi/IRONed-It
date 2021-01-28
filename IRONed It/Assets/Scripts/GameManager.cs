using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public KeyCode viuaKey, irgaKey, hutaKey, fhuaKey;

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
                Player.instance.viuaKey = viuaKey = newKey;
                break;

            case "irga":
                Player.instance.irgaKey = irgaKey = newKey;
                break;

            case "huta":
                Player.instance.hutaKey = hutaKey = newKey;
                break;

            case "fhua":
                Player.instance.fhuaKey = fhuaKey = newKey;
                break;
        }
    }
}
