using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics; //used to close and restart... DEBUG!

public class GameController : MonoBehaviour
{
    public Camera GlobalCamera;

    private TileController TileControls;

    public Player ActivePlayer;
    private int ActivePlayerIndex;
    private int DEBUG_CameraOrder;

    public List<Player> PlayerList;

    private BattleUI CombatUI;

    public bool AllowCombatAnimations; //shortened battle animations

    [HideInInspector] public Unit InitiatingUnit;
    [HideInInspector] public Unit ReceivingUnit;

    private int PreviousScreenWidth;
    private int PreviousScreenHeight;

    public Vector3 SpectatorCameraPosition; //the position of the camera when it's not the player's turn.

    private bool InitiatorAttacked;
    private bool DefenderAttacked;

    // Start is called before the first frame update
    void Start()
    {
        TileControls = FindObjectOfType<TileController>();
        CombatUI = FindObjectOfType<BattleUI>();

        PlayerList = new List<Player>();
        //PlayerList.AddRange(FindObjectsOfType<Player>());

        //Adds each player to the global PlayerList in the order determined during the Menu/Draft phase
        for (int i = 0; i < FindObjectsOfType<Player>().Length; i++)
        {
            if (FindObjectsOfType<Player>()[i].PlayerOrder == i + 1)
            {
                PlayerList.Add(FindObjectsOfType<Player>()[i]);
            }

            //set the PlayerControls to active and preps the unit's layers/camera
            PlayerList[i].transform.GetChild(0).gameObject.SetActive(true);
            PlayerList[i].GetComponentInChildren<Cursor>().PrepareCursorForMap();
            PlayerList[i].SetLayers();
        }

        /*
        foreach (Player GamePlayer in PlayerList) //set the PlayerControls to active
        {
            GamePlayer.transform.GetChild(0).gameObject.SetActive(true);
            GamePlayer.GetComponentInChildren<Cursor>().PrepareCursorForMap();

            GamePlayer.SetLayers();
        }
        */

        //The "Spectating camera" position is controlled by the map's size
        SpectatorCameraPosition = new Vector3(TileControls.MapSize, TileControls.MapSize, -10f);

        AddUnitsToMap();

        StartFirstTurn();
    }

