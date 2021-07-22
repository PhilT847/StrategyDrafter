using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftRounds : MonoBehaviour
{
    private int DisplayIndex;

    public Image[] DisplayCircles;

    public void SelectNextCircle()
    {
        DisplayCircles[DisplayIndex].color = Color.gray;

        if (DisplayIndex < 5)
        {
            DisplayIndex++;
        }
    }
}
