using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitActionsMenu : Menu
{
    public Button[] Buttons;

    public RectTransform Cursor;

    public int SelectedButton;

    public Unit ControlledUnit; //the unit that this menu is building for. Used for attack range, attack type, etc.
    public Tile ReturnIfCanceled; //the tile to return to upon hitting "Cancel"

    public WeaponDesc WeaponDescriptionPanel;

    private void OnEnable() //when UI pops up, go to first position.
    {
        SelectedButton = 0;
        MoveCursorTo(0);

        //also change "Attack" button based on unit's abilities
        Buttons[0].GetComponentInChildren<TextMeshProUGUI>().SetText(ControlledUnit.HeldWeapon.ActionText);
        Buttons[0].GetComponentInChildren<Image>().sprite = ControlledUnit.HeldWeapon.DisplaySprite;
    }

    void Update()
    {
        if (MenuActive)
        {
            if (Input.GetKeyDown(KeyCode.Z)) //Select the current button. Takes priority over moving the cursor.
            {
                Buttons[SelectedButton].onClick.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (SelectedButton < 2)
                {
                    SelectedButton += 1;
                }

                MoveCursorTo(SelectedButton);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (SelectedButton > 0)
                {
                    SelectedButton -= 1;
                }

                MoveCursorTo(SelectedButton);
            }

            if (Input.GetKeyDown(KeyCode.X)) //move to cancel button; if on cancel, remove the UI and move the player back to their initial space.
            {
                CancelChoice();
            }
        }
    }

    void MoveCursorTo(int index) //cursor moves to y = 80 -> 0 -> -80 based on selection.
    {
        Cursor.anchoredPosition = new Vector3(-25, 80 - (SelectedButton * 80), 0);

        if (index != 1)
        {
            WeaponDescriptionPanel.gameObject.SetActive(false);
        }
        else
        {
            WeaponDescriptionPanel.gameObject.SetActive(true);

            WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.HeldWeapon);
        }
    }

    public void ChooseAttack() //open up the weapon menu, highlighting potential ranges. Start with the Held Weapon as that's where the cursor starts on the Weapon Choice menu.
    {
        ControlledUnit.DetermineTargetableUnits(ControlledUnit.HeldWeapon);

        Master_UI.PlayerMenus[1].GetComponent<WeaponChoiceMenu>().ControlledUnit = ControlledUnit; //prepare the Weapon Choice menu
        Master_UI.ActivateMenu(1); //open the weapon menu
    }

    public void ChooseEquip() //"Equip" swaps the units weapons.
    {
        ControlledUnit.SwitchWeapon();

        //Refresh the text/icon of the attack button to match the new weapon
        Buttons[0].GetComponentInChildren<TextMeshProUGUI>().SetText(ControlledUnit.HeldWeapon.ActionText);
        Buttons[0].GetComponentInChildren<Image>().sprite = ControlledUnit.HeldWeapon.DisplaySprite;

        WeaponDescriptionPanel.SetWeaponDescription(ControlledUnit.HeldWeapon);
    }

    public void ChooseWait() //End this unit's turn.
    {
        ControlledUnit.RemoveFromAction();

        CloseMenu();
    }

    public void CancelChoice() //move back and allow this unit to act again.
    {
        ControlledUnit.StartCoroutine(ControlledUnit.MoveUnitTo(ReturnIfCanceled, false));

        CloseMenu();
    }

    public override void OpenMenu()
    {
        //Since the unit may have switched weapons, refresh the text/icon of the attack button
        Buttons[0].GetComponentInChildren<TextMeshProUGUI>().SetText(ControlledUnit.HeldWeapon.ActionText);
        Buttons[0].GetComponentInChildren<Image>().sprite = ControlledUnit.HeldWeapon.DisplaySprite;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public override void CloseMenu()
    {
        MenuActive = false;

        GetComponentInParent<ControlledUI>().PlayerCursor.ChangeCursorState(global::Cursor.CursorState.CURSOR_VIEWING);

        WeaponDescriptionPanel.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
