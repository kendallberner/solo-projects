using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer, spriteRenderer;
    [SerializeField] private Color defaultColor, offsetColor;
    [SerializeField] private SpriteRenderer fullHighlight, partialHighlight, spawncracks, damagedTelemetryIcon;
    [SerializeField] private bool isSubmerged, isWalkable, canBeOccupied, blocksProjectiles, isFlammable, isBurning;

    private bool underAttack;
    private float targetAlpha;

    public char keyCode;
    public string displayName;
    public string displayDescription;
    public Character occupant;

    private TextMeshProUGUI tilePositionDisplay;
    private TextMeshProUGUI occupantDisplay;
    private TextMeshProUGUI tileDescriptionDisplay;

    public bool canEnemySpawnHere;
    public bool isOffset;

    private GameManager gameManager;
    private GridManager gridManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        gridManager = GridManager.Instance;

        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GAME_STATE oldState, GAME_STATE newState)
    {
        if(oldState == GAME_STATE.PLAYER_TURN_ATTACK_SELECTED)
        {
            UnThreaten();
            if(gridManager.selectedCharacter != null) gridManager.selectedCharacter.attack1.CancelTelemetry();
        }
    }

    public void Init(bool _isOffset, bool _canEnemySpawnHere, string tileName, TextMeshProUGUI _tilePositionDisplay, TextMeshProUGUI _occupantDisplay, TextMeshProUGUI _tileDescriptionDisplay)
    {
        isOffset = _isOffset;
        backgroundSpriteRenderer.color = _isOffset ? offsetColor : defaultColor;
        canEnemySpawnHere = _canEnemySpawnHere;
        name = tileName;
        tilePositionDisplay = _tilePositionDisplay;
        occupantDisplay = _occupantDisplay;
        tileDescriptionDisplay = _tileDescriptionDisplay;
    }

    private void Update()
    {
        if (isBurning && occupant != null && !occupant.isFlameproof) occupant.SetAflame();

        if (underAttack)
        {
            partialHighlight.color = new Color(partialHighlight.color.r, partialHighlight.color.g, partialHighlight.color.b, Mathf.Lerp(partialHighlight.color.a, targetAlpha, Time.deltaTime * 5f));
            if (partialHighlight.color.a < .55f) targetAlpha = 1f;
            if (partialHighlight.color.a > .95f) targetAlpha = .5f;
        }
    }

    #region ATTRIBUTES

    public bool IsTraversable(Character traversant)
    {
        return traversant.isFlying || isWalkable && (occupant == null || occupant.faction == traversant.faction) && (traversant.isMassive || !isSubmerged);
    }

    public bool IsOccupiable(Character traversant)
    {
        return canBeOccupied && occupant == null && (traversant.isFlying || isWalkable) || traversant == occupant;
    }

    public bool CanSpawnEnemyHere()
    {
        return canEnemySpawnHere && canBeOccupied && occupant == null && isWalkable && !isSubmerged;
    }

    public bool IsBlocker()
    {
        return occupant != null || blocksProjectiles;
    }

    public bool EnemyOccupantHasAttackPrepared()
    {
        return occupant != null && occupant.faction.Equals(FACTION.ENEMY) && ((Enemy)occupant).attackPrepared;
    }

    public bool IsSubmerged()
    {
        return isSubmerged;
    }

    public bool IsBurning()
    {
        return isBurning;
    }

    #endregion

    #region HIGHLIGHTING

    public void FullHighlight(Color color)
    {
        fullHighlight.color = color;
    }

    public void EndFullHighlight()
    {
        fullHighlight.color = new Color(0, 0, 0, 0);
    }

    public void PartialHighlight(Color color)
    {
        partialHighlight.color = color;
        underAttack = true;
        targetAlpha = .5f;
    }

    public void EndPartialHighlight()
    {
        underAttack = false;
        partialHighlight.color = new Color(0, 0, 0, 0);
    }

    public void EndAllHighlights()
    {
        EndFullHighlight();
        EndPartialHighlight();
    }

    public void EnableSpawncracks()
    {
        spawncracks.enabled = true;
    }

    public void DisableSpawncracks()
    {
        spawncracks.enabled = false;
    }

    #endregion

    #region DAMAGE

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, DAMAGE_TYPE.STANDARD);
    }

    public void TakeDamage(int damage, DAMAGE_TYPE damageType)
    {
        if (occupant != null) occupant.TakeDamage(damage, damageType);

        switch (damageType)
        {
            case DAMAGE_TYPE.STANDARD:
                if (keyCode == 'F') TransformIntoNewTileType('f');
                if (keyCode == 'B' || keyCode == 'b') DamageBuilding();
                if (keyCode == 'M') TransformIntoNewTileType('m');
                if (keyCode == 'm') TransformIntoNewTileType('g');
                break;
            case DAMAGE_TYPE.FIRE:
                if (isFlammable) SetAflame();
                break;
            case DAMAGE_TYPE.SHIFT:
                if (keyCode == 'B' || keyCode == 'b') DamageBuilding();
                if (keyCode == 'M') TransformIntoNewTileType('m');
                if (keyCode == 'm') TransformIntoNewTileType('g');
                break;
        }
    }

    public void SetAflame()
    {
        isBurning = true;
    }

    public void DamageBuilding()
    {
        gameManager.TakeGridDamage(1);
        if(keyCode == 'B')
            TransformIntoNewTileType('b');
        else if (keyCode == 'b')
            TransformIntoNewTileType('g');
    }

    public void Threaten(int damage)
    {
        if (occupant != null) occupant.Threaten(damage);
        Telemetry();
    }

    public void UnThreaten()
    {
        if (occupant != null) occupant.UnThreaten();
        CancelTelemetry();
    }

    #endregion

    #region MOUSEOVER_MOUSECLICK

    private void OnMouseEnter()
    {
        tilePositionDisplay.text = displayName + " (" + transform.position.x + "," + transform.position.y + ")";
        occupantDisplay.text = occupant != null ? "Occupant: " + occupant.displayName : "Unnoccupied";
        tileDescriptionDisplay.text = displayDescription;
        if (spawncracks.enabled) tileDescriptionDisplay.text += "\nAn enemy will emerge here next turn. Any unit blocking this space will take one damage.";

        if (occupant != null) occupant.OnHover();

        switch (gameManager.getGameState())
        {
            case GAME_STATE.SPAWN_HEROES:
                gridManager.GetHeroToSpawn().transform.position = transform.position;
                break;
            case GAME_STATE.PLAYER_TURN:
                if (occupant != null && occupant.CanMove)
                    gridManager.HighlightTilesForMovement(gridManager.GetReachableTiles(occupant));
                else
                    gridManager.EndTileHighlightingForMovement();

                if (EnemyOccupantHasAttackPrepared()) ((Enemy)occupant).DisplayDetailedAttackTelegraphy();
                break;
            case GAME_STATE.PLAYER_TURN_HERO_SELECTED:
                gridManager.HighlightPathToTile(this, gridManager.GetReachableTilesByPath(gridManager.selectedCharacter));
                break;
            case GAME_STATE.PLAYER_TURN_ATTACK_SELECTED:
                if (fullHighlight.color == Color.red)
                    gridManager.selectedCharacter.attack1.Telemetry(this);
                break;
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (gameManager.getGameState())
            {
                case GAME_STATE.SPAWN_HEROES:
                    if(IsOccupiable(gridManager.GetHeroToSpawn())) gridManager.SpawnHero(this);
                    break;
                case GAME_STATE.PLAYER_TURN:
                    if (occupant != null)
                        gridManager.SelectTile(this);
                    break;
                case GAME_STATE.PLAYER_TURN_HERO_SELECTED:
                    if (occupant != null)
                        gridManager.SelectTile(this);
                    else
                        gridManager.selectedCharacter.Move(gridManager.selectedCharacter.occupiedTile, this);
                    break;
                case GAME_STATE.PLAYER_TURN_ATTACK_SELECTED:
                    gridManager.selectedCharacter.Attack(this);
                    break;
                case GAME_STATE.ENEMY_TURN:
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            switch (gameManager.getGameState())
            {
                case GAME_STATE.SPAWN_HEROES:
                    //Reselect spawn point for previous hero
                    break;
                case GAME_STATE.PLAYER_TURN_HERO_SELECTED:
                    gridManager.DeselectTile();
                    gameManager.UpdateGameState(GAME_STATE.PLAYER_TURN);
                    break;
                case GAME_STATE.PLAYER_TURN_ATTACK_SELECTED:
                    ((Hero)gridManager.selectedCharacter).RemoveAttackPreparation();
                    gridManager.EndTileHighlightingForMovement();
                    gameManager.UpdateGameState(GAME_STATE.PLAYER_TURN_HERO_SELECTED);
                    break;
                default:
                    gridManager.EndTileHighlightingForMovement();
                    break;
            }
        }
    }

    private void OnMouseExit()
    {
        switch (gameManager.getGameState())
        {
            case GAME_STATE.SPAWN_HEROES:
                gridManager.GetHeroToSpawn().transform.position = new Vector3(500, 500, 0);
                break;
            case GAME_STATE.PLAYER_TURN_ATTACK_SELECTED:
                gridManager.selectedCharacter.attack1.CancelTelemetry();
                UnThreaten();
                break;
        }
        if (EnemyOccupantHasAttackPrepared()) ((Enemy)occupant).EndDisplayDetailedAttackTelegraphy();
        if (occupant != null) occupant.LoseHover();
    }

    #endregion


    public Vector2 GetCoords()
    {
        return new Vector2(transform.position.x, transform.position.y);
    }

    public void TransformIntoNewTileType(char _keyCode)
    {
        Tile newTile = Instantiate(GridManager.tilePrefabsByKeycode[_keyCode], transform.position, Quaternion.identity);
        
        spriteRenderer.sprite = newTile.spriteRenderer.sprite;
        spriteRenderer.color = newTile.spriteRenderer.color;
        defaultColor = newTile.defaultColor;
        offsetColor = newTile.offsetColor;
        backgroundSpriteRenderer.color = isOffset ? offsetColor : defaultColor;

        isSubmerged = newTile.isSubmerged;
        isWalkable = newTile.isWalkable;
        canBeOccupied = newTile.canBeOccupied;
        blocksProjectiles = newTile.blocksProjectiles;
        isFlammable = newTile.isFlammable;
        isBurning = newTile.isBurning;

        keyCode = newTile.keyCode;
        displayName = newTile.displayName;
        displayDescription = newTile.displayDescription;

        Destroy(newTile.gameObject);
    }

    public void Telemetry()
    {
        if(damagedTelemetryIcon != null) damagedTelemetryIcon.enabled = true;
    }

    public void CancelTelemetry()
    {
        if(damagedTelemetryIcon != null) damagedTelemetryIcon.enabled = false;
    }
}