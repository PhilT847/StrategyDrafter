public class Staff : Weapon
{
    public int SpellLevel;

    //0 (Cure), 1 (Again), 2 (Barrier)
    public int StaffID;

    public override int GetMaxRange()
    {
        return MaxRange;
    }

    //Changes a spell's level, altering its power and attributes.
    public void ChangeSpellLevel(int LevelChange)
    {
        //Certain methods increase/decrease spell level. Allow the spell level to cycle from 3 all the way back to 1 and vice versa
        if(LevelChange > 3)
        {
            LevelChange = 1;
        }
        else if(LevelChange < 1)
        {
            LevelChange = 3;
        }

        //if the caster's orb count isn't high enough to cast a spell at this level, don't allow them to select this level.
        if (WeaponOwner.OrbCount < LevelChange)
        {
            return;
        }

        SpellLevel = LevelChange;

        //uses the name substring as the number at the end has changed the WeaponName
        switch (WeaponName.Substring(0, 4))
        {
            case "Cure":
                {
                    WeaponName = "Cure ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    string CureDescription = "Restores an ally's HP by ";

                    switch (SpellLevel)
                    {
                        case 1:
                            {
                                CureDescription += "[Attack].";
                                MaxRange = 1;
                                break;
                            }
                        case 2:
                            {
                                CureDescription += "[Attack].";
                                MaxRange = 2;
                                break;
                            }
                        case 3:
                            {
                                CureDescription += "[Attack x 2].";
                                MaxRange = 2;
                                break;
                            }
                    }

                    WeaponDescription = CureDescription;

                    break;
                }
            case "Barr": //Barrier
                {
                    WeaponName = "Barrier ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    string BarrierDescription = "Grants temporary HP equal to ";

                    switch (SpellLevel)
                    {
                        case 1:
                            {
                                MaxRange = 1;
                                BarrierDescription += "[Attack - 2].";
                                break;
                            }
                        case 2:
                            {
                                MaxRange = 2;
                                BarrierDescription += "[Attack].";
                                break;
                            }
                        case 3:
                            {
                                MaxRange = 3;
                                BarrierDescription += "[Attack + 2].";
                                break;
                            }
                    }

                    WeaponDescription = BarrierDescription;

                    break;
                }
            case "Refr": //Refresh
                {
                    WeaponName = "Refresh ";
                    for (int i = 0; i < SpellLevel; i++)
                    {
                        WeaponName += "I";
                    }

                    //Refresh costs 12 -> 8 -> 4 HP to cast.
                    HealthRecoil = 12 - SpellLevel * 4;

                    string RefreshDescription = "Allow an ally who has already acted to move again.";

                    //recoil and non-Refresh restriction removed by level 3.
                    if (SpellLevel < 3)
                    {
                        RefreshDescription += " Cannot be used on allies with Refresh. Costs " + HealthRecoil + " HP to cast.";
                    }
                    else
                    {
                        WeaponDescription = "";
                    }

                    WeaponDescription = RefreshDescription;

                    break;
                }
        }
    }
}
