using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Customizer : MonoBehaviour
{
    public Unit CustomizedUnit;

    public Color[] HairColors, SkinTones, PrimaryOutfitColors, MountColors;

    public SpriteRenderer[] SkinSprites, HairSprites, UnitFace, UnitHead, PrimaryOutfitSprites, SecondaryOutfitSprites, MountSprites;

    public List<Weapon> PotentialWeapons; //a list of potential weapons for this unit.

    public Sprite[] FrontHairSprites, BackHairSprites, FaceSprites, HeadSprites;

    //Each of these integers is a "Base Stat" for the class that is modified and applied to the unit.
    public int MaxHP, Attack, Speed, Technique, Defense, Resistance;

    public void BuildUnit()
    {
        //Sets any color that ISN'T HAIR, as hair is altered by the unit's boon stat.
        SetColors();

        SetStats();

        SetWeapons();

        CustomizedUnit.FacingRight = true;

        CustomizedUnit.VisualEffects = CustomizedUnit.GetComponentInChildren<UnitEffects>();

        Destroy(this);
    }

    void SetColors()
    {
        //Hair Style... change sprite of both front and back. Note that indices 0-7 are shorter/"masculine".
        int ChosenHairStyle = Random.Range(0, FrontHairSprites.Length);

        HairSprites[0].sprite = FrontHairSprites[ChosenHairStyle];
        HairSprites[1].sprite = BackHairSprites[ChosenHairStyle];

        if (ChosenHairStyle < 8) //big jawline head looks weird on long hair. Do not allow it (the last head) on longer styles. Also restricts the angry jaw face
        {
            UnitHead[0].sprite = HeadSprites[Random.Range(0, HeadSprites.Length)];

            UnitFace[0].sprite = FaceSprites[Random.Range(0, FaceSprites.Length)];
        }
        else
        {
            UnitHead[0].sprite = HeadSprites[Random.Range(0, HeadSprites.Length - 3)];

            UnitFace[0].sprite = FaceSprites[Random.Range(0, FaceSprites.Length - 1)];
        }

        //Hair Color. Note that the Customizer changes hair color when it creates traits for drafted units.
        int ChosenHairColor = Random.Range(0, HairColors.Length);

        for (int i = 0; i < HairSprites.Length; i++)
        {
            HairSprites[i].color = HairColors[ChosenHairColor];
        }

        //Skin
        int ChosenSkinColor = Random.Range(0, SkinTones.Length);

        for (int i = 0; i < SkinSprites.Length; i++)
        {
            SkinSprites[i].color = SkinTones[ChosenSkinColor];
        }

        //PrimaryOutfit
        int ChosenPrimaryColor = Random.Range(0, PrimaryOutfitColors.Length);

        for (int i = 0; i < PrimaryOutfitSprites.Length; i++)
        {
            PrimaryOutfitSprites[i].color = PrimaryOutfitColors[ChosenPrimaryColor];
        }

        //Pick secondary colors based on the unit's team leader
        for (int i = 0; i < SecondaryOutfitSprites.Length; i++)
        {
            if (CustomizedUnit.Leader != null)
            {
                SecondaryOutfitSprites[i].color = CustomizedUnit.Leader.TeamColor;
            }
            else //no leader currently; make it all gray
            {
                SecondaryOutfitSprites[i].color = Color.gray; //set the health bar's fill color the the leader's color.
            }
        }

        //Mount (for cavalry/pegasus units)
        int ChosenMountColor = Random.Range(0, MountColors.Length);

        for (int i = 0; i < MountSprites.Length; i++)
        {
            MountSprites[i].color = MountColors[ChosenMountColor];
        }
    }

    void SetStats()
    {
        int StatVariation = 0; //measures how much stats changed overall
        string BoostedStat = "";

        //Randomly increase a single stat. This boosted stat is also unable to be decreased by random variation.
        int RandomBoost = Random.Range(1, 9);

        switch (RandomBoost)
        {
            case 1: //HP +3 ... Pink hair
                {
                    MaxHP += 3;
                    BoostedStat = "MaxHP";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[0];
                    }

                    break;
                }
            case 2: //ATK +2 ... Red hair
                {
                    Attack += 2;
                    BoostedStat = "Attack";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[1];
                    }

                    break;
                }
            case 3: //SPD +2 ... Green hair
                {
                    Speed += 2;
                    BoostedStat = "Speed";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[2];
                    }

                    break;
                }
            case 4: //TEC +2 ... Blue hair
                {
                    Technique += 2;
                    BoostedStat = "Technique";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[3];
                    }

                    break;
                }
            case 5: //DEF +1 ... Brown hair
                {
                    Defense += 1;
                    BoostedStat = "Defense";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[4];
                    }

                    break;
                }
            case 6: //RES +3 ... Dark purple hair
                {
                    Resistance += 3;
                    BoostedStat = "Resistance";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[5];
                    }

                    break;
                }
            case 7: //ATK/SPD +1 ... Dark gray hair
                {
                    Attack += 1;
                    Speed += 1;
                    BoostedStat = "Attack";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[6];
                    }

                    break;
                }
            case 8: //HP/RES +1 ... Off-white hair
                {
                    MaxHP += 1;
                    Resistance += 1;
                    BoostedStat = "MaxHP";

                    for (int i = 0; i < HairSprites.Length; i++)
                    {
                        HairSprites[i].color = HairColors[7];
                    }

                    break;
                }
        }

        //Add some variability to each stat as well.

        //Health varies by +- 3.
        int Vary = Random.Range(-3, 4);

        if (BoostedStat == "MaxHP" && Vary < 0)
        {
            Vary *= -1;
        }

        StatVariation += Vary;
        MaxHP += Vary;

        //Attack, Speed, and Technique vary by +- 2.
        Vary = Random.Range(-2, 3);

        if (BoostedStat == "Attack" && Vary < 0)
        {
            Vary *= -1;
        }

        StatVariation += Vary;
        Attack += Vary;

        Vary = Random.Range(-2, 3);

        if (BoostedStat == "Speed" && Vary < 0)
        {
            Vary *= -1;
        }

        StatVariation += Vary;
        Speed += Vary;

        Vary = Random.Range(-2, 3);

        if (BoostedStat == "Technique" && Vary < 0)
        {
            Vary *= -1;
        }

        //Defense and Resistance vary by +- 1.

        StatVariation += Vary;
        Technique += Vary;

        Vary = Random.Range(-1, 2);

        if (BoostedStat == "Defense" && Vary < 0)
        {
            Vary *= -1;
        }

        StatVariation += Vary;
        Defense += Vary;

        Vary = Random.Range(-1, 2);

        if (BoostedStat == "Resistance" && Vary < 0)
        {
            Vary *= -1;
        }

        StatVariation += Vary;
        Resistance += Vary;

        //Units can vary from -11 to +11 base points. Adjust point values to bring them closer to "average" values
        if (StatVariation < -5) //increase the stats of underpowered units by +5
        {
            Attack += 1;
            Speed += 1;
            Technique += 1;
            Defense += 1;
            Resistance += 1;
        }
        else if (StatVariation > 5) //decrease the stats of overpowered units by -5
        {
            Attack -= 1;
            Speed -= 1;
            Technique -= 1;
            Defense -= 1;
            Resistance -= 1;
        }

        //Keep stats from capping up or down.
        if(Resistance > 10)
        {
            Resistance = 10;
        }
        else if (Resistance < 1)
        {
            Resistance = 0;
        }

        if (Defense > 10)
        {
            Defense = 10;
        }
        else if(Defense < 1)
        {
            Defense = 0;
        }

        if (Speed > 10)
        {
            Speed = 10;
        }
        else if(Speed < 1)
        {
            Speed = 0;
        }

        if(Attack > 10)
        {
            Attack = 10;
        }
        else if (Attack < 1)
        {
            Attack = 0;
        }

        if (Technique > 10)
        {
            Technique = 10;
        }
        else if (Technique < 1)
        {
            Technique = 0;
        }

        CustomizedUnit.MaxHealth = MaxHP;
        CustomizedUnit.CurrentHealth = CustomizedUnit.MaxHealth; //set current HP
        CustomizedUnit.OrbCount = 3; //set orbs to full

        //sets the unit's health bar
        CustomizedUnit.UnitHealthBar = CustomizedUnit.GetComponentInChildren<HealthBar>(true); //"true" as it has to find the disabled HealtHBar in the draft menu
        CustomizedUnit.UnitHealthBar.Owner = CustomizedUnit;

        if (CustomizedUnit.Leader != null)
        {
            CustomizedUnit.UnitHealthBar.FillSprite.GetComponent<SpriteRenderer>().color = CustomizedUnit.Leader.TeamColor; //set the health bar's fill color the the leader's color.
            CustomizedUnit.UnitHealthBar.HealthValueText.color = CustomizedUnit.Leader.TeamColor; //set the health bar's number color the the leader's color.
        }
        else //no leader currently; make it all gray
        {
            CustomizedUnit.UnitHealthBar.FillSprite.GetComponent<SpriteRenderer>().color = Color.gray; //set the health bar's fill color the the leader's color.
        }

        CustomizedUnit.UnitHealthBar.UpdateHealthBar(); //ensure health shows 100%

        CustomizedUnit.Attack = Attack;
        CustomizedUnit.Speed = Speed;
        CustomizedUnit.Technique = Technique;
        CustomizedUnit.Defense = Defense;
        CustomizedUnit.Resistance = Resistance;

        //The unit's "Rating" is each stat added up.
        CustomizedUnit.Rating = MaxHP + Attack + Speed + Technique + Defense + Resistance;
    }

    void SetWeapons() //set a unit's weapon and equip the held weapon.
    {
        int FirstWeaponIndex = Random.Range(0, PotentialWeapons.Count);

        //Sets the first weapon's object, sprite and sorting order.
        Weapon FirstWeapon = Instantiate(PotentialWeapons[FirstWeaponIndex], CustomizedUnit.WeaponHand.position, CustomizedUnit.WeaponHand.rotation, CustomizedUnit.WeaponHand);
        FirstWeapon.WeaponSprite = FirstWeapon.GetComponent<SpriteRenderer>();
        FirstWeapon.WeaponSprite.sortingLayerName = "Unit";
        FirstWeapon.WeaponSprite.sortingOrder = CustomizedUnit.WeaponHand.GetComponent<SpriteRenderer>().sortingOrder;
        CustomizedUnit.HeldWeapon = FirstWeapon;

        //CustomizedUnit.HeldWeapon = PotentialWeapons[FirstWeaponIndex];

        PotentialWeapons.RemoveAt(FirstWeaponIndex); //remove the first weapon so that the second is unique.

        Weapon SecondWeapon = Instantiate(PotentialWeapons[Random.Range(0, PotentialWeapons.Count)], CustomizedUnit.WeaponHand.position, CustomizedUnit.WeaponHand.rotation, CustomizedUnit.WeaponHand);
        SecondWeapon.WeaponSprite = SecondWeapon.GetComponent<SpriteRenderer>();
        SecondWeapon.WeaponSprite.sortingLayerName = "Unit";
        SecondWeapon.WeaponSprite.sortingOrder = CustomizedUnit.WeaponHand.GetComponent<SpriteRenderer>().sortingOrder;
        CustomizedUnit.SecondaryWeapon = SecondWeapon;

        //CustomizedUnit.SecondaryWeapon = PotentialWeapons[Random.Range(0, PotentialWeapons.Count)];

        CustomizedUnit.EquipWeapon(CustomizedUnit.HeldWeapon);

        //Determine whether this unit uses magic. "true" if either of their weapons are magic or staves.
        CustomizedUnit.UsesMagic = (CustomizedUnit.HeldWeapon.GetComponent<Staff>() != null || CustomizedUnit.HeldWeapon.GetComponent<Magic>() != null || CustomizedUnit.SecondaryWeapon.GetComponent<Staff>() != null || CustomizedUnit.SecondaryWeapon.GetComponent<Magic>() != null);

        //Units without magic don't need an orb counter; deactivate it from their health bar.
        if (!CustomizedUnit.UsesMagic)
        {
            CustomizedUnit.UnitHealthBar.Orbs[0].transform.parent.gameObject.SetActive(false);
        }
        else //ensure that spells are shown at "3" level in the Draft so they're advertised well. Also, remove the numerals at the end of the name.
        {
            if (CustomizedUnit.HeldWeapon.GetComponent<Magic>())
            {
                CustomizedUnit.HeldWeapon.GetComponent<Magic>().ChangeSpellLevel(3);
                CustomizedUnit.HeldWeapon.GetComponent<Magic>().WeaponName = CustomizedUnit.HeldWeapon.GetComponent<Magic>().WeaponName.Substring(0, CustomizedUnit.HeldWeapon.GetComponent<Magic>().WeaponName.IndexOf(" "));
            }
            else if(CustomizedUnit.HeldWeapon.GetComponent<Staff>())
            {
                CustomizedUnit.HeldWeapon.GetComponent<Staff>().ChangeSpellLevel(3);
                CustomizedUnit.HeldWeapon.GetComponent<Staff>().WeaponName = CustomizedUnit.HeldWeapon.GetComponent<Staff>().WeaponName.Substring(0, CustomizedUnit.HeldWeapon.GetComponent<Staff>().WeaponName.IndexOf(" "));
            }

            if (CustomizedUnit.SecondaryWeapon.GetComponent<Magic>())
            {
                CustomizedUnit.SecondaryWeapon.GetComponent<Magic>().ChangeSpellLevel(3);
                CustomizedUnit.SecondaryWeapon.GetComponent<Magic>().WeaponName = CustomizedUnit.SecondaryWeapon.GetComponent<Magic>().WeaponName.Substring(0, CustomizedUnit.SecondaryWeapon.GetComponent<Magic>().WeaponName.IndexOf(" "));
            }
            else if (CustomizedUnit.SecondaryWeapon.GetComponent<Staff>())
            {
                CustomizedUnit.SecondaryWeapon.GetComponent<Staff>().ChangeSpellLevel(3);
                CustomizedUnit.SecondaryWeapon.GetComponent<Staff>().WeaponName = CustomizedUnit.SecondaryWeapon.GetComponent<Staff>().WeaponName.Substring(0, CustomizedUnit.SecondaryWeapon.GetComponent<Staff>().WeaponName.IndexOf(" "));
            }
        }
    }

    /*
    //Creates a copy of the weapon for use by the Unit. Ensures that all methods affecting spell levels don't affect each copy of that spell on the map.
    Magic CopySpellObject(Weapon CopiedSpell)
    {
        Magic CopiedSpell = null;

        return CopiedSpell;
    }
    */
}
