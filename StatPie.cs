using UnityEngine;

public class StatPie : MonoBehaviour
{
    public Transform[] PieSlices; //the "slices" that make up each stat in the pie.

    public void SetStatPie(Unit ViewedUnit)
    {
        //Max HP isn't 1-10, so make sure it scales differently. Lowest possible is 12... 12-31 is max range (19 variance)
        PieSlices[0].localScale = new Vector3(0.2f + ((ViewedUnit.MaxHealth - 12) * 0.042f), 0.2f + ((ViewedUnit.MaxHealth - 12) * 0.042f), 0f);

        PieSlices[1].localScale = new Vector3(0.2f + (ViewedUnit.Attack * .08f), 0.2f + (ViewedUnit.Attack * .08f), 0f);

        PieSlices[2].localScale = new Vector3(0.2f + (ViewedUnit.Speed * .08f), 0.2f + (ViewedUnit.Speed * .08f), 0f);

        PieSlices[3].localScale = new Vector3(0.2f + (ViewedUnit.Technique * .08f), 0.2f + (ViewedUnit.Technique * .08f), 0f);

        //Def/Res maximums treated as 7; however, note that the "Capped" 7 only shows up to 
        PieSlices[4].localScale = new Vector3(0.25f + (ViewedUnit.Resistance * .107f), 0.25f + (ViewedUnit.Resistance * .107f), 0f);

        PieSlices[5].localScale = new Vector3(0.25f + (ViewedUnit.Defense * .107f), 0.25f + (ViewedUnit.Defense * .107f), 0f);
    }
}
