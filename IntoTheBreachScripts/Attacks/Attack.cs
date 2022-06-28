using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    [HideInInspector] public Character owner;
    [HideInInspector] public bool prepared;

    public GameObject shiftArrowPrefab;
    public TextMeshPro damagePreviewPrefab;

    public List<GameObject> shiftArrows = new List<GameObject>();
    public List<TextMeshPro> damagePreviews = new List<TextMeshPro>();

    public int damage;

    protected List<Tile> endangeredTiles = new List<Tile>();

    public abstract void Telegraph();

    public abstract void Execute(Tile targetTile);

    protected void Highlight(List<Tile> tiles)
    {
        Highlight(tiles, Color.red);
    }

    protected void Highlight(List<Tile> tiles, Color color)
    {
        GridManager.Instance.HighlightTilesForMovement(tiles, color);
        GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN_ATTACK_SELECTED);
    }

    public abstract void Telemetry(Tile targetTile);
    public virtual void CancelTelemetry()
    {
        foreach (TextMeshPro damagePreview in damagePreviews)
        {
            Destroy(damagePreview.gameObject);
        }
        foreach (GameObject shiftArrow in shiftArrows)
        {
            Destroy(shiftArrow);
        }
        damagePreviews.Clear();
        shiftArrows.Clear();

        foreach(Tile tile in endangeredTiles)
        {
            tile.UnThreaten();
        }
        endangeredTiles.Clear();
    }

    protected void PostAction()
    {
        prepared = false;
        owner.hasActed = true;
        owner.hasMoved = true;
        GridManager.Instance.EndTileHighlightingForMovement();

        if(GridManager.Instance.AreAllHeroesOutOfActions())
            GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN_ENDSTEP);
        else
            GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN_HERO_SELECTED);
    }
}