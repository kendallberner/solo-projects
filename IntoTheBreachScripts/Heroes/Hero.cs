using Assets.Scripts.Attacks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Hero : Character
{
    [SerializeField] private SpriteRenderer actionIndicatorDisplay, movementIndicatorDisplay;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        actionIndicatorDisplay.enabled = !hasActed;
        movementIndicatorDisplay.enabled = !hasMoved;
    }

    public override void Select()
    {
        base.Select();
        GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN_HERO_SELECTED);
    }

    public override async Task Move(Tile originTile, Tile destinationTile)
    {
        Dictionary<Tile, List<Tile>> reachableTilesByPath = GridManager.Instance.GetReachableTilesByPath(this);
        if (CanMove && reachableTilesByPath.ContainsKey(destinationTile) && destinationTile.IsOccupiable(this))
        {
            hasMoved = true;

            destinationTile.occupant = this;
            originTile.occupant = null;
            occupiedTile = destinationTile;

            transform.localPosition = destinationTile.transform.position;

            GridManager.Instance.EndTileHighlightingForMovement();
            if (GameManager.Instance.getGameState() == GAME_STATE.PLAYER_TURN_HERO_SELECTED && GridManager.Instance.AreAllHeroesOutOfActions())
                GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN_ENDSTEP);

            foreach (Tile tile in reachableTilesByPath[destinationTile])
            {
                transform.localPosition = tile.transform.position;
                await Task.Delay(200);
            }
        }
        GridManager.Instance.DisplayAttackingEnemiesTelegraphy();
    }

    public void RemoveAttackPreparation()
    {
        if (attack1 != null) attack1.prepared = false;
        if (attack2 != null) attack2.prepared = false;
    }

    public async override Task Upkeep()
    {
        base.Upkeep();
        if (isBurning && !isFlameproof) TakeDamage(1, DAMAGE_TYPE.FIRE);
    }
}
