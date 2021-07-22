using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [HideInInspector] public Unit Owner;

    public TextMeshPro HealthValueText;

    public TextMeshPro BarrierValueText;

    public GameObject BarrierPanel; //panel that appears when the unit has a barrier
    public Transform FillSprite; // the "full HP" sprite that changes in length based on HP value

    public SpriteRenderer[] Orbs;
    public Sprite[] FullAndEmptyOrbSprites;

    public void UpdateHealthBar()
    {
        float HealthPercentage = 0.05f + (((float) Owner.CurrentHealth / Owner.MaxHealth) - 0.05f); //base width is 5% so that it's seen even at low health values

        FillSprite.localScale = new Vector3(HealthPercentage, 1.1f, 1f);

        HealthValueText.SetText("{0}", Owner.CurrentHealth);

        //positive barrier values make the barrier HP object show.
        if (Owner.BarrierHealth > 0)
        {
            BarrierPanel.SetActive(true);
            BarrierValueText.SetText("{0}", Owner.BarrierHealth);
        }
        else
        {
            BarrierPanel.SetActive(false);
        }

        if (Owner.UsesMagic)
        {
            UpdateOrbCounter();
        }
    }

    void UpdateOrbCounter()
    {
        int RemainingOrbs = Owner.OrbCount;

        //fill an amount of orbs equal to the unit's current OrbCount
        for (int i = 0; i < Orbs.Length; i++)
        {
            if(RemainingOrbs > 0)
            {
                Orbs[i].sprite = FullAndEmptyOrbSprites[0]; //full sprite
                RemainingOrbs--;
            }
            else
            {
                Orbs[i].sprite = FullAndEmptyOrbSprites[1]; //empty sprite
            }
        }
    }
}
