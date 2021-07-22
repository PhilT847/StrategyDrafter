public class Magic : Weapon
{
    public int SpellLevel;

    public int MightBonusPerLevel;
    public int HitBonusPerLevel;
    public int CritBonusPerLevel;

    public override int GetMaxRange()
    {
        return MaxRange;
    }

    //Changes a spell's level, altering its power and attributes.
    public void ChangeSpellLevel(int LevelChange)
    {
        //Certain methods increase/decrease spell level. Allow the spell level to cycle from 3->0 and vice versa
        if (LevelChange > 3)
        {
            LevelChange = 1;
        }
        else if (LevelChange < 1)
        {
            LevelChange = 3;
        }

        //if the caster's orb count isn't high enough to cast a spell at this level, don't allow them to select this level.
        if (WeaponOwner.OrbCount < LevelChange)
        {
            return;
        }

        SpellLevel = LevelChange;

        //Determine the overall bonuses before the switch:case for clarity. Consider the bonus as only granted to levels 2 and 3; do SpellLevel - 1
        int MightBonus = (SpellLevel - 1) * MightBonusPerLevel;
        int HitBonus = (SpellLevel - 1) * HitBonusPerLevel;
        int ExtraCrit = (SpellLevel - 1) * CritBonusPerLevel;

        //uses the name substring as the number at the end has changed the WeaponName
        switch (WeaponName.Substring(0, 4))
        {
            case "Fire":
                {
                    WeaponName = "Fire ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    Might = 2 + MightBonus;
                    HitChance = 70 + HitBonus;
                    CritBonus = 0 + ExtraCrit;
                    HealthRecoil = -3 + (SpellLevel * 3); //deals 0->3->6 recoil damage

                    /*
                    if(SpellLevel == 3)
                    {
                        EffectiveAgainstArmored = true;
                        HealthRecoil = 5;
                        WeaponDescription = "Deals critical damage against Armored foes. If unit attacked, unit takes 5 damage after combat.";
                    }
                    else
                    {
                        EffectiveAgainstArmored = false;
                        HealthRecoil = 0;
                        WeaponDescription = "";
                    }
                    */

                    if(HealthRecoil > 0)
                    {
                        WeaponDescription = "Unit takes " + HealthRecoil + " damage after combat.";
                    }
                    else
                    {
                        WeaponDescription = "";
                    }

                    break;
                }
            case "Wind":
                {
                    WeaponName = "Wind ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    Might = 0 + MightBonus;
                    HitChance = 120 + HitBonus;
                    CritBonus = 5 + ExtraCrit;

                    //Wind grants 10-30 Avoid based on level
                    AvoidBonus = SpellLevel * 10;

                    string NewDescription = "";

                    NewDescription += "Increases Avoid by " + AvoidBonus + ".";

                    if(SpellLevel == 3)
                    {
                        NewDescription += " Deals critical damage against Flying foes.";
                        EffectiveAgainstFlying = true;
                    }
                    else
                    {
                        EffectiveAgainstFlying = false;
                    }

                    WeaponDescription = NewDescription;

                    break;
                }
            case "Thun": //thunder
                {
                    WeaponName = "Thunder ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    Might = 1 + MightBonus;
                    HitChance = 40 + HitBonus;
                    CritBonus = 0 + ExtraCrit;

                    if(SpellLevel == 3) //Thunder 3 has 4 max range
                    {
                        MaxRange = 4;
                    }
                    else
                    {
                        MaxRange = 3;
                    }

                    break;
                }
            case "Bliz": //Blizzard
                {
                    WeaponName = "Blizzard ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    Might = 1 + MightBonus;
                    HitChance = 70 + HitBonus;
                    CritBonus = 10 + ExtraCrit;

                    break;
                }
            case "Holy":
                {
                    WeaponName = "Holy ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    Might = 0 + MightBonus;
                    HitChance = 80 + HitBonus;
                    CritBonus = 0 + ExtraCrit;

                    HealthRecoil = -2 * SpellLevel;

                    WeaponDescription = "After combat, restores " + -HealthRecoil + " HP to the caster and allies within 2 spaces if unit attacked.";

                    break;
                }
        }
    }
}
