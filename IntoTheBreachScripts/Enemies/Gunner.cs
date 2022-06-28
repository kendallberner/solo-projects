using Assets.Scripts.Attacks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;

public class Gunner : Enemy
{
    public override GameObject Telegraph()
    {
        DIRECTION direction = GridManager.Instance.GetDirectionBetween(occupiedTile, targetTile);
        targetTile = GridManager.Instance.GetTileInProjectilePath(occupiedTile, direction);

        targetTile.PartialHighlight(Color.red);

        GameObject line = Instantiate(projectileTargetingLinePrefab);
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.SetPosition(0, occupiedTile.transform.position);
        lr.SetPosition(1, targetTile.transform.position);

        return line;
    }

    public async override Task Attack()
    {
        Transform projectile = Instantiate(projectilePrefab, transform).GetComponent<Transform>();
        float elapsedTime = 0f;
        while (Vector3.Distance(projectile.position, targetTile.transform.position) > .1f)
        {
            projectile.position = Vector3.Lerp(transform.position, targetTile.transform.position, elapsedTime+=(Time.deltaTime * 3f));
            await Task.Delay(1);
        }
        Destroy(projectile.gameObject);
        targetTile.TakeDamage(1);

        targetTile = null;
        attackPrepared = false;
    }

    public override List<Tile> GetTilesTargetableFromGivenTile(Tile tile)
    {
        return GridManager.Instance.GetTilesForAttackBasicProjectile(tile);
    }
}
