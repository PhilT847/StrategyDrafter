using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour //abstract class that provides methods to Sword, Lance, Bow, and Staff classes
{
    [HideInInspector] public Unit WeaponOwner;

    public string WeaponName;
    public string ActionText; //"Attack" or "Heal"

    public string WeaponDescription;

    public string AttackAnimationTrigger; //a string that tells the unit what animation to play when attacking. See Unit.

    public SpriteRenderer WeaponSprite; //the renderer that's enabled/disabled based on whether the weapon's equipped
    public Sprite DisplaySprite; //sprite shown in action option display and forecast
    public GameObject SpellEffect; //an extra animation or particlesystem that can be placed on the enemy when hit

    public int MinRange; //minimum range for counters/attacks; 1 for most melee/magic, 2 for bows.
    public int MaxRange; //maximum range for counters/attacks; magic and some ranged weapons can attack at 1-2 or larger ranges

    public int Might; //the damage value added to unit's Attack during combat.

    public int HitChance; //base hit chance; increased by unit's Technique.
    public int CritBonus; //starts at 0 for most weapons; increased by unit's Technique.
    public int AvoidBonus;

    public bool MagicDamage; //magic weapons deal damage based on Resistance instead of Defense.
    public bool TargetsAllies; //spell or Technique targets allies
    public bool IgnoresDefenses; //ignores Barrier, Defense, and Resistance.
    public bool FreezeOnCrit; //used by the Frost Lance and Blizzard spell
    public int HealthRecoil; //recoil damage caused by casting spells or certain physical weapons. If negative, it instead heals the user and adjacent allies.

    public bool CannotMakeDoubleAttacks;
    public bool NegatesEnemyCounterattacks; //certain weapons allow a unit to initate combat without the foe being able to counter, even if they're in range.

    public bool EffectiveAgainstCavalry; //guaranteed crit vs. enemies on horseback
    public bool EffectiveAgainstArmored; //guaranteed crit vs. armored enemies
    public bool EffectiveAgainstFlying; //guaranteed crit vs. pegasus foes

    public abstract int GetMaxRange(); //max range changes on some weapons
}
