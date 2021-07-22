using UnityEngine;
using TMPro;

public class DamageStars : MonoBehaviour
{
    private Animator StarAnimator;
    private TextMeshProUGUI DamageAmountText;

    private void Start()
    {
        StarAnimator = GetComponentInChildren<Animator>();
        DamageAmountText = GetComponentInChildren<TextMeshProUGUI>();
    }

    //animate the main star and emit an amount of little stars equal to damage dealt.
    public void ActivateDamageStars(Unit ThisUnit, int damage, bool Crit)
    {
        //move the stars to this unit
        transform.position = ThisUnit.transform.position;

        DamageAmountText.SetText("{0}", damage);

        if (!Crit)
        {
            StarAnimator.SetTrigger("ActivateStars");
        }
        else
        {
            StarAnimator.SetTrigger("ActivateCritical");
        }
    }

    public void ActivateHealingStar(Unit ThisUnit, int healing)
    {
        //move the stars to this unit
        transform.position = ThisUnit.transform.position;

        DamageAmountText.SetText("{0}", healing);

        StarAnimator.SetTrigger("ActivateHealing");
    }

    public void ActivateMissVisual(Unit ThisUnit)
    {
        transform.position = ThisUnit.transform.position;

        StarAnimator.SetTrigger("ActivateMiss");
    }
}
