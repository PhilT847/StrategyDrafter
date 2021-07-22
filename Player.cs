using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string PlayerLayer; //Player1, Player2, Player3, Player4... controls what the camera sees

    public Color TeamColor;
    public int PlayerOrder; //1-4

    public List<Unit> UnitsOnTeam; //units in the player's roster. Note that these aren't the SPAWNED units, but copies that are spawned in during the match.

    public List<Unit> SpawnedUnits;

    public ControlledUI PlayerUI;
    public Camera PlayerCamera;
    [HideInInspector] public Vector3 PreviousCamPosition; //"Previous" position/scale used when exiting combat cutscenes
    [HideInInspector] public float PreviousCamScale;

    public GameObject PlayerMapObjectPrefab; //contains camera and UI elements for players once they enter the map

    private void Start()
    {
        //if not already DontDestroyOnLoad, make it so
        if (gameObject.scene.buildIndex != -1)
        {
            DontDestroyOnLoad(this.gameObject);

            PrepareUnitForMap();
        }
    }

    public List<Player> GetEnemyPlayers() //Gets the TeamNumber of the enemy team.
    {
        List<Player> EnemyPlayers = new List<Player>();

        foreach(Player Leader in FindObjectOfType<GameController>().PlayerList)
        {
            if(Leader != FindObjectOfType<GameController>().ActivePlayer)
            {
                EnemyPlayers.Add(Leader);
            }
        }

        return EnemyPlayers;
    }

    void PrepareUnitForMap() //creates the camera and UI elements for players once they enter the map.
    {
        var NewPlayerControls = Instantiate(PlayerMapObjectPrefab, Vector2.zero, Quaternion.identity, transform);

        PlayerUI = GetComponentInChildren<ControlledUI>(true);
        PlayerCamera = GetComponentInChildren<Camera>(true);

        PlayerUI.SetMenuColors(this);

        NewPlayerControls.gameObject.SetActive(false);
    }

    public void SetLayers() //sets the tags for this player's camera. Ensures that the other players can't see the unit's highlighted tiles, cursor, options, etc...
    {
        PlayerLayer = "Player" + (PlayerOrder); //sets layer as 1-4

        ChangeLayersOfChildren(PlayerUI.gameObject);
        ChangeLayersOfChildren(PlayerCamera.gameObject);
        ChangeLayersOfChildren(GetComponentInChildren<Cursor>().gameObject);

        //Set the camera to view player's UI, Cursor, etc.
        PlayerCamera.cullingMask = PlayerCamera.cullingMask | (1 << LayerMask.NameToLayer(PlayerLayer));
    }

    void ChangeLayersOfChildren(GameObject ChangedLayerObject)
    {
        foreach (Transform ChildObject in ChangedLayerObject.transform.GetComponentsInChildren<Transform>(true))
        {
            ChildObject.gameObject.layer = LayerMask.NameToLayer(PlayerLayer);
        }
    }

    public bool PlayerTurnIsOver()
    {
        foreach(Unit PlayerUnit in UnitsOnTeam)
        {
            if (PlayerUnit.CanAct && PlayerUnit.CurrentHealth > 0) //if they have a unit that is both alive and can act, keep their turn going
            {
                return false;
            }
        }

        //if they're out of acting units, switch to the next turn.
        return true;
    }
}
