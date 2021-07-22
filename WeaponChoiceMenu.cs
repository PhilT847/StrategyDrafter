using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponChoiceMenu : Menu
{
    public GameObject[] PotentialWeaponButtons;

    public RectTransform Cursor;

    public int SelectedButton;

    public Unit ControlledUnit; //the unit that this menu is building for. Used for attack range, attack type, etc.

    public WeaponDesc WeaponDescriptionPanel;

    private void OnEnable()
    {
        ControlledUnit.DetermineTargetableUnits(ControlledUnit.HeldWeapon); //create a list to determine whether a weapon can be selected

        SelectedButton = 0;
        MoveCursorTo(0, true);

        PotentialWeaponButtons[0].GetComponentInChildren<TextMeshProUGUI>().SetText(ControlledUnit.HeldWeapon.WeaponName);
        PotentialWeaponButtons[0].GetComponentInChildren<Image>().sprite = ControlledUnit.HeldWeapon.DisplaySprite;

        PotentialWeaponButtons[1].GetComponentInChildren<TextMeshProUGUI>().SetText(ControlledUnit.SecondaryWeapon.WeaponName);
        PotentialWeaponButtons[1].GetComponentInChildren<Image>().sprite = ControlledUnit.SecondaryWeapon.DisplaySprite;
    }

    void Update()
    {
        if (MenuActive)
        {
            if (Input.GetKeyDown(KeyCode.Z) && ControlledUnit.PotentialTargets.Count > 0) //Select the current button and choose an enemy.
            {
                SelectCombatWeapon();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                FindObjectOfType<TileController>().RemoveHighlightedTiles();

                if (SelectedButton == 0)
                {
                    SelectedButton = 1;
                }
                else
                {
                    SelectedButton = 0;
                }

                MoveCursorTo(SelectedButton, false);

                ControlledUnit.DetermineTargetableUnits(ControlledUnit.HeldWeapon);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) //when selecting a spell, the spell's power can be changed by the right/left arrows.
            {
                FindObjectOfType<TileController>().RemoveHighlightedTiles();

                if (Input.GetKeyDown(KeyCode.RightArrow)) //right arrow has dominance
                {
                    ChangeSpellLevel(true);
                }
                else
                {
                    ChangeSpellLevel(false);
                }

                WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.HeldWeapon); //refresh the weapon description panel

                ControlledUnit.DetermineTargetableUnits(ControlledUnit.HeldWeapon);

                //reset the weapon's name in the choice menu
                PotentialWeaponButtons[SelectedButton].GetComponentInChildren<TextMeshProUGUI>().SetText(ControlledUnit.HeldWeapon.WeaponName);

                /*
                //Usually, MoveCursorTo() handles refreshing the description. Since left->right doesn't switch weapons though, just refresh without moving.
                if (SelectedButton == 0)
                {
                    WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.HeldWeapon); //refresh the weapon description panel

                    ControlledUnit.DetermineTargetableUnits(ControlledUnit.HeldWeapon);
                }
                else
                {
                    WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.SecondaryWeapon); //refresh the weapon description panel

                    ControlledUnit.DetermineTargetableUnits(ControlledUnit.SecondaryWeapon);
                }
                */

            }

            if (Input.GetKeyDown(KeyCode.X)) //cancel; do not switch weapon
            {
                ControlledUnit.ResetSpellLevels(); //reset spell levels so the descriptions return to the "1" level; note that it doesn't reset in CloseMenu() as then it'd be resetted before combat
                CloseMenu();
            }
        }
    }

    void ChangeSpellLevel(bool increase)
    {
        if (ControlledUnit.HeldWeapon.GetComponent<Magic>())
        {
            if (increase)
            {
                ControlledUnit.HeldWeapon.GetComponent<Magic>().ChangeSpellLevel(ControlledUnit.HeldWeapon.GetComponent<Magic>().SpellLevel + 1);
            }
            else
            {
                ControlledUnit.HeldWeapon.GetComponent<Magic>().ChangeSpellLevel(ControlledUnit.HeldWeapon.GetComponent<Magic>().SpellLevel - 1);
            }
        }
        else if (ControlledUnit.HeldWeapon.GetComponent<Staff>())
        {
            if (increase)
            {
                ControlledUnit.HeldWeapon.GetComponent<Staff>().ChangeSpellLevel(ControlledUnit.HeldWeapon.GetComponent<Staff>().SpellLevel + 1);
            }
            else
            {
                ControlledUnit.HeldWeapon.GetComponent<Staff>().ChangeSpellLevel(ControlledUnit.HeldWeapon.GetComponent<Staff>().SpellLevel - 1);
            }
        }
    }

    void MoveCursorTo(int index, bool FirstOpen) //move to the other weapon, equipping it in the process. On the first opening (when the menu opens), do NOT switch.
    {
        //ensure spells return to Level 1 when equipping
        ControlledUnit.ResetSpellLevels();

        if (!FirstOpen)
        {
            ControlledUnit.SwitchWeapon();
        }

        Cursor.anchoredPosition = new Vector3(-25, 30 - (SelectedButton * 60), 0);

        WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.HeldWeapon);
    }

    public void SelectCombatWeapon() //Select a weapon and choose an enemy.
    {
        Master_UI.PlayerMenus[2].GetComponent<BattleForecast>().ControlledUnit = ControlledUnit;
        Master_UI.PlayerMenus[2].GetComponent<BattleForecast>().SetForecast(ControlledUnit.PotentialTargets[0]);

        WeaponDescriptionPanel.gameObject.SetActive(false);

        Master_UI.ActivateMenu(2); //display forecast and choose an enemy.

        //CloseMenu();
    }

    public override void OpenMenu() //Creates the menu if it's not already opened; otherwise, it just becomes active due to the ControlledUI opening it.
    {
        ControlledUnit.ResetSpellLevels(); //reset spell levels so the descriptions return to the "1" level

        gameObject.SetActive(true);

        WeaponDescriptionPanel.gameObject.SetActive(true);
        WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.HeldWeapon);
    }

    public override void CloseMenu()
    {
        Master_UI.ActivateMenu(0); //closing the menu always returns to the Choice Menu

        FindObjectOfType<TileController>().RemoveHighlightedTiles(); //highlighted tiles go away once there are no weapons displayed

        WeaponDescriptionPanel.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
