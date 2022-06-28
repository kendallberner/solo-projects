using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public class TankShoot : Attack
    {
        public override void Telegraph()
        {
            List<Tile> tiles = GridManager.Instance.GetTilesForAttackBasicProjectile(owner.occupiedTile);
            Highlight(tiles);
        }

        public override void Telemetry(Tile targetTile)
        {
            DIRECTION direction = GridManager.Instance.GetDirectionBetween(owner.occupiedTile, targetTile);
            List<Tile> tiles = GridManager.Instance.GetTilesInDirection(owner.occupiedTile, direction);
            foreach (Tile tile in tiles)
            {
                targetTile = tile;
                if (targetTile.IsBlocker()) break;
            }

            if(targetTile.occupant != null)
            {
                TextMeshPro damagePreview = Instantiate(damagePreviewPrefab, targetTile.transform);
                damagePreview.text = damage.ToString();
                damagePreviews.Add(damagePreview);

                float zRotation = GridManager.DIRECTION_ROTATIONS[direction];
                Vector3 offset = GridManager.DIRECTION_VECTORS[direction] * .55f;
                GameObject shiftArrow = Instantiate(shiftArrowPrefab, targetTile.transform.position + offset, Quaternion.Euler(0, 0, zRotation));
                shiftArrows.Add(shiftArrow);

                Tile adjacentTile = GridManager.Instance.GetTileInDirection(targetTile, direction, 1);
                if (adjacentTile != null && adjacentTile.IsBlocker())
                {
                    targetTile.occupant.Threaten(damage + 1);
                    adjacentTile.Threaten(1);
                    endangeredTiles.Add(adjacentTile);
                }
                else
                {
                    targetTile.occupant.Threaten(damage);
                }
            }
            endangeredTiles.Add(targetTile);
        }

        public override void Execute(Tile targetTile)
        {
            CancelTelemetry();
            targetTile.TakeDamage(1);
            DIRECTION direction = GridManager.Instance.GetDirectionBetween(owner.occupiedTile, targetTile);

            List<Tile> tiles = GridManager.Instance.GetTilesInDirection(owner.occupiedTile, direction);
            foreach(Tile tile in tiles)
            {
                targetTile = tile;
                if (targetTile.IsBlocker()) break;
            }
            
            if(targetTile.occupant != null) GridManager.Instance.Shift(targetTile.occupant, direction);
            
            targetTile.CancelTelemetry();
            PostAction();
        }
    }
}
