using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateText : MonoBehaviour
{
    public IEnumerator UpdateTutorialText(string newText)
    {
        CanvasManager.instance.GetTutorialText().text = newText;
        yield return null;
    }
}
