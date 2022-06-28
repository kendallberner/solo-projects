using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public class Repair : Attack
    {
        public override void Telegraph()
        {
            List<Tile> tiles = new List<Tile>();
            tiles.Add(owner.occupiedTile);
            Highlight(tiles, Color.green);
        }

        public override void Telemetry(Tile targetTile)
        {
            TextMeshPro damagePreview = Instantiate(damagePreviewPrefab, targetTile.transform);
            damagePreview.text = damage.ToString();
            damagePreview.color = Color.green;
            damagePreviews.Add(damagePreview);
        }

        public override void Execute(Tile targetTile)
        {
            if (owner.hitpoints < owner.maxHitpoints) owner.hitpoints++;
            owner.DouseFlames();
            owner.isAcidic = false;
            owner.isFrozen = false;

            PostAction();
        }
    }
}
