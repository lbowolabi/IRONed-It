using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateText : MonoBehaviour
{
    private TextMeshProUGUI tutorialText;

    public void SetTutorialText(TextMeshProUGUI t)
    {
        tutorialText = t;
    }

    public IEnumerator UpdateTutorialText(string newText)
    {
        tutorialText.text = newText;
        yield return null;
    }
}
