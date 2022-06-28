using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public class MechaPunch : Attack
    {
        public override void Telegraph()
        {
            List<Tile> tiles = GridManager.Instance.GetTilesForAttackBasicMelee(owner.occupiedTile);
            Highlight(tiles);
        }

        public override void Telemetry(Tile targetTile)
        {
            if(targetTile.occupant != null)
            {
                TextMeshPro damagePreview = Instantiate(damagePreviewPrefab, targetTile.transform);
                damagePreview.text = damage.ToString();
                damagePreviews.Add(damagePreview);

                DIRECTION direction = GridManager.Instance.GetDirectionBetween(owner.occupiedTile, targetTile);
                float zRotation = GridManager.DIRECTION_ROTATIONS[direction];
                Vector3 offset = GridManager.DIRECTION_VECTORS[direction] * .55f;
                GameObject shiftArrow = Instantiate(shiftArrowPrefab, targetTile.transform.position + offset, Quaternion.Euler(0, 0, zRotation));
                shiftArrows.Add(shiftArrow);

                Tile adjacentTile = GridManager.Instance.GetTileInDirection(targetTile, direction, 1);
                if(adjacentTile != null && adjacentTile.IsBlocker())
                {
                    targetTile.Threaten(damage + 1);
                    adjacentTile.Threaten(1);
                    endangeredTiles.Add(adjacentTile);
                }
                else
                {
                    targetTile.Threaten(damage);
                }
            }
            else
            {
                targetTile.Threaten(damage);
            }

            endangeredTiles.Add(targetTile);
        }

        public override void Execute(Tile targetTile)
        {
            CancelTelemetry();
            targetTile.CancelTelemetry();
            targetTile.occupant.UnThreaten();

            targetTile.TakeDamage(2);
            DIRECTION direction = GridManager.Instance.GetDirectionBetween(owner.occupiedTile, targetTile);
            if(targetTile.occupant != null) GridManager.Instance.Shift(targetTile.occupant, direction);

            PostAction();
        }
    }
}
