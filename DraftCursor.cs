using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DraftCursor : MonoBehaviour
{
    public Player ControllingPlayer;
    public DraftRoster ControllingRoster;
    public int SelectionsBeforeNextRound;

    public Unit SelectedUnit;

    public int CurrentUnitIndex; //0-4, based on left to right

    public List<Unit> AllUnits;
    public List<Unit> DraftedUnits;

    private float MoveWaitTimer;

    public SpriteRenderer CursorSprite; //the squarish cursor that changes color based on controlling player.
    public List<SpriteRenderer> CursorCircles; //the images that order players above the cursor.
    public Sprite[] CursorCircleSprites; //the circles that denote players 1-4.

    [HideInInspector] public DraftRoomController DraftControls;

    // Update is called once per frame
    void Update()
    {
        if(MoveWaitTimer > 0f)
        {
            MoveWaitTimer -= Time.deltaTime;
        }
        else
        {
            CursorControls();
        }

        //DEBUG; REMOVE LATER
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("MOVED BACK TO SCENE 0");
            SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        }
    }

    void CursorControls()
    {
        if(Input.GetAxisRaw("Horizontal") > 0f)//right
        {
            if (CurrentUnitIndex < AllUnits.Count - 1)
            {
                MoveToPosition(CurrentUnitIndex + 1);
            }
            else
            {
                MoveToPosition(0);
            }
        }
        else if (Input.GetAxisRaw("Horizontal") < 0f)
        {
            if (CurrentUnitIndex > 0)
            {
                MoveToPosition(CurrentUnitIndex - 1);
            }
            else
            {
                MoveToPosition(AllUnits.Count - 1);
            }
        }

        /*
        if (Input.GetAxisRaw("Vertical") > 0f)//up
        {
            if (CurrentUnitIndex > 3)
            {
                MoveToPosition(CurrentUnitIndex - 4);
            }
            else
            {
                MoveToPosition(CurrentUnitIndex + 12);
            }
        }
        else if (Input.GetAxisRaw("Vertical") < 0f)
        {
            if (CurrentUnitIndex < 12)
            {
                MoveToPosition(CurrentUnitIndex + 4);
            }
            else
            {
                MoveToPosition(CurrentUnitIndex - 12);
            }
        }
        */

        if (Input.GetKeyDown(KeyCode.Z))
        {
            CursorSelect();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            //CompareStats(true);
            //AllUnits = ChangeSort(false);
        }
    }

    void CursorSelect() //if it's an empty slot, add the selected unit to the controlling player's team and mark them as selected.
    {
        if (!DraftedUnits.Contains(SelectedUnit))
        {
            //StatComparers[AllUnits.IndexOf(SelectedUnit)].gameObject.SetActive(false); //removes their stat comparer until units are reset

            SelectedUnit.tag = "SelectedUnit"; //Ensures that the DraftRoomController doesn't delete this unit when it resets

            SelectedUnit.JoinTeam(ControllingPlayer); //sets the unit's leader and colors to match their new team

            DraftedUnits.Add(SelectedUnit);

            ControllingRoster.AddNewUnit(SelectedUnit); //adds the new unit to a roster and moves them into position

            AllUnits.Remove(SelectedUnit);

            SelectionsBeforeNextRound--;

            if(SelectionsBeforeNextRound != 0) //Round isn't over yet; allow another player to select a unit
            {
                //NOTE: The next player isn't changed when a round starts, as the player who picked last now gets to pick first.

                //allows the next player in the list to take control once selections are complete
                ControllingPlayer = FindObjectOfType<DraftRoomController>().SelectNextPlayer();
                //the roster is in player order, so just find the index of the player from its order
                ControllingRoster = FindObjectsOfType<DraftRoster>()[ControllingPlayer.PlayerOrder - 1];

                MoveToPosition(0);
            }
            else //This round is complete
            {
                FindObjectOfType<DraftRounds>().SelectNextCircle(); //changes the DraftRounds to show which round it is

                DraftControls.StartNextDraftingRound();
            }

            //Set circles AFTER finding a new ControllingUnit so that the circles use a new order
            SetCursorCircles();
        }
    }

    public void InitializeCursorCircles() //Removes all extra player circles should there be less than 4 players.. length 1-4... i < 4
    {
        for (int i = 4 - DraftControls.AllPlayers.Length; i < 4; i++)
        {
            CursorCircles[i].transform.localScale = Vector2.zero;
        }
    }

    public void SetCursorCircles()
    {
        CursorSprite.color = ControllingPlayer.TeamColor;

        int OrderedPlayerIndex = ControllingPlayer.PlayerOrder - 1; //recoloring the circle sprites starts at the ControllingPlayer's index, then moves in player order.

        for(int i = 0; i < DraftControls.AllPlayers.Length; i++)
        {
            CursorCircles[i].color = DraftControls.AllPlayers[OrderedPlayerIndex].TeamColor;
            CursorCircles[i].sprite = CursorCircleSprites[DraftControls.AllPlayers[OrderedPlayerIndex].PlayerOrder - 1];

            OrderedPlayerIndex += 1;

            if (OrderedPlayerIndex == DraftControls.AllPlayers.Length) //return to index 0 for AllPlayers
            {
                OrderedPlayerIndex = 0;
            }
        }

        /*
        for (int i = 0; i < CursorCircles.Count; i++)
        {
            if (i < DraftControls.AllPlayers.Length) //ensures that the "zeroed out" removed circles are not counted, as they'd go beyond AllPlayers.Length
            {
                CursorCircles[i].color = DraftControls.AllPlayers[i].TeamColor;
                CursorCircles[i].sprite = CursorCircleSprites[DraftControls.AllPlayers[i].PlayerOrder - 1];
            }
        }
        */
    }

    void CursorCancel()
    {
        MoveToPosition(0);
    }

    public void MoveToPosition(int index)
    {
        MoveWaitTimer = 0.12f;

        CurrentUnitIndex = index;
        SelectedUnit = AllUnits[CurrentUnitIndex];
        transform.position = SelectedUnit.transform.position;

        FindObjectOfType<DraftUnitInfo>().SetUnitInfo(SelectedUnit);
    }
}
