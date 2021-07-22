using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftRoster : MonoBehaviour
{
    public Player RosterOwner;

    public Image MainPanel;

    public void SetOwner(Player NewPlayer)
    {
        RosterOwner = NewPlayer;

        MainPanel.color = RosterOwner.TeamColor;
    }

    public void AddNewUnit(Unit NewUnit)
    {
        StartCoroutine(MoveNewUnit(RosterOwner.UnitsOnTeam[RosterOwner.UnitsOnTeam.Count - 1]));
    }

    //Moves a just-introduced unit into the roster.
    public IEnumerator MoveNewUnit(Unit SelectedUnit)
    {
        Vector3 NewPosition = new Vector3(-7f + ((RosterOwner.UnitsOnTeam.Count - 1) * 2.85f), 6.3f - (12.6f * (RosterOwner.PlayerOrder - 1)), 0f);

        float TimeElapsed = 0f;

        while (TimeElapsed < 1f) //((transform.position - EndTile.transform.position).sqrMagnitude > 0.005)
        {
            SelectedUnit.transform.position = Vector3.Lerp(SelectedUnit.transform.position, NewPosition, TimeElapsed / 1f);

            TimeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        SelectedUnit.transform.position = NewPosition;

        yield return null;
    }
}
