using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponDesc : MonoBehaviour
{
    public TextMeshProUGUI WeaponName;
    public Image WeaponImage;

    public TextMeshProUGUI MightText;
    public TextMeshProUGUI RangeText;
    public TextMeshProUGUI HitText;
    public TextMeshProUGUI CritText;

    public TextMeshProUGUI WeaponDescText;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetWeaponDescription(Weapon NewWeapon)
    {
        WeaponName.SetText(NewWeapon.WeaponName);
        WeaponImage.sprite = NewWeapon.DisplaySprite;

        if(NewWeapon.Might > -1)
        {
            MightText.SetText("+" + "{0}", NewWeapon.Might);
        }
        else //if zero or negative, remove the "+"... keep in mind that negatives have a "-" automatically.
        {
            MightText.SetText("{0}", NewWeapon.Might);
        }

        if (NewWeapon.MinRange == NewWeapon.GetMaxRange()) //range descriptor may only account for one range
        {
            RangeText.SetText("{0}", NewWeapon.MinRange);
        }
        else
        {
            RangeText.SetText("{0}-{1}", NewWeapon.MinRange, NewWeapon.GetMaxRange());
        }

        HitText.SetText("{0}", NewWeapon.HitChance);
        CritText.SetText("{0}", NewWeapon.CritBonus);

        WeaponDescText.SetText(NewWeapon.WeaponDescription);
    }
}
