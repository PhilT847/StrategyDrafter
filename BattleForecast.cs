using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleForecast : Menu
{
    public Unit ControlledUnit;
    public Unit EnemyUnit;

    public TextMeshProUGUI PlayerStats;
    public TextMeshProUGUI EnemyStats;

    public TextMeshProUGUI EnemyName;
    public TextMeshProUGUI EnemyWeaponName;
    public Image EnemyWeaponSprite;

    public Image PlayerDoubleIndicator;
    public Image EnemyDoubleIndicator;

    public Image PlayerPanel; //the allied panel that gets colored.
    public Image EnemyPanel; //the enemy panel that gets colored.

    private void OnEnable()
    {
        string DisplayPlayerStats = ControlledUnit.CurrentHealth + "\n" + ControlledUnit.MightAgainst(EnemyUnit) + "\n" + ControlledUnit.CombinedHit(true, EnemyUnit) + "\n" + ControlledUnit.CombinedCrit(EnemyUnit);

        PlayerStats.SetText(DisplayPlayerStats);

        SetForecast(EnemyUnit); //sets the enemy's forecast
    }

    public void SetForecast(Unit SelectedEnemy)
    {
        PlayerPanel.color = ControlledUnit.Leader.TeamColor;
        EnemyPanel.color = SelectedEnemy.Leader.TeamColor;

        EnemyUnit = SelectedEnemy;

        if (!ControlledUnit.HeldWeapon.TargetsAllies)
        {
            string DisplayPlayerStats = ControlledUnit.CurrentHealth + "\n" + ControlledUnit.MightAgainst(EnemyUnit) + "\n" + ControlledUnit.CombinedHit(true, EnemyUnit) + "\n" + ControlledUnit.CombinedCrit(EnemyUnit);

            PlayerStats.SetText(DisplayPlayerStats);

            if (ControlledUnit.CanDoubleAgainst(true, EnemyUnit))
            {
                PlayerDoubleIndicator.color = Color.white;
            }
            else
            {
                PlayerDoubleIndicator.color = Color.clear;
            }

            //non-frozen enemies can hit back! display their stats.
            if (!EnemyUnit.IsFrozen)
            {
                if (EnemyUnit.CanDoubleAgainst(false, ControlledUnit))
                {
                    EnemyDoubleIndicator.color = Color.white;
                }
                else
                {
                    EnemyDoubleIndicator.color = Color.clear;
                }

                string DisplayEnemyStats = "";

                if (EnemyUnit.MightAgainst(ControlledUnit) != "--") //if the enemy can hit back, show their stats.
                {
                    DisplayEnemyStats += EnemyUnit.CurrentHealth + "\n" + EnemyUnit.MightAgainst(ControlledUnit) + "\n" + EnemyUnit.CombinedHit(false, ControlledUnit) + "\n" + EnemyUnit.CombinedCrit(ControlledUnit);
                }
                else //"--" indicates no ability for the enemy to counter; thus, turn hit, crit, etc. to this as well if they can't attack.
                {
                    DisplayEnemyStats += EnemyUnit.CurrentHealth + "\n--\n--\n--";

                    EnemyDoubleIndicator.color = Color.clear; //since they can't counter, they can't double, either
                }

                EnemyStats.SetText(DisplayEnemyStats);
            }
            else
            {
                EnemyDoubleIndicator.color = Color.clear;
                EnemyStats.SetText(EnemyUnit.CurrentHealth + "\n--\n--\n--");
            }
        }
        else
        {
            PlayerDoubleIndicator.color = Color.clear;
            EnemyDoubleIndicator.color = Color.clear;

            PlayerStats.SetText(ControlledUnit.CurrentHealth + "\n--\n--\n--");
            EnemyStats.SetText(EnemyUnit.CurrentHealth + "\n--\n--\n--");
        }

        EnemyName.SetText(EnemyUnit.Name);
        EnemyWeaponName.SetText(EnemyUnit.HeldWeapon.WeaponName);
        EnemyWeaponSprite.sprite = EnemyUnit.HeldWeapon.DisplaySprite;
    }

    public override void OpenMenu() //Creates the menu if it's not already opened; otherwise, it just becomes active due to the ControlledUI opening it.
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Master_UI.PlayerCursor.ChangeCursorState(Cursor.CursorState.CURSOR_ATTACKING);
    }

    public override void CloseMenu()
    {
        Master_UI.PlayerCursor.ChangeCursorState(Cursor.CursorState.CURSOR_STANDBY);
        Master_UI.ActivateMenu(1); //closing the menu returns to the Weapon Choice Menu

        gameObject.SetActive(false);
    }
}
