using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameStateDisplay;
    [SerializeField] TextMeshProUGUI turnsUntilVictoryDisplay;
    [SerializeField] TextMeshProUGUI attackHotKeysDisplay;
    [SerializeField] Button advanceToNextLevelButton;
    [SerializeField] Image powerGridImage;
    [SerializeField] int powerGridMaxHealth;

    public int desiredEnemyCount;
    public int initialEnemyCount;

    public static GameManager Instance;

    private GAME_STATE state;
    private int powerGridHealth;
    [HideInInspector] private int turnsRemaining;

    public static event Action<GAME_STATE, GAME_STATE> OnGameStateChanged;

    private void Awake()
    {
        Instance = this;

        powerGridHealth = powerGridMaxHealth;
    }

    private void Start()
    {
        UpdateGameState(GAME_STATE.PREPARING_MAP);
    }

    private void Update()
    {
        if(state == GAME_STATE.PLAYER_TURN_HERO_SELECTED)
        {
            Hero hero = (Hero)GridManager.Instance.selectedCharacter;
            if (Input.GetKeyDown(KeyCode.Alpha1) && hero.CanAct && !hero.occupiedTile.IsSubmerged() || hero.isFlying)
            {
                hero.PrepareAttack1();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && hero.CanAct && !hero.occupiedTile.IsSubmerged() || hero.isFlying)
            {
                hero.PrepareAttack2();
            }
            else if (Input.GetKeyDown(KeyCode.R) && hero.CanAct)
            {
                hero.PrepareRepair();
            }
        }
        
    }

    public void TakeGridDamage(int damage)
    {
        powerGridHealth -= damage;
        powerGridImage.fillAmount = (float)powerGridHealth / powerGridMaxHealth;
        if (powerGridHealth == 0) {
            turnsUntilVictoryDisplay.text = "Defeat! Click to start again";
            advanceToNextLevelButton.enabled = true;
            UpdateGameState(GAME_STATE.GAME_END);
        }
    }

    public GAME_STATE getGameState()
    {
        return state;
    }

    public void EndTurn()
    {
        UpdateGameState(GAME_STATE.PLAYER_TURN_ENDSTEP);
    }

    public void UpdateGameState(GAME_STATE newGameState)
    {
        GAME_STATE oldState = state;
        switch (oldState)
        {
            case GAME_STATE.PREPARING_MAP:
                break;
            case GAME_STATE.SPAWN_ENEMIES:
                break;
            case GAME_STATE.SPAWN_HEROES:
                break;
            case GAME_STATE.PLAYER_TURN_UPKEEP:
                break;
            case GAME_STATE.PLAYER_TURN:
                break;
            case GAME_STATE.PLAYER_TURN_HERO_SELECTED:
                attackHotKeysDisplay.enabled = false;
                break;
            case GAME_STATE.PLAYER_TURN_ATTACK_SELECTED:
                break;
            case GAME_STATE.PLAYER_TURN_ENDSTEP:
                break;
            case GAME_STATE.ENEMY_TURN:
                break;
            case GAME_STATE.ENEMY_TURN_ENDSTEP:
                break;
            case GAME_STATE.CLEANUP:
                break;
            default:
                break;
        }


        state = newGameState;
        switch (newGameState)
        {
            case GAME_STATE.PREPARING_MAP:
                break;
            case GAME_STATE.SPAWN_ENEMIES:
                break;
            case GAME_STATE.SPAWN_HEROES:
                break;
            case GAME_STATE.PLAYER_TURN_UPKEEP:
                break;
            case GAME_STATE.PLAYER_TURN:
                break;
            case GAME_STATE.PLAYER_TURN_HERO_SELECTED:
                attackHotKeysDisplay.enabled = true;
                break;
            case GAME_STATE.PLAYER_TURN_ATTACK_SELECTED:
                break;
            case GAME_STATE.PLAYER_TURN_ENDSTEP:
                break;
            case GAME_STATE.ENEMY_TURN:
                break;
            case GAME_STATE.ENEMY_TURN_ENDSTEP:
                break;
            case GAME_STATE.CLEANUP:
                break;
            default:
                break;
        }

        gameStateDisplay.text = state.ToString();
        OnGameStateChanged(oldState, state);
    }

    public void UpdateTurnCount(int turns)
    {
        turnsRemaining = turns;
        if(turnsRemaining == 0)
        {
            turnsUntilVictoryDisplay.text = "Victory! Click to advance to next level";
            advanceToNextLevelButton.enabled = true;
            UpdateGameState(GAME_STATE.GAME_END);
        }
        else
        {
            turnsUntilVictoryDisplay.text = "Victory in " + turnsRemaining + " turns";
        }
    }

    public void DecrementTurnCount()
    {
        turnsRemaining--;
        UpdateTurnCount(turnsRemaining);
    }

    public int GetTurnCount()
    {
        return turnsRemaining;
    }

    public void EndGame()
    {
        advanceToNextLevelButton.enabled = false;
        UpdateGameState(GAME_STATE.CLEANUP);
    }
}

public enum GAME_STATE
{
    PREPARING_MAP,
    SPAWN_ENEMIES,
    SPAWN_HEROES,
    PLAYER_TURN_UPKEEP,
    PLAYER_TURN,
    PLAYER_TURN_HERO_SELECTED,
    PLAYER_TURN_ATTACK_SELECTED,
    PLAYER_TURN_ENDSTEP,
    ENEMY_TURN,
    ENEMY_TURN_ENDSTEP,
    GAME_END,
    CLEANUP
}