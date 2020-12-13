using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateText : MonoBehaviour
{
    public IEnumerator UpdateTutorialText(string newText)
    {
        TextMeshProUGUI tutorialText = CanvasManager.instance.GetTutorialText();
        tutorialText.text = newText;
        tutorialText.maxVisibleCharacters = 0;

        bool special = false;
        int startIndex = 0;
        string unformatted = newText;
        for (int i = 0; i < unformatted.Length; i++) // loop through current line string and take out formatting, only leave plaintext
        { // need this to get the actual length of all characters in the string without extra characters added by formatting, like <i>
            if (!special)
            {
                if (unformatted[i] == '<')
                {
                    special = true;
                    startIndex = i;
                }
            }
            else
            {
                if (newText[i] == '>')
                {
                    unformatted = unformatted.Remove(startIndex, i + 1 - startIndex);
                    special = false;
                    i = startIndex;
                }
            }
        }

        for (int i = 0; i < unformatted.Length; i++)
        {
            yield return new WaitUntil(() => Time.timeScale != 0);
            //string updatedLine = tutorialText.text; // <-- commented-out section increases size of last character in string
            //if (i > 0) // <-- problems: doesn't work on formatted text, looks janky because there's no smoothing/transition between sizes
            //{
            //    updatedLine = updatedLine.Remove(i - 1, 10);
            //    updatedLine = updatedLine.Remove(i, 7);
            //}
            //updatedLine = updatedLine.Insert(i + 1, "</size>");
            //updatedLine = updatedLine.Insert(i, "<size=+30>");
            //tutorialText.text = updatedLine;
            tutorialText.maxVisibleCharacters++;
            yield return null;
        }
        //tutorialText.text = tutorialText.text.Remove(unformatted.Length - 1, 10);
        //tutorialText.text = tutorialText.text.Remove(unformatted.Length, 7);
    }
}
