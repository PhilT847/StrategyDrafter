using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int X_Position;
    public int Y_Position;

    public int MovementCost; //cost for units to move through this tile. 1 is normal; 2 is forests; 3 is mountains. Check notes for how cavalry/pegasus units traverse tiles.

    public Unit OccupyingUnit;

    public List<Tile> NeighboringTiles; //tiles on each side of this tile.

    public bool Highlighted; //true when this tile is highlighted for movement.
    public Sprite BlueHighlightSprite;
    public Sprite RedHighlightSprite;

    public SpriteRenderer HighlightPiece;

    void SelectTile() //if there's a unit on the tile that the player can control, check for movement/actions.
    {
        if(OccupyingUnit != null)
        {
            FindObjectOfType<TileController>().VisualizeMovementFrom(this);
        }
    }

    public void HighlightTile(bool Highlight, bool MovementHighlight)
    {
        if (Highlight)
        {
            //Change the tag so that only the controlling player sees the highlighted tiles
            HighlightPiece.gameObject.layer = LayerMask.NameToLayer(FindObjectOfType<GameController>().ActivePlayer.PlayerLayer);

            if (MovementHighlight)
            {
                HighlightPiece.sprite = BlueHighlightSprite;
            }
            else
            {
                HighlightPiece.sprite = RedHighlightSprite;
            }

            HighlightPiece.color = new Color32(255, 255, 255, 100);
        }
        else
        {
            HighlightPiece.color = Color.clear;
        }
    }
}
