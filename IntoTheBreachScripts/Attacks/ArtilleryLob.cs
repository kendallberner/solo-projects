using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public class ArtilleryLob : Attack
    {
        public override void Telegraph()
        {
            List<Tile> tiles = GridManager.Instance.GetTilesForAttackNonAdjacentArtillery(owner.occupiedTile);
            Highlight(tiles);
        }

        public override void Telemetry(Tile targetTile)
        {
            if (targetTile.occupant != null)
            {
                TextMeshPro damagePreview = Instantiate(damagePreviewPrefab, targetTile.transform);
                damagePreview.text = damage.ToString();
                damagePreviews.Add(damagePreview);
            }
            targetTile.Threaten(damage);
            endangeredTiles.Add(targetTile);

            List<Tile> adjacentTiles = GridManager.Instance.GetAdjacentTiles(targetTile);
            foreach (Tile tile in adjacentTiles)
            {
                DIRECTION direction = GridManager.Instance.GetDirectionBetween(targetTile, tile);
                if (tile.occupant != null)
                {
                    float zRotation = GridManager.DIRECTION_ROTATIONS[direction];
                    Vector3 offset = GridManager.DIRECTION_VECTORS[direction] * .55f;
                    GameObject shiftArrow = Instantiate(shiftArrowPrefab, tile.transform.position + offset, Quaternion.Euler(0, 0, zRotation));
                    shiftArrows.Add(shiftArrow);

                    Tile adjacentTile = GridManager.Instance.GetTileInDirection(tile, direction, 1);
                    if (adjacentTile != null && adjacentTile.IsBlocker())
                    {
                        tile.occupant.Threaten(1);
                        adjacentTile.Threaten(1);
                        endangeredTiles.Add(adjacentTile);
                        endangeredTiles.Add(tile);
                    }
                }
            }
        }

        public override void Execute(Tile targetTile)
        {
            CancelTelemetry();
            targetTile.CancelTelemetry();

            targetTile.TakeDamage(1);

            List<Tile> adjacentTiles = GridManager.Instance.GetAdjacentTiles(targetTile);
            foreach(Tile tile in adjacentTiles)
            {
                DIRECTION direction = GridManager.Instance.GetDirectionBetween(targetTile, tile);
                if(tile.occupant != null) GridManager.Instance.Shift(tile.occupant, direction);
            }
            
            PostAction();
        }
    }
}
