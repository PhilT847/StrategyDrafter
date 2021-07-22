using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public Tile[,] TileMap;

    public GameObject TilePrefab;

    public int MapSize; //Maps are either 6x6 for a 3-unit Skirmish map or 10x10 for a 6-unit Control map.

    public List<Tile> HighlightedTiles;

    private GameController Controls;

    private MapBuilder Builder;

    private void Awake()
    {
        Controls = FindObjectOfType<GameController>();
        Builder = GetComponentInParent<MapBuilder>();

        GenerateMap(MapSize);
    }

    void DestroyCurrentMap()
    {
        for (int x = 0; x < MapSize; x++)
        {
            for (int y = 0; y < MapSize; y++)
            {
                Destroy(TileMap[x, y].gameObject);
                TileMap[x, y] = null;
            }
        }
    }

    void GenerateMap(int SelectedMapSize)
    {
        //Generate a map of grass tiles that can find their neighboring tiles.
        TileMap = new Tile[SelectedMapSize, SelectedMapSize];

        for (int x = 0; x < SelectedMapSize; x++)
        {
            for (int y = 0; y < SelectedMapSize; y++)
            {
                TileMap[x, y] = GenerateTileAt(x, y);
            }
        }

        //Find the neighboring tiles. This loop has to be done after the tile objects are already generated.
        for (int x = 0; x < SelectedMapSize; x++)
        {
            for (int y = 0; y < SelectedMapSize; y++)
            {
                AssignNeighboringTiles(TileMap[x, y]);
            }
        }

        //Now that the tiles are generated, add fortresses as well as mountains/forests as appropriate.
    }

    Tile GenerateTileAt(int x, int y)
    {
        GameObject NewTile = Instantiate(TilePrefab, new Vector2(x * 2, y * 2), Quaternion.identity, transform);

        int Randomizer = Random.Range(0, 10);

        if(Randomizer < 8) //0-7: normal tile
        {
            NewTile.GetComponentInChildren<SpriteRenderer>().sprite = Builder.GrassTiles[Random.Range(0, Builder.GrassTiles.Length)];
        }
        else if(Randomizer < 9) //8; forest tile
        {
            NewTile.GetComponent<Tile>().MovementCost = 2;
            NewTile.GetComponentInChildren<SpriteRenderer>().sprite = Builder.ForestTiles[0];
        }
        else //9; mountain tiles
        {
            NewTile.GetComponent<Tile>().MovementCost = 3;
            NewTile.GetComponentInChildren<SpriteRenderer>().sprite = Builder.MountainTiles[0];
        }

        NewTile.GetComponent<Tile>().X_Position = x;
        NewTile.GetComponent<Tile>().Y_Position = y;

        NewTile.name = "Tile [" + x +"," + y +"]";
        return NewTile.GetComponent<Tile>();
    }

    public void AssignNeighboringTiles(Tile GeneratedTile) //fills the NeighboringTiles list for each tile.
    {
        if(GeneratedTile.X_Position > 0) //if not on the leftmost side, find a neighbor on the left.
        {
            GeneratedTile.NeighboringTiles.Add(TileMap[GeneratedTile.X_Position - 1, GeneratedTile.Y_Position]);
        }

        if (GeneratedTile.X_Position < MapSize - 1) //if not on the rightmost side, find a neighbor on the right.
        {
            GeneratedTile.NeighboringTiles.Add(TileMap[GeneratedTile.X_Position + 1, GeneratedTile.Y_Position]);
        }

        if (GeneratedTile.Y_Position > 0) //if not on the bottom, find a neighbor under the tile.
        {
            GeneratedTile.NeighboringTiles.Add(TileMap[GeneratedTile.X_Position, GeneratedTile.Y_Position - 1]);
        }

        if (GeneratedTile.Y_Position < MapSize - 1) //if not on the top, find a neighbor over the tile.
        {
            GeneratedTile.NeighboringTiles.Add(TileMap[GeneratedTile.X_Position, GeneratedTile.Y_Position + 1]);
        }
    }

    public void VisualizeMovementFrom(Tile SelectedTile)
    {
        HighlightedTiles = new List<Tile>(); //Might need to remove as you switch from unit to unit

        Unit UnitInTile = SelectedTile.OccupyingUnit;

        int UnitMaxMovement = UnitInTile.MovementDistance;

        List<Tile> MovementOptions = DeterminePassableNeighbors(UnitInTile, SelectedTile, UnitMaxMovement);

        //highlight all movement options

        foreach(Tile NewlyHighlightedTile in MovementOptions)
        {
            HighlightedTiles.Add(NewlyHighlightedTile);
        }
    }

    public void RemoveHighlightedTiles()
    {
        foreach (Tile TileForRemoval in HighlightedTiles)
        {
            TileForRemoval.Highlighted = false;
            TileForRemoval.HighlightTile(false, true);
        }

        HighlightedTiles = new List<Tile>();
    }

    public void ConvertTileToType(Tile SelectedTile, int NewType) //turns grass tiles to forests or mountains.
    {

    }

    public List<Tile> DeterminePassableNeighbors(Unit SelectedUnit, Tile OriginTile, int RemainingMoves) //checks a neighboring tile to see
    {
        List<Tile> TotalMovableTiles = new List<Tile>();

        if(OriginTile.OccupyingUnit == SelectedUnit)
        {
            TotalMovableTiles.Add(OriginTile); //add the unit's tile.
            OriginTile.HighlightTile(true, true);
            OriginTile.Highlighted = true;
        }

        foreach (Tile NeighborTile in OriginTile.NeighboringTiles)
        {
            int TotalCost = NeighborTile.MovementCost; //the actual cost of moving thru this tile; changes based on unit type.

            if(NeighborTile.OccupyingUnit != null && NeighborTile.OccupyingUnit.Leader != SelectedUnit.Leader) //occupying enemy units act as a wall
            {
                TotalCost = 99;
            }
            else if (NeighborTile.MovementCost > 1 && SelectedUnit.UnitType == "Cavalry") //cavalry units are slowed more by forests/mountains than infantry/armored units
            {
                TotalCost += 3;
            }
            else if (SelectedUnit.UnitType == "Flying") //pegasus units can fly freely over any tile
            {
                TotalCost = 1;
            }

            if (TotalCost <= RemainingMoves /*&& !NeighborTile.Highlighted*/)
            {
                if (!NeighborTile.Highlighted/* && NeighborTile.OccupyingUnit == null*/)
                {
                    TotalMovableTiles.Add(NeighborTile);

                    NeighborTile.HighlightTile(true, true);

                    NeighborTile.Highlighted = true;
                }

                //highlight this tile and check its neighbors
                //NeighborTile.Highlighted = true;
                TotalMovableTiles.AddRange(DeterminePassableNeighbors(SelectedUnit, NeighborTile, RemainingMoves - TotalCost)); //recursive... check all further neighboring tiles.
            }
        }

        //DetermineAttackableTiles(TotalMovableTiles, SelectedUnit.HeldWeapon.MinRange, SelectedUnit.HeldWeapon.MaxRange); //highlight unit's attack range.

        return TotalMovableTiles;
    }

    public int DistanceBetweenUnits(Unit Unit1, Unit Unit2) //Used to determine the range between two units. Helps with area buffs, area damage, and return attacks.
    {
        return Mathf.Abs(Unit1.CurrentTile.X_Position - Unit2.CurrentTile.X_Position) + Mathf.Abs(Unit1.CurrentTile.Y_Position - Unit2.CurrentTile.Y_Position);
    }

    public List<Unit> HighlightUnitsWithinRange(Unit AttackingUnit, Weapon ChosenWeapon) //returns a list of units that the selected unit can attack.
    {
        List<Unit> TargetableUnits = new List<Unit>();

        if (!ChosenWeapon.TargetsAllies) //If the unit has a damaging attack, search for nearby enemies.
        {
            List<Unit> PotentiallyTargetableUnits = new List<Unit>();

            for (int i = 0; i < Controls.PlayerList.Count; i++)
            {
                if (Controls.PlayerList[i] != AttackingUnit.Leader) //add all players that AREN'T on the unit's team.
                {
                    for (int add = 0; add < Controls.PlayerList[i].UnitsOnTeam.Count; add++)
                    {
                        PotentiallyTargetableUnits.Add(Controls.PlayerList[i].UnitsOnTeam[add]);
                    }
                    //PotentiallyTargetableUnits.AddRange(Controls.PlayerList[i].UnitsOnTeam);
                }
            }

            //List<Unit> PotentiallyTargetableUnits = Controls.PlayerList[AttackingUnit.Leader.GetEnemyTeam() - 1].SpawnedUnits;

            foreach (Unit DefendingUnit in PotentiallyTargetableUnits)
            {
                if (DistanceBetweenUnits(AttackingUnit, DefendingUnit) >= ChosenWeapon.MinRange && DistanceBetweenUnits(AttackingUnit, DefendingUnit) <= ChosenWeapon.GetMaxRange())
                {
                    //can attack these; add red highlight
                    DefendingUnit.CurrentTile.HighlightTile(true, false);

                    DefendingUnit.CurrentTile.Highlighted = true;

                    HighlightedTiles.Add(DefendingUnit.CurrentTile);

                    TargetableUnits.Add(DefendingUnit);
                }
            }
        }
        else //If the unit has a support ability, search for nearby allies.
        {
            List<Unit> PotentiallyTargetableAllies = new List<Unit>();

            for(int i = 0; i < AttackingUnit.Leader.UnitsOnTeam.Count; i++)
            {
                //staves can target any ally that isn't the unit themself
                if (AttackingUnit.Leader.UnitsOnTeam[i] != AttackingUnit)
                {
                    PotentiallyTargetableAllies.Add(AttackingUnit.Leader.UnitsOnTeam[i]);
                }

                //Refresh can only be used on allies that have already acted AND don't have Refresh equipped (at levels 1-2). Remove allies that don't meet these conditions
                if (ChosenWeapon.WeaponName.Contains("Refresh"))
                {
                    if(ChosenWeapon.GetComponent<Staff>().SpellLevel < 3)
                    {
                        for(int j = 0; j < PotentiallyTargetableAllies.Count; j++)
                        {
                            if (PotentiallyTargetableAllies[j].CanAct || PotentiallyTargetableAllies[j].HeldWeapon.WeaponName.Contains("Refresh") || PotentiallyTargetableAllies[j].SecondaryWeapon.WeaponName.Contains("Refresh"))
                            {
                                PotentiallyTargetableAllies.Remove(PotentiallyTargetableAllies[j]);
                                j--;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < PotentiallyTargetableAllies.Count; j++)
                        {
                            if (PotentiallyTargetableAllies[j].CanAct)
                            {
                                PotentiallyTargetableAllies.Remove(PotentiallyTargetableAllies[j]);
                                j--;
                            }
                        }
                    }
                }
            }

            //List<Unit> PotentiallyTargetableUnits = Controls.PlayerList[AttackingUnit.Leader.GetEnemyTeam() - 1].SpawnedUnits;

            foreach (Unit AlliedUnit in PotentiallyTargetableAllies)
            {
                if (DistanceBetweenUnits(AttackingUnit, AlliedUnit) >= ChosenWeapon.MinRange && DistanceBetweenUnits(AttackingUnit, AlliedUnit) <= ChosenWeapon.GetMaxRange())
                {
                    //can attack these; add red highlight
                    AlliedUnit.CurrentTile.HighlightTile(true, false);

                    AlliedUnit.CurrentTile.Highlighted = true;

                    HighlightedTiles.Add(AlliedUnit.CurrentTile);

                    TargetableUnits.Add(AlliedUnit);
                }
            }

        }

        return TargetableUnits;
    }
}
