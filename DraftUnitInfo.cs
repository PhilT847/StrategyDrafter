using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DraftUnitInfo : MonoBehaviour
{
    public TextMeshProUGUI UnitName;
    public TextMeshProUGUI UnitClass;

    public TextMeshProUGUI UnitRating;
    public TextMeshProUGUI DefensiveStats; //HP/DEF/RES
    public TextMeshProUGUI OffensiveStats; //ATK/SPD/TEC

    public Image Weapon1_Image;
    public TextMeshProUGUI Weapon1_Name;
    public TextMeshProUGUI Weapon1_Range;
    public TextMeshProUGUI Weapon1_Atk;
    public TextMeshProUGUI Weapon1_Hit;
    public TextMeshProUGUI Weapon1_Crit;
    public TextMeshProUGUI Weapon1_Info;

    public Image Weapon2_Image;
    public TextMeshProUGUI Weapon2_Name;
    public TextMeshProUGUI Weapon2_Range;
    public TextMeshProUGUI Weapon2_Atk;
    public TextMeshProUGUI Weapon2_Hit;
    public TextMeshProUGUI Weapon2_Crit;
    public TextMeshProUGUI Weapon2_Info;

    public void SetUnitInfo(Unit Hero)
    {
        UnitName.SetText(Hero.Name);
        UnitClass.SetText(Hero.Name);

        UnitRating.SetText("Rating: {0}", Hero.Rating);
        DefensiveStats.SetText("{0}\n{1}\n{2}", Hero.MaxHealth, Hero.Defense, Hero.Resistance);
        OffensiveStats.SetText("{0}\n{1}\n{2}", Hero.Attack, Hero.Speed, Hero.Technique);

        Weapon1_Image.sprite = Hero.HeldWeapon.DisplaySprite;
        Weapon1_Name.SetText(Hero.HeldWeapon.WeaponName);

        if (Hero.HeldWeapon.MinRange == Hero.HeldWeapon.GetMaxRange())
        {
            Weapon1_Range.SetText("{0}", Hero.HeldWeapon.GetMaxRange());
        }
        else
        {
            Weapon1_Range.SetText("{0}-{1}", Hero.HeldWeapon.MinRange, Hero.HeldWeapon.GetMaxRange());
        }

        if (!Hero.HeldWeapon.TargetsAllies)
        {
            if (Hero.HeldWeapon.Might > -1)
            {
                Weapon1_Atk.SetText("+{0}", Hero.HeldWeapon.Might);
            }
            else
            {
                Weapon1_Atk.SetText("{0}", Hero.HeldWeapon.Might);
            }

            Weapon1_Hit.SetText("{0}", Hero.HeldWeapon.HitChance);
            Weapon1_Crit.SetText("{0}", Hero.HeldWeapon.CritBonus);
        }
        else //support abilities don't show Atk/Hit/Crit
        {
            Weapon1_Atk.SetText("-");
            Weapon1_Hit.SetText("-");
            Weapon1_Crit.SetText("-");
        }

        Weapon1_Info.SetText(Hero.HeldWeapon.WeaponDescription);

        Weapon2_Image.sprite = Hero.SecondaryWeapon.DisplaySprite;
        Weapon2_Name.SetText(Hero.SecondaryWeapon.WeaponName);

        if (Hero.SecondaryWeapon.MinRange == Hero.SecondaryWeapon.GetMaxRange())
        {
            Weapon2_Range.SetText("{0}", Hero.SecondaryWeapon.GetMaxRange());
        }
        else
        {
            Weapon2_Range.SetText("{0}-{1}", Hero.SecondaryWeapon.MinRange, Hero.SecondaryWeapon.GetMaxRange());
        }

        if (!Hero.SecondaryWeapon.TargetsAllies)
        {
            if (Hero.SecondaryWeapon.Might > -1)
            {
                Weapon2_Atk.SetText("+{0}", Hero.SecondaryWeapon.Might);
            }
            else
            {
                Weapon2_Atk.SetText("{0}", Hero.SecondaryWeapon.Might);
            }

            Weapon2_Hit.SetText("{0}", Hero.SecondaryWeapon.HitChance);
            Weapon2_Crit.SetText("{0}", Hero.SecondaryWeapon.CritBonus);
        }
        else //support abilities don't show Atk/Hit/Crit
        {
            Weapon2_Atk.SetText("-");
            Weapon2_Hit.SetText("-");
            Weapon2_Crit.SetText("-");
        }

        Weapon2_Info.SetText(Hero.SecondaryWeapon.WeaponDescription);
    }
}
