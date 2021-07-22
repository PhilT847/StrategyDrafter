using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public RectTransform TopBar;
    public RectTransform BottomBar;

    public Image CritBlinder;

    public CombatStatDisplay PlayerInfo;
    public CombatStatDisplay EnemyInfo;

    public Unit InitiatingUnit; //the unit that initiated combat. Used in CombatStatDisplay functions
    //public Unit ReceivingUnit;

    [HideInInspector] public bool UI_Activated;

    public IEnumerator ToggleBattleUI(Unit Attacker, Unit Defender) //Moves the cinematic bars. Automatically toggles in/out based on whether or not it's already out.
    {
        if (UI_Activated) 
        {
            PlayerInfo.gameObject.SetActive(true); //add CombatStatDisplays to the screen and set them.
            EnemyInfo.gameObject.SetActive(true);

            PlayerInfo.SetCombatStatDisplay(Attacker, Defender);
            EnemyInfo.SetCombatStatDisplay(Defender, Attacker);

            float TimeElapsed = 0f;

            while (TimeElapsed < 2f)
            {
                TopBar.anchoredPosition = Vector2.Lerp(TopBar.anchoredPosition, Vector2.zero, TimeElapsed / 8f);
                BottomBar.anchoredPosition = Vector2.Lerp(BottomBar.anchoredPosition, Vector2.zero, TimeElapsed / 8f);

                TimeElapsed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            TopBar.anchoredPosition = Vector2.zero;
            BottomBar.anchoredPosition = Vector2.zero;
        }
        else
        {
            PlayerInfo.gameObject.SetActive(false); //remove displays
            EnemyInfo.gameObject.SetActive(false);

            float TimeElapsed = 0f;

            while (TimeElapsed < 2)
            {
                TopBar.anchoredPosition = Vector2.Lerp(TopBar.anchoredPosition, new Vector2(0f, 150f), TimeElapsed / 8f);
                BottomBar.anchoredPosition = Vector2.Lerp(BottomBar.anchoredPosition, new Vector2(0f, -150f), TimeElapsed / 8f);

                TimeElapsed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            TopBar.anchoredPosition = new Vector2(0f, 150f);
            BottomBar.anchoredPosition = new Vector2(0f, -150f);
        }

        yield return null;
    }
}
