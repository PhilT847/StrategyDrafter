using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatStatDisplay : MonoBehaviour
{
    public Unit DisplayedUnit;

    public TextMeshProUGUI UnitName;
    public TextMeshProUGUI UnitStatValues;

    public TextMeshProUGUI HealthValue;
    public RectTransform HealthFiller;

    public RectTransform BarrierIndicator;
    public TextMeshProUGUI BarrierValue;

    //panels moved based on whether unit is to the right or left. They also change colors based on the controlling unit
    public RectTransform UnitStatsPanel;
    public RectTransform UnitNamePanel;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetCombatStatDisplay(Unit ChosenUnit, Unit EnemyUnit) //Enemy needed to determine whether unit counters
    {
        DisplayedUnit = ChosenUnit;

        if (ChosenUnit.transform.position.x < EnemyUnit.transform.position.x) //if a unit's more to the left, their display goes to the left. Without a difference in X-position, AttackingUnit gets left priority
        {
            ShiftToSide(true);
        }
        else if (ChosenUnit.transform.position.x == EnemyUnit.transform.position.x) //no left/right sides determined- units are on top of one another- give attacker left-side priority
        {
            ShiftToSide(DisplayedUnit == FindObjectOfType<BattleUI>().InitiatingUnit);
        }
        else //unit is on the right. move accordingly
        {
            ShiftToSide(false);
        }

        UnitName.SetText(DisplayedUnit.Name);

        string DisplayUnitStats = "";

        if (ChosenUnit.Leader != EnemyUnit.Leader) //enemy combat
        {
            if (ChosenUnit.MightAgainst(EnemyUnit) != "--") //if the enemy can hit back (AND ISN'T A SUPPORTING ALLY), show their stats.
            {
                DisplayUnitStats += ChosenUnit.MightAgainst(EnemyUnit) + "\n" + ChosenUnit.CombinedHit(ChosenUnit == FindObjectOfType<GameController>().InitiatingUnit, EnemyUnit) + "\n" + ChosenUnit.CombinedCrit(EnemyUnit);
            }
            else //"--" indicates no ability for the enemy to counter; thus, turn hit, crit, etc. to this as well if they can't attack.
            {
                DisplayUnitStats += "--\n--\n--";
            }
        }
        else //staff/support combat
        {
            DisplayUnitStats = "--\n--\n--";
        }

        UnitStatValues.SetText(DisplayUnitStats);

        //Also set health bar and window colors.
        HealthFiller.GetComponent<Image>().color = ChosenUnit.Leader.TeamColor;
        UnitNamePanel.GetComponent<Image>().color = ChosenUnit.Leader.TeamColor;
        UnitStatsPanel.GetComponent<Image>().color = ChosenUnit.Leader.TeamColor;

        UpdateDisplayHealth();
    }

    public void UpdateDisplayHealth()
    {
        HealthValue.SetText("{0}", DisplayedUnit.CurrentHealth);

        float HealthPercentage = 0.05f + (((float)DisplayedUnit.CurrentHealth / DisplayedUnit.MaxHealth) - 0.05f); //base width is 5% so that it's seen even at low health values

        HealthFiller.localScale = new Vector3(HealthPercentage, 1.15f, 1f);

        if(DisplayedUnit.BarrierHealth > 0)
        {
            BarrierIndicator.gameObject.SetActive(true);
            BarrierValue.SetText("{0}", DisplayedUnit.BarrierHealth);
        }
        else
        {
            BarrierIndicator.gameObject.SetActive(false);
        }
    }

    public void ShiftToSide(bool LeftSide)
    {
        if (LeftSide)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-1050f, 0f);
            UnitStatsPanel.anchoredPosition = new Vector2(-60f, -280f);
            HealthValue.GetComponent<RectTransform>().anchoredPosition = new Vector2(260f, 0f);
            BarrierIndicator.anchoredPosition = new Vector2(535f, 120f);
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(1050f, 0f);
            UnitStatsPanel.anchoredPosition = new Vector2(60f, -280f);
            HealthValue.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260f, 0f);
            BarrierIndicator.anchoredPosition = new Vector2(-140f, 120f);
        }
    }
}
