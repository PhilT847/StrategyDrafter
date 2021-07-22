using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Unit : MonoBehaviour
{
    public string Name;
    public Player Leader;

    [HideInInspector] public UnitEffects VisualEffects; //visual effects like barrier, frozen, and healing particles

    public string UnitType; //unit type determines which tiles a unit can move across, as well as which weapons are effective against them. *ARMORED, PEGASUS, INFANTRY, CAVALRY*

    public int MovementDistance; //the amount of spaces a unit can move each turn.
    //Cavalry units increase the movement cost of Forests to 3. Pegasus units decrease the cost of all tile types to 1.

    public int MaxHealth, BarrierHealth;
    [HideInInspector] public int CurrentHealth;

    public int Rating, Attack, Speed, Technique, Defense, Resistance; //base attack before accounting for the strength of weapons and spells.

    public Transform WeaponHand; //the hand that holds your weapons
    public Weapon HeldWeapon;
    public Weapon SecondaryWeapon; //the non-equipped weapon not used in combat unless equipped
    public bool UsesMagic; //during Customizer phase, determines whether this unit uses magic/staves. Removes the OrbCounter from the unit's HealthBar if they don't use magic.
    public int OrbCount; //the amount of orbs the unit has for casting spells.

    public Tile CurrentTile;

    public List<Unit> PotentialTargets; //units that this unit can attack.
    public int SelectedEnemyIndex;

    public bool CanAct, IsFrozen, SpentTheirPlayerTurnFrozen; //casting Barrier on a unit reduces incoming damage by 50%.

    [HideInInspector] public HealthBar UnitHealthBar;

    public Animator BodyAnim, OrbAnim; //the unit's animator as well as the Orb Animator which moves the unit's Orbs when casting spells.
    [HideInInspector] public string AttackAnimation; //the unit's attack animation based on movement type and weapon.
    [HideInInspector] public float AttackWaitTime; //the time before the unit deals damage. This time changes based on crits and spell levels
    [HideInInspector] public bool FacingRight;

    [HideInInspector] public SpriteRenderer[] BodySprites;

    public bool CanReturnAttackAgainst(Unit EnemyUnit, int UnitDistance) //Determines whether the unit can counterattack. Based on range and the enemy's weapon (some lances negate counters).
    {
        return UnitDistance >= HeldWeapon.MinRange && UnitDistance <= HeldWeapon.GetMaxRange() && !EnemyUnit.HeldWeapon.NegatesEnemyCounterattacks;
    }

    public int CombinedAttack()
    {
        return Attack + HeldWeapon.Might;
    }

    public int CombinedHit(bool IsInitiating, Unit EnemyUnit) //combined hit of the unit accounting for weapon and potential bonuses from Lance weapons. Reduced by enemy Speed * 2.5 as well as the enemy weapon's AvoidBonus
    {
        int HitChance = (int)(Technique * 2.5f) + HeldWeapon.HitChance - (int)(EnemyUnit.Speed * 2.5f) - EnemyUnit.HeldWeapon.AvoidBonus;

        if (HeldWeapon.GetComponent<Lance>())
        {
            //being in the lance's preferred phase doubles hit chance
            if ((IsInitiating && HeldWeapon.GetComponent<Lance>().PlayerPhaseLance) || (!IsInitiating && !HeldWeapon.GetComponent<Lance>().PlayerPhaseLance))
            {
                HitChance *= 2;
            }
        }

        //Make the text easier to read by capping at 100
        if(HitChance > 100)
        {
            return 100;
        }

        return HitChance;
    }

    public int CombinedCrit(Unit EnemyUnit) //crit can be altered based on the enemy's movement type (ex. Effective Against Armored)
    {
        switch (EnemyUnit.UnitType)
        {
            case "Armor":
                {
                    if (HeldWeapon.EffectiveAgainstArmored)
                    {
                        return 100;
                    }

                    break;
                }
            case "Flying":
                {
                    if (HeldWeapon.EffectiveAgainstFlying)
                    {
                        return 100;
                    }

                    break;
                }
            case "Cavalry":
                {
                    if (HeldWeapon.EffectiveAgainstCavalry)
                    {
                        return 100;
                    }

                    break;
                }
        }

        return (int)(Technique * 2.5f) + HeldWeapon.CritBonus;
    }

    public string MightAgainst(Unit EnemyUnit) //calculates how much damage a unit will deal; accounts for defenses and Barrier.
    {
        int UnitDamage = CombinedAttack();

        if (!HeldWeapon.TargetsAllies && CanReturnAttackAgainst(EnemyUnit, FindObjectOfType<TileController>().DistanceBetweenUnits(this, EnemyUnit))) //if this attack deals damage, calculate how much.
        {
            if (HeldWeapon.MagicDamage && !HeldWeapon.IgnoresDefenses) //magic damage is reduced by Res
            {
                UnitDamage -= EnemyUnit.Resistance;
            }
            else if (!HeldWeapon.IgnoresDefenses) //physical damage is reduced by Def
            {
                UnitDamage -= EnemyUnit.Defense;
            }
        }
        else //supportive abilities do not deal damage; thus, return 0.
        {
            return "--";
        }

        /*
        if (EnemyUnit.HasBarrier && !HeldWeapon.IgnoresDefenses) //barrier halves incoming damage
        {
            UnitDamage /= 2;
        }
        */

        if(UnitDamage < 1) //units ALWAYS deal at least 1 damage.
        {
            return "1";
        }

        return "" + UnitDamage;
    }

    public bool CanDoubleAgainst(bool IsInitiating, Unit EnemyUnit) //unit can double attack so long as they have higher speed and the enemy lacks follow-up denial.
    {
        if ((EnemyUnit.HeldWeapon.GetComponent<Lance>() && EnemyUnit.HeldWeapon.GetComponent<Lance>().DenyEnemyFollowUp && IsInitiating) || HeldWeapon.CannotMakeDoubleAttacks)
        {
            return false;
        }

        return Speed > EnemyUnit.Speed;
    }

    public void TakeDamage(int dmg, Unit Source) //take a non-zero amount of damage
    {
        //Damage hits a unit's Barrier first, should they have one.
        if(BarrierHealth > 0)
        {
            if (!Source.HeldWeapon.IgnoresDefenses) //weapon does NOT ignore barrier
            {
                BarrierHealth -= dmg;

                if (BarrierHealth < 1)
                {
                    dmg = -BarrierHealth; //all excess damage is taken by the unit... this number is the overflow damage

                    BarrierHealth = 0;

                    //shatter barrier anim/sound
                    VisualEffects.BarrierAnim.SetTrigger("Shatter");
                }
            }
            else
            {
                BarrierHealth = 0;

                //shatter barrier anim/sound
                VisualEffects.BarrierAnim.SetTrigger("Shatter");
            }
        }

        if(dmg > 0) //damage may have been nullified by a barrier; change animations accordingly
        {
            CurrentHealth -= dmg;

            BeginUnitAnimation(UnitType + "_Damaged");

            if (CurrentHealth < 1) //kill unit
            {
                CurrentHealth = 0;

                KillUnit();
            }
        }
        else
        {
            //play "Block!" sound
        }

        UnitHealthBar.UpdateHealthBar();
    }

    public void PostCombatHealthChange(int change) //take recoil damage or heal after combat
    {
        CurrentHealth -= change;

        if (change > 0) //post combat damage... non-fatal
        {
            BeginUnitAnimation(UnitType + "_Damaged");

            if (CurrentHealth < 1)
            {
                CurrentHealth = 1;
            }
        }
        else if(change < 0) //post combat healing
        {
            VisualEffects.HealingParticles.Play();

            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        UnitHealthBar.UpdateHealthBar();
    }

    public void HealHP(int HealValue)
    {
        CurrentHealth += HealValue;

        if(CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        UnitHealthBar.UpdateHealthBar();

        //also set a sound and particle anim
        FindObjectOfType<DamageStars>().ActivateHealingStar(this, HealValue);
    }

    public void DealDamage(int Damage, bool Crit, Unit Receiver)
    {
        int TotalDamage = Damage;

        if (HeldWeapon.MagicDamage) //magic deals damage based on Resistance.
        {
            TotalDamage -= Receiver.Resistance;
        }
        else //physical attacks deal damage based on Defense.
        {
            TotalDamage -= Receiver.Defense;
        }

        if (TotalDamage < 1)
        {
            TotalDamage = 1;
        }

        if (Crit) //critical hits don't double the unit's Attack; instead, they double the damage that would be dealt after accounting for DEF/RES. 
        {
            //play critical sound
            TotalDamage *= 2;

            //frost weapons freeze the enemy when they crit.
            if (HeldWeapon.FreezeOnCrit && !Receiver.IsFrozen)
            {
                Receiver.BeginUnitAnimation("Frozen");
                Receiver.IsFrozen = true;
                Receiver.VisualEffects.IceCube.SetActive(true);
            }
        }

        if (HeldWeapon.SpellEffect != null) //if the unit has a spell effect, set it loose on the enemy. Can be an animation, particlesystem, or both.
        {
            HeldWeapon.SpellEffect.transform.position = Receiver.transform.position;

            if (HeldWeapon.SpellEffect.GetComponentInChildren<ParticleSystem>())
            {
                HeldWeapon.SpellEffect.GetComponentInChildren<ParticleSystem>().Play();
            }

            if (HeldWeapon.SpellEffect.GetComponentInChildren<Animator>())
            {
                HeldWeapon.SpellEffect.GetComponentInChildren<Animator>().SetTrigger("CastSpell");
            }
        }

        Receiver.TakeDamage(TotalDamage, this);

        //Create a damage star on the Receiver that displays damage dealt
        FindObjectOfType<DamageStars>().ActivateDamageStars(Receiver, TotalDamage, Crit);
    }

    public void ApplyBarrier(int BarrierAmount)
    {
        VisualEffects.BarrierAnim.SetTrigger("Creation");

        if (BarrierHealth < BarrierAmount)
        {
            BarrierHealth = BarrierAmount;
        }

        UnitHealthBar.UpdateHealthBar();
    }

    public void UseSupportAbility(Unit Receiver) //use a staff.
    {
        switch (HeldWeapon.GetComponent<Staff>().StaffID)
        {
            case 0: //Cure Staff; heal HP based on unit's Attack
                {
                    switch (HeldWeapon.GetComponent<Staff>().SpellLevel)
                    {
                        case 1:
                            {
                                Receiver.HealHP(Attack);
                                break;
                            }
                        case 2:
                            {
                                Receiver.HealHP(Attack);
                                break;
                            }
                        case 3:
                            {
                                Receiver.HealHP(Attack * 2);
                                break;
                            }
                    }

                    Receiver.VisualEffects.HealingParticles.Play();
                    break;
                }
            case 1: //Refresh Staff; allow an acted unit to act again... also, create some music notes.
                {
                    if (!Receiver.IsFrozen && !Receiver.CanAct)
                    {
                        Receiver.ReturnToAction();
                    }

                    Receiver.VisualEffects.MusicParticles.Play();

                    break;
                }
            case 2: //Barrier Staff; regen barrier
                {
                    switch (HeldWeapon.GetComponent<Staff>().SpellLevel)
                    {
                        case 1:
                            {
                                Receiver.ApplyBarrier(Attack - 2);
                                break;
                            }
                        case 2:
                            {
                                Receiver.ApplyBarrier(Attack);
                                break;
                            }
                        case 3:
                            {
                                Receiver.ApplyBarrier(Attack + 2);
                                break;
                            }
                    }

                    break;
                }
        }
    }

    //Spends an amount of orbs specified by the spell's level. Used by the Cursor when initiating combat.
    public void SpendOrbs(int OrbCost)
    {
        OrbCount -= OrbCost;

        if(OrbCount < 0)
        {
            OrbCount = 0;
        }
        else if(OrbCount > 3)
        {
            OrbCount = 3;
        }

        UnitHealthBar.UpdateHealthBar();
    }

    public void JoinTeam(Player NewLeader)
    {
        Leader = NewLeader;
        Leader.UnitsOnTeam.Add(this);

        transform.SetParent(Leader.transform);

        //changes the unit's color to match team colors
        UnitHealthBar.FillSprite.GetComponent<SpriteRenderer>().color = Leader.TeamColor; //set the health bar's fill color the the leader's color.
        UnitHealthBar.GetComponentInChildren<TextMeshPro>().color = Leader.TeamColor; //set the health bar's number color the the leader's color.

        for (int i = 0; i < BodySprites.Length; i++)
        {
            if (BodySprites[i].color == Color.gray)
            {
                BodySprites[i].color = Leader.TeamColor;
            }
        }
    }

    public void RemoveStatusEffects() //at the start of unit's turn, remove status effects.
    {
        if (IsFrozen) //Frozen acts on the unit's next turn, so removing it will allow the unit to move again later
        {
            RemoveFromAction();
            SpentTheirPlayerTurnFrozen = true; //by the next turn, their frost will go away. however, they must go through a turn of frozen-ness first. See GameController for the unfreezing process
        }
        /*
        else //once they return from being frozen, ensure their animator returns to normal
        {
            IceCube.SetActive(false);
            BeginUnitAnimation("Unfrozen");
        }
        */

        if(BarrierHealth > 0) //reduce barrier HP
        {
            BarrierHealth -= 1;
            UnitHealthBar.UpdateHealthBar();

            if(BarrierHealth == 0) //shatter barrier if it's fully gone
            {
                VisualEffects.BarrierAnim.SetTrigger("Shatter");
            }
        }
    }

    public void DetermineTargetableUnits(Weapon ChosenWeapon) //take the list of units created by the TileController and sort them by x-position ascending
    {
        PotentialTargets = FindObjectOfType<TileController>().HighlightUnitsWithinRange(this, ChosenWeapon);

        for (int i = 0; i < PotentialTargets.Count - 1; i++)
        {
            if (PotentialTargets[i].CurrentTile.X_Position >= PotentialTargets[i + 1].CurrentTile.X_Position)
            {
                Unit save = PotentialTargets[i];
                PotentialTargets[i] = PotentialTargets[i + 1];
                PotentialTargets[i + 1] = save;
            }
        }

        SelectedEnemyIndex = 0;
    }

    public void SwitchWeapon() //swap the Held and Secondary weapons
    {
        Weapon saved = HeldWeapon;
        HeldWeapon = SecondaryWeapon;
        SecondaryWeapon = saved;

        EquipWeapon(HeldWeapon);
    }

    public void EquipWeapon(Weapon NewWeapon)
    {
        HeldWeapon.WeaponSprite.enabled = true;
        SecondaryWeapon.WeaponSprite.enabled = false;

        //WeaponHand.sprite = NewWeapon.WeaponSprite;

        AttackAnimation = UnitType + "_" + NewWeapon.AttackAnimationTrigger; // ex. "Infantry_Bow"

        if (!HeldWeapon.GetComponent<Magic>() && !HeldWeapon.GetComponent<Staff>()) //physical weapons have 0.5s anim before hitting. magic has 2s. both are increased by 1s for crits.
        {
            AttackWaitTime = 0.5f;
        }
        else //magic
        {
            AttackWaitTime = 2f;
        }

        HeldWeapon.WeaponOwner = this; //allows the weapon to use this unit's stats in certain calculations
        SecondaryWeapon.WeaponOwner = this; //ensures that the second weapon can be seen and read
    }

    public IEnumerator MoveUnitTo(Tile EndTile, bool AllowUnitAction)
    {
        Leader.PlayerUI.PlayerCursor.ChangeCursorState(Cursor.CursorState.CURSOR_STANDBY);

        Tile SaveCurrentTile = CurrentTile; //used at the end when opening the action menu so that the unit can return if canceled

        if (CurrentTile != EndTile) //if moving tiles, swap the unit's current tile
        {
            EndTile.OccupyingUnit = this;
            CurrentTile.OccupyingUnit = null;
            CurrentTile = EndTile;
        }

        if (AllowUnitAction) //if the unit can act, make unit walk to their space and display the action menu.
        {
            //increase move speed based on shortness of distance... less moves = faster speed... moving units 1-2 spaces feels sluggish otherwise. Max dist is 10 (speed 5), min is 2 (speed ~12.8)
            float DistanceSpeedMult = 18f / Mathf.Sqrt(Vector2.Distance(transform.position, EndTile.transform.position));

            while ((transform.position - EndTile.transform.position).sqrMagnitude > 0.001)
            {
                transform.position = Vector3.Lerp(transform.position, EndTile.transform.position, DistanceSpeedMult * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            }

            transform.position = EndTile.transform.position;

            //disable the cursor and activate the Action UI
            Leader.PlayerUI.PlayerMenus[0].GetComponent<UnitActionsMenu>().ReturnIfCanceled = SaveCurrentTile;
            Leader.PlayerUI.PlayerMenus[0].GetComponent<UnitActionsMenu>().ControlledUnit = this;
            Leader.PlayerUI.ActivateMenu(0);
        }
        else //otherwise, activate the cursor again.
        {
            transform.position = EndTile.transform.position;

            Leader.PlayerUI.PlayerCursor.MoveCursorTo(EndTile.X_Position, EndTile.Y_Position);
            Leader.PlayerUI.PlayerCursor.ChangeCursorState(Cursor.CursorState.CURSOR_VIEWING);
        }

        yield return null;
    }

    public void RemoveFromAction() //this unit can no longer act; make each sprite darker by halving its RBG values.
    {
        CanAct = false;

        for(int i = 0; i < BodySprites.Length; i++)
        {
            BodySprites[i].color = new Color(BodySprites[i].color.r / 2f, BodySprites[i].color.g / 2f, BodySprites[i].color.b / 2f, 1f);
        }

        if (Leader.PlayerTurnIsOver())
        {
            FindObjectOfType<GameController>().StartNextTurn();
        }
    }

    void KillUnit()
    {
        CurrentTile.OccupyingUnit = null;
        Leader.UnitsOnTeam.Remove(this);
        Destroy(gameObject, 6f);
    }

    public void ReturnToAction() //this unit can begin acting again; return the sprites to their original color.
    {
        CanAct = true;

        for (int i = 0; i < BodySprites.Length; i++)
        {
            BodySprites[i].color = new Color(BodySprites[i].color.r * 2f, BodySprites[i].color.g * 2f, BodySprites[i].color.b * 2f, 1f);
        }

        //allow mages to regenerate 1 Orb when they have Refresh used on them or their turn ends
        SpendOrbs(-1);
    }

    public void FlipUnit() //Flips the way the unit is facing.
    {
        FacingRight = !FacingRight;

        BodyAnim.transform.parent.localScale = new Vector3(BodyAnim.transform.parent.localScale.x * -1f, 1f, 1f);
    }

    //use a unique function as being Frozen removes other animations... note that this only applies to BodyAnim
    public void BeginUnitAnimation(string Trigger)
    {
        //Unfreezing forces the unit into their damage animation and removes the Frozen status in their animator
        if(Trigger == "Unfrozen")
        {
            BodyAnim.SetBool("Frozen", false);
            BodyAnim.SetTrigger(UnitType + "_Damaged");

            return;
        }
        else if(Trigger == "Frozen") //begin the freeze animation
        {
            BodyAnim.SetBool("Frozen", true);
        }

        if(!BodyAnim.GetBool("Frozen"))
        {
            //Spells also make an amount of orbs appear based on the spell level.
            if (Trigger.Contains("Magic")|| Trigger.Contains("Staff"))
            {
                if (HeldWeapon.GetComponent<Magic>()) //tome
                {
                    OrbAnim.SetTrigger("Level" + HeldWeapon.GetComponent<Magic>().SpellLevel);
                }
                else //staff
                {
                    OrbAnim.SetTrigger("Level" + HeldWeapon.GetComponent<Staff>().SpellLevel);
                }
            }

            BodyAnim.SetTrigger(Trigger);
        }
    }

    public void ResetSpellLevels() //ensures spell levels hit zero before exiting menus and entering enemy phase combat.
    {
        if (HeldWeapon.GetComponent<Magic>())
        {
            HeldWeapon.GetComponent<Magic>().ChangeSpellLevel(1);
        }
        else if (HeldWeapon.GetComponent<Staff>())
        {
            HeldWeapon.GetComponent<Staff>().ChangeSpellLevel(1);
        }

        if (SecondaryWeapon.GetComponent<Magic>())
        {
            SecondaryWeapon.GetComponent<Magic>().ChangeSpellLevel(1);
        }
        else if (SecondaryWeapon.GetComponent<Staff>())
        {
            SecondaryWeapon.GetComponent<Staff>().ChangeSpellLevel(1);
        }
    }


}
