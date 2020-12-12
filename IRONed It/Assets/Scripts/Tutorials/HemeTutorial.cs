using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HemeTutorial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HemeIntro());   
    }

    IEnumerator HemeIntro()
    {
        float initialWallSpeed = LevelManager.instance.wallSpeed;
        LevelManager.instance.wallSpeed = 2;
        yield return null;
    }
}
