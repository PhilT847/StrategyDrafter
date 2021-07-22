using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    //Handles player controls. Determines how units can move and act on the field.

    public Player Owner;

    public Unit SelectedUnit;
    public Unit SelectedEnemy;

    public Tile CurrentTile;

    public Tile[,] TileMap;

    private float MoveWaitTime;

    private Camera PlayerCam;
    [HideInInspector] public Vector3 PreferredCameraPosition;
    [HideInInspector] public float PreferredCameraScale;

    public Animator CursorAnim;

    public enum CursorState { CURSOR_VIEWING, CURSOR_MOVEMENT, CURSOR_ATTACKING, CURSOR_STANDBY, CURSOR_SPECTATING }; //different states for viewing the map, moving a unit, and attacking... also standby while menus are open.

    private CursorState PlayerCursorState;

    private bool ShowingUnitInfo;
    private UnitInfoPanel UnitInfoPanel;

    //All the parts that can be grabbed before entering the map
    private void Awake()
    {
        CursorAnim = GetComponentInChildren<Animator>();

        UnitInfoPanel = GetComponentInChildren<UnitInfoPanel>(true);

        PlayerCam = transform.parent.GetComponentInChildren<Camera>(true);
        PreferredCameraScale = PlayerCam.orthographicSize;

        Owner = transform.parent.GetComponentInParent<Player>();
    }

    /*
    void OnEnabled()
    {
        TileMap = FindObjectOfType<TileController>().TileMap;

        CurrentTile = TileMap[0,0];
        transform.position = CurrentTile.transform.position;

        //move the camera to the cursor if required
        CheckCursorPosition(CurrentTile);

        ChangeCursorState(CursorState.CURSOR_VIEWING);
    }
    */

    public void PrepareCursorForMap()
    {
        TileMap = FindObjectOfType<TileController>().TileMap;

        CurrentTile = TileMap[0, 0];
        transform.position = CurrentTile.transform.position;

        //move the camera to the cursor if required
        CheckCursorPosition(CurrentTile);

        ChangeCursorState(CursorState.CURSOR_VIEWING);
    }

    // Update is called once per frame
    void Update()
    {
        if (FindObjectOfType<GameController>().ActivePlayer == Owner)
        {
            if (PlayerCursorState != CursorState.CURSOR_STANDBY && PlayerCursorState != CursorState.CURSOR_SPECTATING) //if the cursor's active, allow it to move and control based on its current state (view/move/attack)
            {
                if (MoveWaitTime > 0f)
                {
                    MoveWaitTime -= Time.deltaTime;
                }
                else
                {
                    MoveWaitTime = 0f;

                    CameraControls(); //allow player to control their camera's size/position while not on standby and MoveWait is zero
                }

                switch (PlayerCursorState)
                {
                    case CursorState.CURSOR_VIEWING:
                        {
                            ViewingCursor();
                            break;
                        }
                    case CursorState.CURSOR_MOVEMENT:
                        {
                            MoveOptionsCursor();
                            break;
                        }
                    case CursorState.CURSOR_ATTACKING:
                        {
                            AttackOptionsCursor();
                            break;
                        }
                }

                //move the cursor object as well as the camera if needed
                transform.position = Vector3.MoveTowards(transform.position, CurrentTile.transform.position, 20f * Time.deltaTime); //0.25f
                PlayerCam.transform.position = Vector3.MoveTowards(PlayerCam.transform.position, PreferredCameraPosition, 20f * Time.deltaTime);
                PlayerCam.orthographicSize = Mathf.MoveTowards(PlayerCam.orthographicSize, PreferredCameraScale, 20f * Time.deltaTime);
                CheckCursorPosition(); //move the camera as needed
                ScaleUnitInfoPanel();

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    CursorSelect();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    CursorCancel();
                }
            }
        }
        else //Set the cursor on standby when it's not that player's turn.
        {
            ChangeCursorState(CursorState.CURSOR_STANDBY);
        }
    }

    void ViewingCursor() //The cursor when moving around the map and selecting units.
    {
        if (MoveWaitTime == 0f)
        {
            if (Input.GetKey(KeyCode.RightArrow) && CurrentTile.X_Position < 11)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position + 1, CurrentTile.Y_Position]);

                if (CurrentTile.OccupyingUnit == null)
                {
                    RemoveUnitStatPanel();
                }
                else if (ShowingUnitInfo)
                {
                    ShowUnitStatPanel();
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && CurrentTile.X_Position > 0)
            {
                CheckCursorPosition(CurrentTile = TileMap[CurrentTile.X_Position - 1, CurrentTile.Y_Position]);

                if (CurrentTile.OccupyingUnit == null)
                {
                    RemoveUnitStatPanel();
                }
                else if (ShowingUnitInfo)
                {
                    ShowUnitStatPanel();
                }
            }

            if (Input.GetKey(KeyCode.UpArrow) && CurrentTile.Y_Position < 11)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position, CurrentTile.Y_Position + 1]);

                if (CurrentTile.OccupyingUnit == null)
                {
                    RemoveUnitStatPanel();
                }
                else if (ShowingUnitInfo)
                {
                    ShowUnitStatPanel();
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow) && CurrentTile.Y_Position > 0)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position, CurrentTile.Y_Position - 1]);

                if (CurrentTile.OccupyingUnit == null)
                {
                    RemoveUnitStatPanel();
                }
                else if (ShowingUnitInfo)
                {
                    ShowUnitStatPanel();
                }
            }

            if (Input.GetKeyDown(KeyCode.C)) //pressing C opens the unit menu
            {
                CursorOtherCommand();
            }

            //DEBUG
            if (Input.GetKeyDown(KeyCode.V)) //pressing V ends your turn.
            {
                //EndTurn();
            }

            CursorAnim.SetBool("Hovering", CurrentTile.OccupyingUnit != null && CurrentTile.OccupyingUnit.Leader == Owner && CurrentTile.OccupyingUnit.CanAct);
        }
    }

    void MoveOptionsCursor()
    {
        if (MoveWaitTime == 0f)
        {
            if (Input.GetKey(KeyCode.RightArrow) && CurrentTile.X_Position < 11
                && TileMap[CurrentTile.X_Position + 1, CurrentTile.Y_Position].Highlighted)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position + 1, CurrentTile.Y_Position]);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && CurrentTile.X_Position > 0
                && TileMap[CurrentTile.X_Position - 1, CurrentTile.Y_Position].Highlighted)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position - 1, CurrentTile.Y_Position]);
            }

            if (Input.GetKey(KeyCode.UpArrow) && CurrentTile.Y_Position < 11
                && TileMap[CurrentTile.X_Position, CurrentTile.Y_Position + 1].Highlighted)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position, CurrentTile.Y_Position + 1]);
            }
            else if (Input.GetKey(KeyCode.DownArrow) && CurrentTile.Y_Position > 0
                && TileMap[CurrentTile.X_Position, CurrentTile.Y_Position - 1].Highlighted)
            {
                CheckCursorPosition(TileMap[CurrentTile.X_Position, CurrentTile.Y_Position - 1]);
            }
        }
    }

    void AttackOptionsCursor() //move between the tiles of TargetableUnits added to the Unit during HighlightUnitsWithinRange() in the TileController
    {
        transform.position = SelectedEnemy.transform.position;

        if (MoveWaitTime == 0f)
        {
            if (SelectedUnit.PotentialTargets.Count > 1)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    MoveAttackCursorInDirection(1);
                }

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    MoveAttackCursorInDirection(2);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    MoveAttackCursorInDirection(3);
                }

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    MoveAttackCursorInDirection(4);
                }

                SelectedEnemy = SelectedUnit.PotentialTargets[SelectedUnit.SelectedEnemyIndex]; //track the current enemy
                Owner.PlayerUI.PlayerMenus[2].GetComponent<BattleForecast>().SetForecast(SelectedEnemy);
            }
            else
            {
                SelectedEnemy = SelectedUnit.PotentialTargets[0]; //no other enemy targets
            }
        }

        CurrentTile = TileMap[SelectedEnemy.CurrentTile.X_Position, SelectedEnemy.CurrentTile.Y_Position];
        CheckCursorPosition();
    }

    void MoveAttackCursorInDirection(int direction) //Move the attack cursor. Looks for a unit in the selected direction
    {
        if (FindEnemyInDirection(direction) != null) //Found a unit in the direction
        {
            SelectedUnit.SelectedEnemyIndex = SelectedUnit.PotentialTargets.IndexOf(FindEnemyInDirection(direction));
        }
        else if (FindEnemiesInGeneralDirection(direction).Count > 0) //No direct path; look for a unit that's in the *general* direction
        {
            SelectedUnit.SelectedEnemyIndex = SelectedUnit.PotentialTargets.IndexOf(FindEnemiesInGeneralDirection(direction)[0]);
        }
        else //just move farther down the PotentialTargets list
        {
            switch (direction)
            {
                case 1:
                    {
                        if (SelectedUnit.SelectedEnemyIndex < SelectedUnit.PotentialTargets.Count - 1)
                        {
                            SelectedUnit.SelectedEnemyIndex++;
                        }
                        else
                        {
                            SelectedUnit.SelectedEnemyIndex = 0;
                        }

                        break;
                    }
                case 2:
                    {
                        if (SelectedUnit.SelectedEnemyIndex < SelectedUnit.PotentialTargets.Count - 1)
                        {
                            SelectedUnit.SelectedEnemyIndex++;
                        }
                        else
                        {
                            SelectedUnit.SelectedEnemyIndex = 0;
                        }

                        break;
                    }
                case 3:
                    {
                        if (SelectedUnit.SelectedEnemyIndex > 0)
                        {
                            SelectedUnit.SelectedEnemyIndex--;
                        }
                        else
                        {
                            SelectedUnit.SelectedEnemyIndex = SelectedUnit.PotentialTargets.Count - 1;
                        }

                        break;
                    }
                case 4:
                    {
                        if (SelectedUnit.SelectedEnemyIndex > 0)
                        {
                            SelectedUnit.SelectedEnemyIndex--;
                        }
                        else
                        {
                            SelectedUnit.SelectedEnemyIndex = SelectedUnit.PotentialTargets.Count - 1;
                        }

                        break;
                    }
            }
        }

        CheckCursorPosition(SelectedUnit.PotentialTargets[SelectedUnit.SelectedEnemyIndex].CurrentTile);
    }

    Unit FindEnemyInDirection(int direction) // Finds an enemy in a specified direction. Used when picking a target to attack.
    {
        switch (direction)
        {
            case 1: //up
                {
                    for (int i = CurrentTile.Y_Position; i < 12; i++)
                    {
                        if (SelectedUnit.PotentialTargets.Contains(TileMap[CurrentTile.X_Position, i].OccupyingUnit)
                            && TileMap[CurrentTile.X_Position, i].OccupyingUnit != CurrentTile.OccupyingUnit)
                        {
                            return TileMap[CurrentTile.X_Position, i].OccupyingUnit;
                        }
                    }

                    break;
                }
            case 2: //right
                {
                    for (int i = CurrentTile.X_Position; i < 12; i++)
                    {
                        if (SelectedUnit.PotentialTargets.Contains(TileMap[i, CurrentTile.Y_Position].OccupyingUnit)
                            && TileMap[i, CurrentTile.Y_Position].OccupyingUnit != CurrentTile.OccupyingUnit)
                        {
                            return TileMap[i, CurrentTile.Y_Position].OccupyingUnit;
                        }
                    }

                    break;
                }
            case 3: //down
                {
                    for (int i = CurrentTile.Y_Position; i > -1; i--)
                    {
                        if (SelectedUnit.PotentialTargets.Contains(TileMap[CurrentTile.X_Position, i].OccupyingUnit)
                            && TileMap[CurrentTile.X_Position, i].OccupyingUnit != CurrentTile.OccupyingUnit)
                        {
                            return TileMap[CurrentTile.X_Position, i].OccupyingUnit;
                        }
                    }

                    break;
                }
            case 4: //left
                {
                    for (int i = CurrentTile.X_Position; i > -1; i--)
                    {
                        if (SelectedUnit.PotentialTargets.Contains(TileMap[i, CurrentTile.Y_Position].OccupyingUnit)
                            && TileMap[i, CurrentTile.Y_Position].OccupyingUnit != CurrentTile.OccupyingUnit)
                        {
                            return TileMap[i, CurrentTile.Y_Position].OccupyingUnit;
                        }
                    }

                    break;
                }
        }

        return null;
    }

    List<Unit> FindEnemiesInGeneralDirection(int direction) //finds an enemy in a general direction that isn't DIRECTLY that direction; used when units are disconnected from one another
    {
        List<Unit> FoundEnemies = new List<Unit>();

        switch (direction)
        {
            case 1: //up
                {
                    foreach (Unit PotentialUnit in SelectedUnit.PotentialTargets)
                    {
                        if (PotentialUnit.CurrentTile.Y_Position > CurrentTile.Y_Position && FindEnemyInDirection(1) != PotentialUnit)
                        {
                            FoundEnemies.Add(PotentialUnit);
                        }
                    }

                    break;
                }
            case 2: //right
                {
                    foreach (Unit PotentialUnit in SelectedUnit.PotentialTargets)
                    {
                        if (PotentialUnit.CurrentTile.X_Position > CurrentTile.X_Position && FindEnemyInDirection(2) != PotentialUnit)
                        {
                            FoundEnemies.Add(PotentialUnit);
                        }
                    }

                    break;
                }
            case 3: //down
                {
                    foreach (Unit PotentialUnit in SelectedUnit.PotentialTargets)
                    {
                        if (PotentialUnit.CurrentTile.Y_Position < CurrentTile.Y_Position && FindEnemyInDirection(3) != PotentialUnit)
                        {
                            FoundEnemies.Add(PotentialUnit);
                        }
                    }

                    break;
                }
            case 4: //left
                {
                    foreach (Unit PotentialUnit in SelectedUnit.PotentialTargets)
                    {
                        if (PotentialUnit.CurrentTile.X_Position < CurrentTile.X_Position && FindEnemyInDirection(4) != PotentialUnit)
                        {
                            FoundEnemies.Add(PotentialUnit);
                        }
                    }

                    break;
                }
        }

        return FoundEnemies;
    }

    void CheckCursorPosition(Tile ChosenTile) //Changes the cursor's target, tile, and moves the camera if necessary.
    {
        CurrentTile = ChosenTile;

        MoveWaitTime = 0.12f;

        Vector3 TilePos = new Vector3(ChosenTile.X_Position * 2f, ChosenTile.Y_Position * 2f, 0f);

        Vector3 CursorPosition = PlayerCam.WorldToViewportPoint(TilePos);

        float ScreenLimit = 0.2f * (5f / PlayerCam.orthographicSize); //allows the camera to move variably based on the camera's size

        float MoveX = 0f;
        float MoveY = 0f;

        //if the cursor is wayyy far out, just place the camera right on the cursor.
        if (CursorPosition.x > 1f || CursorPosition.y > 1 || CursorPosition.y < 0f || CursorPosition.y < 0f)
        {
            PreferredCameraPosition = new Vector3(CurrentTile.transform.position.x, CurrentTile.transform.position.y, -10f);
        }
        else
        {
            if (CursorPosition.x < ScreenLimit) // near left edge
            {
                MoveX = -2f;
            }
            else if (CursorPosition.x > 1 - ScreenLimit) // near right edge
            {
                MoveX = 2f;
            }

            if (CursorPosition.y < ScreenLimit) // near bottom
            {
                MoveY = -2f;
            }
            else if (CursorPosition.y > 1 - ScreenLimit) // near top
            {
                MoveY = 2f;
            }

            PreferredCameraPosition = new Vector3(PlayerCam.transform.position.x + MoveX, PlayerCam.transform.position.y + MoveY, -10f);
        }
    }

    void CheckCursorPosition() //Checks where the cursor is in relation to the camera, moving the camera if the cursor reaches the edge.
    {
        Vector3 TilePos = new Vector3(CurrentTile.X_Position * 2f, CurrentTile.Y_Position * 2f, 0f);

        Vector3 CursorPosition = PlayerCam.WorldToViewportPoint(TilePos);

        float ScreenLimit = 0.2f * (5f / PlayerCam.orthographicSize); //allows the camera to move variably based on the camera's size

        float MoveX = 0f;
        float MoveY = 0f;

        //if the cursor is wayyy far out, just place the camera right on the cursor.
        if (CursorPosition.x > 1f || CursorPosition.y > 1 || CursorPosition.y < 0f || CursorPosition.y < 0f)
        {
            PreferredCameraPosition = new Vector3(transform.position.x, transform.position.y, -10f);
        }
        else
        {
            if (CursorPosition.x < ScreenLimit) // near left edge
            {
                MoveX = -2f;
            }
            else if (CursorPosition.x > 1 - ScreenLimit) // near right edge
            {
                MoveX = 2f;
            }

            if (CursorPosition.y < ScreenLimit) // near bottom
            {
                MoveY = -2f;
            }
            else if (CursorPosition.y > 1 - ScreenLimit) // near top
            {
                MoveY = 2f;
            }

            PreferredCameraPosition = new Vector3(PlayerCam.transform.position.x + MoveX, PlayerCam.transform.position.y + MoveY, -10f);
        }
    }

    void CursorSelect() //various commands the cursor can do in its various states.
    {
        switch (PlayerCursorState)
        {
            case CursorState.CURSOR_VIEWING: //If there's no current unit selected, select a unit if it exists on the tile and can still act.
                {
                    if (SelectedUnit == null && CurrentTile.OccupyingUnit != null && CurrentTile.OccupyingUnit.CanAct && CurrentTile.OccupyingUnit.Leader == Owner) 
                    {
                        SelectedUnit = CurrentTile.OccupyingUnit;

                        FindObjectOfType<TileController>().VisualizeMovementFrom(CurrentTile);

                        ChangeCursorState(CursorState.CURSOR_MOVEMENT);
                    }

                    break;
                }
            case CursorState.CURSOR_MOVEMENT: //If you have a unit and select an open, highlighted tile, the unit can move to that position.
                {
                    if (CurrentTile.Highlighted && (CurrentTile.OccupyingUnit == null || CurrentTile.OccupyingUnit == SelectedUnit)) 
                    {
                        FindObjectOfType<TileController>().RemoveHighlightedTiles();

                        SelectedUnit.StartCoroutine(SelectedUnit.MoveUnitTo(CurrentTile, true));
                    }

                    break;
                }
            case CursorState.CURSOR_ATTACKING:
                {
                    Owner.PlayerUI.CloseMenus();

                    FindObjectOfType<TileController>().RemoveHighlightedTiles();

                    //Spend orbs if casting a spell.
                    if (SelectedUnit.HeldWeapon.GetComponent<Magic>())
                    {
                        SelectedUnit.SpendOrbs(SelectedUnit.HeldWeapon.GetComponent<Magic>().SpellLevel);
                    }
                    else if (SelectedUnit.HeldWeapon.GetComponent<Staff>())
                    {
                        SelectedUnit.SpendOrbs(SelectedUnit.HeldWeapon.GetComponent<Staff>().SpellLevel);
                    }

                    FindObjectOfType<GameController>().StartCoroutine(FindObjectOfType<GameController>().CombatCutscene(SelectedUnit, SelectedEnemy));

                    break;
                }
        }
    }

    void CursorCancel()
    {
        switch (PlayerCursorState)
        {
            case CursorState.CURSOR_MOVEMENT: //If you have a unit and select an open, highlighted tile, the unit can move to that position.
                {
                    SelectedUnit.StartCoroutine(SelectedUnit.MoveUnitTo(SelectedUnit.CurrentTile, false));
                    SelectedUnit = null;

                    FindObjectOfType<TileController>().RemoveHighlightedTiles();
                    ChangeCursorState(CursorState.CURSOR_VIEWING);

                    break;
                }
            case CursorState.CURSOR_ATTACKING:
                {
                    Owner.PlayerUI.PlayerMenus[2].CloseMenu();

                    SelectedEnemy = null;

                    //Owner.PlayerUI.ActivateMenu(1); //allow unit to choose a different weapon

                    break;
                }
        }
    }

    public void CursorOtherCommand()
    {
        switch (PlayerCursorState)
        {
            case CursorState.CURSOR_VIEWING: //If you move over a unit and press C, show their stats.
                {
                    ShowingUnitInfo = !ShowingUnitInfo;

                    //if it's currently active and has a unit to show, display the unit info panel. Otherwise, destroy it.
                    if (CurrentTile.OccupyingUnit != null && ShowingUnitInfo)
                    {
                        ShowUnitStatPanel();
                    }
                    else
                    {
                        RemoveUnitStatPanel();
                    }

                    break;
                }
        }
    }

    public void ChangeCursorState(CursorState NewState)
    {
        switch (NewState)
        {
            case CursorState.CURSOR_VIEWING:
                {
                    PlayerCursorState = CursorState.CURSOR_VIEWING;

                    SelectedUnit = null; //ensures that selected unit is removed when selecting "Wait" in UnitActionsMenu
                    CursorAnim.SetTrigger("Viewing");

                    if (CurrentTile.OccupyingUnit != null && ShowingUnitInfo) //start showing the stat panel again if selected
                    {
                        ShowUnitStatPanel();
                    }

                    break;
                }
            case CursorState.CURSOR_MOVEMENT:
                {
                    RemoveUnitStatPanel(); //selecting a unit removes the unit info panel

                    PlayerCursorState = CursorState.CURSOR_MOVEMENT;

                    CursorAnim.SetTrigger("Viewing"); //uses the same viewing cursor, but does not hover over other units
                    CursorAnim.SetBool("Hovering", false);

                    break;
                }
            case CursorState.CURSOR_ATTACKING:
                {
                    SelectedEnemy = SelectedUnit.PotentialTargets[0]; //start by targeting the first potential target.

                    PlayerCursorState = CursorState.CURSOR_ATTACKING;

                    CursorAnim.SetTrigger("Attacking");

                    break;
                }
            case CursorState.CURSOR_STANDBY:
                {
                    PlayerCursorState = CursorState.CURSOR_STANDBY;

                    RemoveUnitStatPanel(); //removes the unit info panel

                    CursorAnim.SetTrigger("Standby");

                    break;
                }
        }
    }

    void CameraControls()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            MoveWaitTime = 0.5f;

            if (PlayerCam.orthographicSize == 15f)
            {
                PreferredCameraScale = 10f;
            }
            else if (PlayerCam.orthographicSize == 10f)
            {
                PreferredCameraScale = 5f;
            }
            else
            {
                PreferredCameraScale = 15f;
            }

            CheckCursorPosition(CurrentTile); //move the camera as needed
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            MoveWaitTime = 0.2f;

            PlayerCam.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
    }

    void ShowUnitStatPanel()
    {
        UnitInfoPanel.gameObject.SetActive(true);
        UnitInfoPanel.SetInfoPanel(CurrentTile.OccupyingUnit);

        //ScaleUnitInfoPanel();
    }

    void ScaleUnitInfoPanel() //scale and move info panel based on camera size/position
    {
        float PercentageOfMaxSize = PlayerCam.orthographicSize / 15f;

        //scale size
        UnitInfoPanel.GetComponent<RectTransform>().localScale = new Vector2(PercentageOfMaxSize, PercentageOfMaxSize);

        Vector3 ViewedTilePosition = PlayerCam.WorldToViewportPoint(CurrentTile.transform.position);

        if (ViewedTilePosition.y < 0.6f) //if the tile is on the top half of the camera, place it under the cursor. otherwise, put it above the cursor.
        {
            UnitInfoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 1.5f + (4.5f * PercentageOfMaxSize));
        }
        else
        {
            UnitInfoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -1.5f + (-4.5f * PercentageOfMaxSize));
        }
    }

    void RemoveUnitStatPanel()
    {
        UnitInfoPanel.gameObject.SetActive(false);
    }

    public void MoveCursorTo(int xPos, int yPos) //Instantly move the cursor to a specific position. Used at the beginning of turns or when an action is canceled.
    {
        CurrentTile = TileMap[xPos, yPos];

        transform.position = CurrentTile.transform.position;

        CheckCursorPosition();
    }

    //automatically begin the next turn.
    void EndTurn()
    {
        FindObjectOfType<GameController>().StartNextTurn();
        /*
        List<Unit> UnitsRemaining = new List<Unit>();

        //Create a list of units to remove. You can't just call RemoveFromAction() as they may become reactivated when the turn ends through that function. Can lead to multiple turn skips.
        foreach (Unit PlayerUnit in Owner.UnitsOnTeam)
        {
            if (PlayerUnit.CanAct)
            {
                UnitsRemaining.Add(PlayerUnit);
            }
        }

        foreach(Unit RemovalUnit in UnitsRemaining)
        {
            RemovalUnit.RemoveFromAction();
        }
        //NOTE: Since RemoveFromAction checks if the turn is over, no need to call PlayerTurnIsOver() here.
        */
    }
}
