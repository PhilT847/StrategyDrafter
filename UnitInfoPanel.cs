using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI MaxHPText;
    public TextMeshProUGUI CurrentHPText;

    public TextMeshProUGUI AtkText;
    public TextMeshProUGUI SpdText;
    public TextMeshProUGUI TecText;
    public TextMeshProUGUI DefText;
    public TextMeshProUGUI ResText;

    public Image HeldWeaponImage;
    public Image SecondaryWeaponImage;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetInfoPanel(Unit ViewedUnit)
    {
        NameText.SetText(ViewedUnit.Name);

        MaxHPText.SetText("/{0}", ViewedUnit.MaxHealth);

        CurrentHPText.SetText("{0}", ViewedUnit.CurrentHealth);
        AtkText.SetText("{0}", ViewedUnit.Attack);
        SpdText.SetText("{0}", ViewedUnit.Speed);
        TecText.SetText("{0}", ViewedUnit.Technique);
        DefText.SetText("{0}", ViewedUnit.Defense);
        ResText.SetText("{0}", ViewedUnit.Resistance);

        if(ViewedUnit.MaxHealth < 16) //16-24 is normal for HP
        {
            MaxHPText.color = Color.red;
        }
        else if (ViewedUnit.MaxHealth > 24)
        {
            MaxHPText.color = Color.green;
        }
        else
        {
            MaxHPText.color = Color.white;
        }

        if (ViewedUnit.Attack < 3) //3-7 is normal for Atk/Spd/Tec
        {
            AtkText.color = Color.red;
        }
        else if (ViewedUnit.Attack > 7)
        {
            AtkText.color = Color.green;
        }
        else
        {
            AtkText.color = Color.white;
        }

        if (ViewedUnit.Speed < 3)
        {
            SpdText.color = Color.red;
        }
        else if (ViewedUnit.Speed > 7)
        {
            SpdText.color = Color.green;
        }
        else
        {
            SpdText.color = Color.white;
        }

        if (ViewedUnit.Technique < 3)
        {
            TecText.color = Color.red;
        }
        else if (ViewedUnit.Technique > 7)
        {
            TecText.color = Color.green;
        }
        else
        {
            TecText.color = Color.white;
        }

        if (ViewedUnit.Defense < 1) //1-4 is normal for Defense
        {
            DefText.color = Color.red;
        }
        else if (ViewedUnit.Defense > 4)
        {
            DefText.color = Color.green;
        }
        else
        {
            DefText.color = Color.white;
        }

        if (ViewedUnit.Resistance < 1) //1-4 is normal for Resistance
        {
            ResText.color = Color.red;
        }
        else if (ViewedUnit.Resistance > 4)
        {
            ResText.color = Color.green;
        }
        else
        {
            ResText.color = Color.white;
        }

        HeldWeaponImage.sprite = ViewedUnit.HeldWeapon.DisplaySprite;
        SecondaryWeaponImage.sprite = ViewedUnit.SecondaryWeapon.DisplaySprite;
    }

}
