using Assets.Scripts.Attacks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;

public class Logger : Enemy
{
    public override GameObject Telegraph()
    {
        targetTile.PartialHighlight(Color.red);

        DIRECTION direction = GridManager.Instance.GetDirectionBetween(occupiedTile, targetTile);
        float zRotation = GridManager.DIRECTION_ROTATIONS[direction];
        Vector3 offset = GridManager.DIRECTION_VECTORS[direction] * .55f;
        GameObject arrow = Instantiate(meleeTargetingArrowPrefab, occupiedTile.transform.position + offset, Quaternion.Euler(0, 0, zRotation));

        return arrow;
    }

    public async override Task Attack()
    {
        Vector3 startPos = transform.position;

        while (Vector3.Distance(transform.position, targetTile.transform.position) > .5f)
        {
            transform.position = Vector3.Lerp(transform.position, targetTile.transform.position, Time.deltaTime * 6f);
            await Task.Delay(1);
        }
        
        targetTile.TakeDamage(1);
        transform.position = startPos;

        targetTile = null;
        attackPrepared = false;
    }

    public override List<Tile> GetTilesTargetableFromGivenTile(Tile tile)
    {
        return GridManager.Instance.GetTilesForAttackBasicMelee(tile);
    }
}
