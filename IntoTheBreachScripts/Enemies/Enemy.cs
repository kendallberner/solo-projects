using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Enemy : Character
{
    [HideInInspector] public List<Tile> targetableTiles;
    [HideInInspector] public Tile targetTile;
    [HideInInspector] public bool attackPrepared;

    public GameObject meleeTargetingArrowPrefab, projectileTargetingLinePrefab, projectilePrefab;

    protected override void Start()
    {
        base.Start();
    }

    public override void Select()
    {
        base.Select();
        if(GameManager.Instance.getGameState().Equals(GAME_STATE.PLAYER_TURN_HERO_SELECTED)) GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN);
    }

    public override async Task Move(Tile originTile, Tile destinationTile)
    {
        if (originTile == destinationTile) return;

        Enemy movingCharacter = (Enemy)originTile.occupant;
        Dictionary<Tile, List<Tile>> reachableTilesByPath = GridManager.Instance.GetReachableTilesByPath(movingCharacter);
        if (reachableTilesByPath.ContainsKey(destinationTile) && destinationTile.IsOccupiable(movingCharacter))
        {
            destinationTile.occupant = movingCharacter;
            originTile.occupant = null;
            movingCharacter.occupiedTile = destinationTile;

            movingCharacter.transform.localPosition = destinationTile.transform.position;

            foreach (Tile tile in reachableTilesByPath[destinationTile])
            {
                transform.localPosition = tile.transform.position;
                await Task.Delay(200);
            }
        }
    }

    public abstract List<Tile> GetTilesTargetableFromGivenTile(Tile tile);

    public abstract GameObject Telegraph();
    public abstract Task Attack();

    public override void Die()
    {
        attackPrepared = false;
        GridManager.Instance.RemoveEnemyFromList(this);
        GridManager.Instance.DisplayAttackingEnemiesTelegraphy();

        base.Die();
    }

    public void DisplayDetailedAttackTelegraphy()
    {
        if (targetTile.occupant != null)
        {
            targetTile.occupant.DisplayHealthBar(true);
            targetTile.occupant.Threaten(1);
        }
    }

    public void EndDisplayDetailedAttackTelegraphy()
    {
        if (targetTile!= null && targetTile.occupant != null)
        {
            targetTile.occupant.UnThreaten();
            targetTile.occupant.DisplayHealthBar(false);
        }
    }
}