    void Update()
    {
        MaintainAspectRatio();

        if (Input.GetKeyDown(KeyCode.R))
        {
            DebugCameraControl();
        }

        if(Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.O))
        {
            Process.Start(Application.dataPath + "/../YourGameName.exe");
            Application.Quit();
        }
    }

    //run through each player and set their cameras active. allows for switching between players
    void DebugCameraControl()
    {
        if (DEBUG_CameraOrder == PlayerList.Count - 1)
        {
            DEBUG_CameraOrder = 0;
        }
        else
        {
            DEBUG_CameraOrder++;
        }

        for (int i = 0; i < PlayerList.Count; i++)
        {
            PlayerList[i].PlayerCamera.gameObject.SetActive(i == DEBUG_CameraOrder);
        }
    }

    void MaintainAspectRatio() //keeps the window at 16:9
    {
        var NewWidth = Screen.width;
        var NewHeight = Screen.height;

        if (PreviousScreenWidth != NewWidth) // if the user is changing the width
        {
            // update the height
            float heightAccordingToWidth = NewWidth / 16.0f * 9.0f;
            Screen.SetResolution(NewWidth, (int) Mathf.Round(heightAccordingToWidth), false, 0);
        }
        else if (PreviousScreenHeight != NewHeight) // if the user is changing the height
        {
            // update the width
            float widthAccordingToHeight = NewHeight / 9.0f * 16.0f;
            Screen.SetResolution((int) Mathf.Round(widthAccordingToHeight), NewHeight, false, 0);
        }

        PreviousScreenWidth = NewWidth;
        PreviousScreenHeight = NewHeight;
    }

    public Player FindPlayerOfNumber(int PlayerOrder)
    {
        return PlayerList[PlayerOrder - 1];

        /*
        List<Player> AllPlayers = new List<Player>();

        foreach(Player LookingForPlayer in PlayerList)
        {
            if(LookingForPlayer.TeamNumber == TeamID)
            {
                return LookingForPlayer;
            }
        }

        return null;
        */
    }

    public void AddUnitsToMap()
    {
        foreach(Player Commander in PlayerList)
        {
            foreach(Unit SpawningUnit in Commander.UnitsOnTeam)
            {
                //also sets their health bars active and sets their spells to Level 1
                SpawningUnit.UnitHealthBar.gameObject.SetActive(true);
                SpawningUnit.ResetSpellLevels();

                SpawningUnit.CurrentTile = TileControls.TileMap[0, 0]; //gives a "previous space" for the MoveUnitTo() coroutine

                SpawningUnit.StartCoroutine(SpawningUnit.MoveUnitTo(TileControls.TileMap[(int) (SpawningUnit.Leader.UnitsOnTeam.IndexOf(SpawningUnit) * 1.5f) + 1, PlayerList.IndexOf(SpawningUnit.Leader) * 8], false));

                /*
                int UnitX_Pos = FindUnoccupiedSpace();

                SpawningUnit.StartCoroutine(SpawningUnit.MoveUnitTo(TileControls.TileMap[UnitX_Pos, FindUnoccupiedSpaceOnX(UnitX_Pos)], false));
                */
            }
        }
    }

    void StartFirstTurn()
    {
        ActivePlayer = PlayerList[0];
        ActivePlayerIndex = 0;

        //deactivate the units/cursors of inactive players; do the opposite for the newly active player.
        foreach (Player CheckPlayer in PlayerList)
        {
            if(CheckPlayer == ActivePlayer)
            {
                CheckPlayer.GetComponentInChildren<Cursor>().ChangeCursorState(Cursor.CursorState.CURSOR_VIEWING);

                foreach (Unit PlayerUnit in CheckPlayer.UnitsOnTeam)
                {
                    PlayerUnit.CanAct = true;
                    PlayerUnit.RemoveStatusEffects();
                }
            }
            else
            {
                CheckPlayer.GetComponentInChildren<Cursor>().ChangeCursorState(Cursor.CursorState.CURSOR_STANDBY); //deactivate this player's cursor

                //move the player's camera to begin spectation
            }
        }
    }

    public void StartNextTurn()
    {
        foreach (Unit PlayerUnit in ActivePlayer.UnitsOnTeam) //Return the units of the previously active player. since these units can't move anyways, return the acted units to action so that their colors return
        {
            PlayerUnit.ReturnToAction();
        }

        if (ActivePlayerIndex < PlayerList.Count - 1)
        {
            ActivePlayerIndex++;
        }
        else
        {
            ActivePlayerIndex = 0;
        }

        ActivePlayer = PlayerList[ActivePlayerIndex];

        //deactivate the units/cursors of inactive players; do the opposite for the newly active player.
        foreach (Player CheckPlayer in PlayerList)
        {
            if (CheckPlayer == ActivePlayer)
            {
                CheckPlayer.GetComponentInChildren<Cursor>().ChangeCursorState(Cursor.CursorState.CURSOR_VIEWING);

                foreach (Unit PlayerUnit in CheckPlayer.UnitsOnTeam)
                {
                    PlayerUnit.CanAct = true;
                    PlayerUnit.RemoveStatusEffects(); //remove effects AND regenerate an orb for magic users.
                }
            }
            else
            {
                CheckPlayer.GetComponentInChildren<Cursor>().ChangeCursorState(Cursor.CursorState.CURSOR_STANDBY); //deactivate this player's cursor

                foreach (Unit PlayerUnit in CheckPlayer.UnitsOnTeam)
                {
                    if (PlayerUnit.SpentTheirPlayerTurnFrozen) //unfreeze units that just lost their turn to Freeze
                    {
                        PlayerUnit.VisualEffects.IceCube.SetActive(false);
                        PlayerUnit.BeginUnitAnimation("Unfrozen");
                        PlayerUnit.IsFrozen = false;
                        PlayerUnit.SpentTheirPlayerTurnFrozen = false;
                    }
                }

                //move the player's camera to begin spectation
            }
        }
    }

    /*

    int FindUnoccupiedSpace() //DEBUG; DELETE LATER
    {
        int RandomSpawnX = Random.Range(1, 11);

        if (TileControls.TileMap[RandomSpawnX, 0].OccupyingUnit == null)
        {
            return RandomSpawnX;
        }
        else if(TileControls.TileMap[1, 0].OccupyingUnit == null)
        {
            return 1;
        }
        else if (TileControls.TileMap[2, 0].OccupyingUnit == null)
        {
            return 2;
        }
        else if (TileControls.TileMap[3, 0].OccupyingUnit == null)
        {
            return 3;
        }

        return 0;
    }

    int FindUnoccupiedSpaceOnX(int ChosenX)
    {
        int RandomSpawnY = Random.Range(0,11);

        if (TileControls.TileMap[ChosenX, RandomSpawnY].OccupyingUnit == null)
        {
            return RandomSpawnY;
        }
        else if (TileControls.TileMap[ChosenX, 1].OccupyingUnit == null)
        {
            return 1;
        }
        else if (TileControls.TileMap[ChosenX, 2].OccupyingUnit == null)
        {
            return 2;
        }
        else if (TileControls.TileMap[ChosenX, 3].OccupyingUnit == null)
        {
            return 3;
        }
        else if (TileControls.TileMap[ChosenX, 4].OccupyingUnit == null)
        {
            return 4;
        }
        else if (TileControls.TileMap[ChosenX, 5].OccupyingUnit == null)
        {
            return 5;
        }

        return 0;
    }
    */

    public IEnumerator CombatCutscene(Unit AttackingUnit, Unit DefendingUnit) //Take both cameras and show the combat animation.
    {
        InitiatingUnit = AttackingUnit;
        ReceivingUnit = DefendingUnit;

        //Neither have hit yet. If they hit, apply recoil or healing if their weapons have it
        InitiatorAttacked = false;
        DefenderAttacked = false;

        //flip the units to face each other if necessary.
        if ((InitiatingUnit.CurrentTile.X_Position < ReceivingUnit.CurrentTile.X_Position && !InitiatingUnit.FacingRight) || (InitiatingUnit.CurrentTile.X_Position > ReceivingUnit.CurrentTile.X_Position && InitiatingUnit.FacingRight))
        {
            InitiatingUnit.FlipUnit();
        }
        if ((ReceivingUnit.CurrentTile.X_Position < InitiatingUnit.CurrentTile.X_Position && !ReceivingUnit.FacingRight) || (ReceivingUnit.CurrentTile.X_Position > InitiatingUnit.CurrentTile.X_Position && ReceivingUnit.FacingRight))
        {
            ReceivingUnit.FlipUnit();
        }

        AttackingUnit.Leader.PlayerUI.PlayerCursor.ChangeCursorState(Cursor.CursorState.CURSOR_STANDBY); //start by removing the cursor until combat ends

        CombatUI.InitiatingUnit = AttackingUnit;

        //Camera moves between each unit (midpoint) and scrolls based on the distance between each.
        float NewCamSize = 3f + TileControls.DistanceBetweenUnits(AttackingUnit, DefendingUnit);
        Vector3 NewCamPosition = (AttackingUnit.transform.position + DefendingUnit.transform.position) / 2f;
        NewCamPosition.z = -10f; //ensures that z-position of camera isn't changed

        if (AllowCombatAnimations)
        {
            HideNonCombatants(InitiatingUnit, ReceivingUnit); //non-involved units are removed from the field to reduce clutter

            yield return StartCoroutine(MoveAllCameras(GetPlayerCameras(), NewCamPosition, NewCamSize));
        }

        yield return new WaitForSeconds(0.5f); //small delay before units start fighting

        yield return StartCoroutine(UnitAttack(AttackingUnit, DefendingUnit));

        //counters and double attacks are only possible in non-staff combat
        if (!AttackingUnit.HeldWeapon.TargetsAllies)
        {
            //if the defending unit can counter, allow them to
            if (!DefendingUnit.HeldWeapon.TargetsAllies && DefendingUnit.CurrentHealth > 0 && DefendingUnit.CanReturnAttackAgainst(AttackingUnit, TileControls.DistanceBetweenUnits(AttackingUnit, DefendingUnit))) //If the receiver is alive, in range for an attack and can hit back, allow them to attack.
            {
                yield return StartCoroutine(UnitAttack(DefendingUnit, AttackingUnit));
            }

            if (AttackingUnit.CurrentHealth > 0 && AttackingUnit.CanDoubleAgainst(true, DefendingUnit)) //if the initiator can double, let them attack again.
            {
                yield return StartCoroutine(UnitAttack(AttackingUnit, DefendingUnit));
            }

            //if the defender can double, let them attack again. Note that they also need to be able to counter.
            if (!DefendingUnit.HeldWeapon.TargetsAllies && DefendingUnit.CurrentHealth > 0 && (DefendingUnit.CanDoubleAgainst(false, AttackingUnit) && DefendingUnit.CanReturnAttackAgainst(AttackingUnit, TileControls.DistanceBetweenUnits(AttackingUnit, DefendingUnit))))
            {
                yield return StartCoroutine(UnitAttack(DefendingUnit, AttackingUnit));
            }
        }

        yield return new WaitForSeconds(1f);

        if (AllowCombatAnimations) //give some time before returning to the field
        {
            ReturnNonCombatants(AttackingUnit, DefendingUnit); //other units are returned to the field

            yield return StartCoroutine(ReturnAllCameras());
        }

        //return units to rightward position to increase clarity/uniformity on the map
        if (!InitiatingUnit.FacingRight)
        {
            InitiatingUnit.FlipUnit();
        }
        if (!ReceivingUnit.FacingRight)
        {
            ReceivingUnit.FlipUnit();
        }

        //weapon combats can lead to recoil based on each unit's weapon. if they're just being healed, it won't activate... it checks to see whether they dealt damage.
        if (InitiatorAttacked && AttackingUnit.HeldWeapon.HealthRecoil > 0 && AttackingUnit.CurrentHealth > 0) //self-inflicted damage after combat
        {
            AttackingUnit.PostCombatHealthChange(AttackingUnit.HeldWeapon.HealthRecoil);
        }
        else if (InitiatorAttacked && AttackingUnit.HeldWeapon.HealthRecoil < 0) //healing weapon
        {
            for (int i = 0; i < AttackingUnit.Leader.UnitsOnTeam.Count; i++) //heal all allies within 2 spaces for the recoil value
            {
                if (TileControls.DistanceBetweenUnits(AttackingUnit, AttackingUnit.Leader.UnitsOnTeam[i]) <= 2 && AttackingUnit.Leader.UnitsOnTeam[i].CurrentHealth > 0)
                {
                    AttackingUnit.Leader.UnitsOnTeam[i].PostCombatHealthChange(AttackingUnit.HeldWeapon.HealthRecoil);
                }
            }
        }

        //Same as above, but for the defender
        if (DefenderAttacked && DefendingUnit.HeldWeapon.HealthRecoil > 0 && DefendingUnit.CurrentHealth > 0) //self-inflicted damage after combat
        {
            DefendingUnit.PostCombatHealthChange(DefendingUnit.HeldWeapon.HealthRecoil);
        }
        else if (DefenderAttacked && DefendingUnit.HeldWeapon.HealthRecoil < 0) //healing weapon
        {
            for (int i = 0; i < DefendingUnit.Leader.UnitsOnTeam.Count; i++) //heal all allies within 2 spaces for the recoil value
            {
                if (TileControls.DistanceBetweenUnits(DefendingUnit, DefendingUnit.Leader.UnitsOnTeam[i]) <= 2 && DefendingUnit.Leader.UnitsOnTeam[i].CurrentHealth > 0)
                {
                    DefendingUnit.Leader.UnitsOnTeam[i].PostCombatHealthChange(DefendingUnit.HeldWeapon.HealthRecoil);
                }
            }
        }

        AttackingUnit.RemoveFromAction();

        //ensures that the attacking unit has their spells set to level 1 once they return to the map
        AttackingUnit.ResetSpellLevels();

        //return the cursor so long as the player's turn isn't over
        if (!AttackingUnit.Leader.PlayerTurnIsOver())
        {
            AttackingUnit.Leader.PlayerUI.PlayerCursor.ChangeCursorState(Cursor.CursorState.CURSOR_VIEWING); //return cursor after combat ends
        }

        yield return null;
    }

    public IEnumerator MoveAllCameras(List<Camera> AllCameras, Vector3 NewPosition, float NewScale)
    {
        if (AllowCombatAnimations) //selecting Quick Combat removes animations
        {
            CombatUI.UI_Activated = !CombatUI.UI_Activated; //toggle the battle UI as the camera sets up.
            CombatUI.StartCoroutine(CombatUI.ToggleBattleUI(InitiatingUnit, ReceivingUnit)); //sets or removes cinematic bar... the coroutine toggles it

            //save the previous camera position/scale for the ReturnAllCameras() function.
            for(int i = 0; i < PlayerList.Count; i++)
            {
                PlayerList[i].PreviousCamPosition = PlayerList[i].PlayerCamera.transform.position;
                PlayerList[i].PreviousCamScale = PlayerList[i].PlayerCamera.orthographicSize;
            }

            float TimeElapsed = 0f;

            while (TimeElapsed < 1.5f)
            {
                for (int i = 0; i < AllCameras.Count; i++)
                {
                    AllCameras[i].transform.position = Vector3.Lerp(AllCameras[i].transform.position, NewPosition, TimeElapsed / 8f);

                    AllCameras[i].orthographicSize = Mathf.Lerp(AllCameras[i].orthographicSize, NewScale, TimeElapsed / 8f);
                }

                TimeElapsed += Time.deltaTime;

                //ensure that the global battle UI size matches the players' cameras.
                GlobalCamera.orthographicSize = AllCameras[0].orthographicSize;
                GlobalCamera.transform.position = AllCameras[0].transform.position;

                yield return new WaitForEndOfFrame();
            }

            for (int i = 0; i < AllCameras.Count; i++)
            {
                AllCameras[i].transform.position = NewPosition;
                AllCameras[i].orthographicSize = NewScale;
            }

            GlobalCamera.orthographicSize = AllCameras[0].orthographicSize;
            GlobalCamera.transform.position = AllCameras[0].transform.position;
        }

        yield return null;
    }

    public IEnumerator ReturnAllCameras()
    {
        if (AllowCombatAnimations) //selecting Quick Combat removes animations
        {
            CombatUI.UI_Activated = !CombatUI.UI_Activated; //toggle the battle UI as the camera sets up.
            CombatUI.StartCoroutine(CombatUI.ToggleBattleUI(InitiatingUnit, ReceivingUnit)); //sets or removes cinematic bar... the coroutine toggles it

            float TimeElapsed = 0f;

            while (TimeElapsed < 1f)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    PlayerList[i].PlayerCamera.transform.position = Vector3.Lerp(PlayerList[i].PlayerCamera.transform.position, PlayerList[i].PreviousCamPosition, TimeElapsed / 6f);

                    PlayerList[i].PlayerCamera.orthographicSize = Mathf.Lerp(PlayerList[i].PlayerCamera.orthographicSize, PlayerList[i].PreviousCamScale, TimeElapsed / 6f);
                }

                TimeElapsed += Time.deltaTime;

                //ensure that the global battle UI size matches the players' cameras.
                GlobalCamera.orthographicSize = PlayerList[0].PlayerCamera.orthographicSize;
                GlobalCamera.transform.position = PlayerList[0].PlayerCamera.transform.position;

                yield return new WaitForEndOfFrame();
            }

            for (int i = 0; i < PlayerList.Count; i++)
            {
                PlayerList[i].PlayerCamera.transform.position = PlayerList[i].PreviousCamPosition;
                PlayerList[i].PlayerCamera.orthographicSize = PlayerList[i].PreviousCamScale;
            }

            GlobalCamera.orthographicSize = PlayerList[0].PlayerCamera.orthographicSize;
            GlobalCamera.transform.position = PlayerList[0].PlayerCamera.transform.position;
        }

        yield return null;
    }

    //Checks whether all player cameras have reached the desired position and scale
    bool AllCamerasPlaced(List<Camera> AllCameras, Vector3 RequiredPosition, float RequiredScale)
    {
        for(int i = 0; i < AllCameras.Count; i++)
        {
            if (Mathf.Abs(AllCameras[i].orthographicSize - RequiredScale) > 0.005f || (AllCameras[i].transform.position - RequiredPosition).sqrMagnitude > 0.005f)
            {
                return false;
            }
        }
        
        return true;
    }

    List<Camera> GetPlayerCameras()
    {
        List<Camera> AllCams = new List<Camera>();

        for(int i = 0; i < PlayerList.Count; i++)
        {
            AllCams.Add(PlayerList[i].PlayerCamera);
        }

        return AllCams;
    }

    /*
    public IEnumerator MoveCamera(Camera PlayerCamera, Camera EnemyCamera, Vector3 NewPosition, float NewScale) //Moves both cameras to their required position during combat
    {
        if (AllowCombatAnimations) //selecting Quick Combat removes animations
        {
            CombatUI.UI_Activated = !CombatUI.UI_Activated; //toggle the battle UI as the camera sets up.
            CombatUI.StartCoroutine(CombatUI.ToggleBattleUI(InitiatingUnit, ReceivingUnit)); //sets or removes cinematic bar... the coroutine toggles it

            while ((Mathf.Abs(PlayerCamera.orthographicSize - NewScale) > 0.005f || (PlayerCamera.transform.position - NewPosition).sqrMagnitude > 0.005f)
                || (Mathf.Abs(EnemyCamera.orthographicSize - NewScale) > 0.005f || (EnemyCamera.transform.position - NewPosition).sqrMagnitude > 0.005f))
            {
                PlayerCamera.transform.position = Vector3.Lerp(PlayerCamera.transform.position, NewPosition, 0.05f);

                PlayerCamera.orthographicSize = Mathf.Lerp(PlayerCamera.orthographicSize, NewScale, 0.05f);

                EnemyCamera.transform.position = Vector3.Lerp(PlayerCamera.transform.position, NewPosition, 0.05f);

                EnemyCamera.orthographicSize = Mathf.Lerp(PlayerCamera.orthographicSize, NewScale, 0.05f);

                //ensure that the global battle UI size matches the players' cameras.
                GlobalCamera.orthographicSize = PlayerCamera.orthographicSize;
                GlobalCamera.transform.position = PlayerCamera.transform.position;

                yield return new WaitForEndOfFrame();
            }

            PlayerCamera.orthographicSize = NewScale;
            PlayerCamera.transform.position = NewPosition;

            EnemyCamera.orthographicSize = NewScale;
            EnemyCamera.transform.position = NewPosition;

            GlobalCamera.orthographicSize = PlayerCamera.orthographicSize;
            GlobalCamera.transform.position = PlayerCamera.transform.position;
        }

        yield return null;
    }
    */

    public IEnumerator UnitAttack(Unit Attacker, Unit Defender) //Units fight! CheckIfHit allows for post-combat effects to proc that rely on the unit attacking.
    {
        if (!Attacker.IsFrozen)
        {
            Attacker.BeginUnitAnimation(Attacker.AttackAnimation); //Set the unit's attack trigger, ex. Infantry_Sword.

            //Specific effects depend on whether the unit is attacking or using a staff.
            if (!Attacker.HeldWeapon.TargetsAllies && Attacker.Leader != Defender.Leader) //weapon/spell
            {
                int HitRoll = Random.Range(0, 100);
                int CritRoll = Random.Range(0, 100); //if you hit, the hit can potentially crit.

                //if the unit hits a critical, darken the screen before attacking. This blinder takes up half the time to deal the attack
                if (AllowCombatAnimations && HitRoll < Attacker.CombinedHit(Attacker == InitiatingUnit, Defender) && CritRoll < Attacker.CombinedCrit(Defender))
                {
                    //changes the played anim to its critical form
                    //Attacker.BodyAnim.SetTrigger("Critical");
                    //Attacker.AttackWaitTime += 1f; //wait time goes from 0.5s to 1.5s (or 2s to 3s for spells).

                    float ElapsedTime = 0f;

                    while (ElapsedTime < Attacker.AttackWaitTime / 2f)
                    {
                        CombatUI.CritBlinder.color = new Color(0f, 0f, 0f, Mathf.Lerp(CombatUI.CritBlinder.color.a, 0.4f, ElapsedTime / (Attacker.AttackWaitTime / 2f)));
                        ElapsedTime += Time.deltaTime;

                        yield return new WaitForEndOfFrame();
                    }

                    CombatUI.CritBlinder.color = new Color(0f, 0f, 0f, 0.4f);

                    yield return new WaitForSeconds(Attacker.AttackWaitTime / 2f);
                }
                else //wait 0.5 seconds
                {
                    yield return new WaitForSeconds(Attacker.AttackWaitTime); //give time for the unit's animation to occur. The "hit" is at the 0.5 second mark on all attack animations
                }

                if (HitRoll < Attacker.CombinedHit(Attacker == InitiatingUnit, Defender)) //hit can be based on initiation; check that the attacker is the initiating unit
                {
                    //The unit hit successfully; since there can be multiple attacks, it's set true even if they miss their next attack. Used for recoil
                    if (Attacker == InitiatingUnit)
                    {
                        InitiatorAttacked = true;
                    }
                    else
                    {
                        DefenderAttacked = true;
                    }

                    Attacker.DealDamage(Attacker.CombinedAttack(), CritRoll < Attacker.CombinedCrit(Defender), Defender); //perform the attack. Crits occur when the roll is lower than unit's Technique.
                }
                else //unit missed; display Missed visuals
                {
                    FindObjectOfType<DamageStars>().ActivateMissVisual(Defender);
                }

                //There's a 0.5 second wait after the attack. If the unit crit during the animation, this time is spent removing the blinder as well.
                if (AllowCombatAnimations)
                {
                    CombatUI.PlayerInfo.UpdateDisplayHealth(); //update the large health bars in the Combat Display
                    CombatUI.EnemyInfo.UpdateDisplayHealth();

                    //clear out the dark crit shader
                    float ElapsedTime = 0f;

                    while (ElapsedTime < 0.5f)
                    {
                        CombatUI.CritBlinder.color = new Color(0f, 0f, 0f, Mathf.Lerp(CombatUI.CritBlinder.color.a, 0f, ElapsedTime / 0.5f));
                        ElapsedTime += Time.deltaTime;

                        yield return new WaitForEndOfFrame();
                    }

                    CombatUI.CritBlinder.color = Color.clear;
                }
                else
                {
                    yield return new WaitForSeconds(0.5f); // same wait without crit blinder
                }
            }
            else if (Attacker.HeldWeapon.TargetsAllies && Attacker.Leader == Defender.Leader) //staff/support ability
            {
                //Whenever a unit uses a support ability, turn the Crit Blinder into a soothing blue.
                if (AllowCombatAnimations)
                {
                    float ElapsedTime = 0f;

                    while (ElapsedTime < Attacker.AttackWaitTime / 2f)
                    {
                        CombatUI.CritBlinder.color = new Color(0.3f, 0.6f, 0.6f, Mathf.Lerp(CombatUI.CritBlinder.color.a, 0.6f, ElapsedTime / (Attacker.AttackWaitTime / 2f)));
                        ElapsedTime += Time.deltaTime;

                        yield return new WaitForEndOfFrame();
                    }

                    CombatUI.CritBlinder.color = new Color(0.3f, 0.6f, 0.6f, 0.6f);

                    yield return new WaitForSeconds(Attacker.AttackWaitTime / 2f);
                }
                else //wait 0.5 seconds
                {
                    yield return new WaitForSeconds(Attacker.AttackWaitTime); //give time for the unit's animation to occur. The "hit" is at the 0.5 second mark on all attack animations
                }

                //Use your staff
                Attacker.UseSupportAbility(Defender);

                //Staves ALWAYS apply recoil if necessary; they always hit.
                InitiatorAttacked = true;

                if (AllowCombatAnimations)
                {
                    //update the large health bars in the Combat Display
                    CombatUI.EnemyInfo.UpdateDisplayHealth();

                    //clear out the crit shader
                    float ElapsedTime = 0f;

                    while (ElapsedTime < 0.5f)
                    {
                        CombatUI.CritBlinder.color = new Color(0.3f, 0.6f, 0.6f, Mathf.Lerp(CombatUI.CritBlinder.color.a, 0f, ElapsedTime / 0.5f));
                        ElapsedTime += Time.deltaTime;

                        yield return new WaitForEndOfFrame();
                    }

                    CombatUI.CritBlinder.color = Color.clear;
                }
                else
                {
                    yield return new WaitForSeconds(0.5f); // same wait without crit blinder
                }
            }

            yield return new WaitForSeconds(0.5f); // wait a moment before resuming battle
        }

        yield return null;
    }

    public IEnumerator PostCombatRecoil(Unit ThisUnit, int RecoilValue)
    {
        if(ThisUnit.CurrentHealth > 0)
        {
            ThisUnit.PostCombatHealthChange(RecoilValue);
        }

        //yield return new WaitForSeconds(0.5f); // wait a moment before resuming battle

        yield return null;
    }

    public void HideNonCombatants(Unit Attacker, Unit Defender) //causes all units besides those in the combat to disappear
    {
        foreach(Unit NonCombatant in FindObjectsOfType<Unit>())
        {
            if(NonCombatant != Attacker && NonCombatant != Defender) //move offscreen
            {
                NonCombatant.transform.position = new Vector2(100f, 100f);
            }
            else //combatant; move their health bar instead
            {
                NonCombatant.UnitHealthBar.transform.localPosition = new Vector2(100f, 100f);
            }
        }
    }

    public void ReturnNonCombatants(Unit Attacker, Unit Defender)
    {
        foreach (Unit ReturningUnit in FindObjectsOfType<Unit>())
        {
            ReturningUnit.transform.position = ReturningUnit.CurrentTile.transform.position;

            if (ReturningUnit == Attacker || ReturningUnit == Defender) //return health bars to their proper place
            {
                ReturningUnit.UnitHealthBar.transform.localPosition = new Vector2(-0.05f, -0.6f);
            }
        }
    }
}
