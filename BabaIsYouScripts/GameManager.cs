using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI victoryText;
    public int level = 1;

    private int turn = 1;
    private GAME_STATE gameState = GAME_STATE.PLAYING;

    private void Update()
    {
        if (gameState.Equals(GAME_STATE.PLAYING))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ExecuteTurn(KeyCode.LeftArrow);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ExecuteTurn(KeyCode.RightArrow);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ExecuteTurn(KeyCode.UpArrow);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ExecuteTurn(KeyCode.DownArrow);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                //ExecuteTurn(KeyCode.Space);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Undo();
            }
        }
        else if (gameState.Equals(GAME_STATE.VICTORY))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("Level" + (level+1));
                gameState = GAME_STATE.PLAYING;
            }
        }
    }

    private void Undo()
    {
        if(turn > 1)
        {
            turn--;
            GridManager.instance.Undo(turn);
        }
    }

    private void ResetLevel()
    {
        while(turn > 1)
        {
            Undo();
        }
    }

    private void ExecuteTurn(KeyCode key)
    {
        ClearRules();
        DetermineRules();

        Noun[] nouns = GameObject.FindObjectsOfType<Noun>();
        if (!nouns.Any(x => x.isYou)) return;
        bool incrementTurn = false;
        for (int i = 0; i < nouns.Length; i++)
        {
            Noun noun = nouns[i];
            if (noun.isYou)
            {
                incrementTurn = GridManager.instance.Move(noun, key, turn) || incrementTurn;
            }
        }
        

        for (int i = 0; i < nouns.Length; i++)
        {
            Noun noun = nouns[i];
            List<Noun> listOfJustMe = new List<Noun>();
            listOfJustMe.Add(noun);
            if (noun.isYou && noun.isWin)
            {
                Win();
            }

            List<Noun> nounsAtMyPos = GridManager.instance.GetNounsAt(noun.X(), noun.Y()).Except(listOfJustMe).ToList();
            foreach (Noun roommateNoun in nounsAtMyPos)
            {
                if(noun.isYou && roommateNoun.isWin)
                {
                    Win();
                }

                if(noun.isYou && roommateNoun.isLose)
                {
                    GridManager.instance.RemoveFrom(noun, noun.X(), noun.Y());
                    noun.gameObject.SetActive(false);
                }

                if (roommateNoun.isExplode)
                {
                    GridManager.instance.RemoveList(noun.X(), noun.Y());
                    noun.gameObject.SetActive(false);
                    roommateNoun.gameObject.SetActive(false);
                    GridManager.instance.AddToMoveHistory(turn, new Tuple<Thing, Vector2>(roommateNoun, new Vector2(roommateNoun.X(), roommateNoun.Y())));
                }
            }
        }
        if (incrementTurn) turn++;
    }

    private void Win()
    {
        gameState = GAME_STATE.VICTORY;
        victoryText.enabled = true;
    }

    private void DetermineRules()
    {
        Rule[] ruleNouns = GameObject.FindObjectsOfType<Rule>().Where(x => x.isNoun).ToArray();
        for (int i = 0; i < ruleNouns.Length; i++)
        {
            Rule ruleNoun = ruleNouns[i];

            //Check for rule going right
            Rule ruleVerb = GridManager.instance.GetRuleAt(ruleNoun.X() + 1, ruleNoun.Y());
            if (ruleVerb != null && ruleVerb.isVerb)
            {
                Rule ruleAdjective = GridManager.instance.GetRuleAt(ruleNoun.X() + 2, ruleNoun.Y());
                if (ruleAdjective != null && ruleAdjective.isAdjective) {
                    if (ruleVerb.name.Equals("IS"))
                    {
                        Apply(ruleAdjective.name, ruleNoun.name);
                    }
                }
            }


            //Check for rule going down
            ruleVerb = GridManager.instance.GetRuleAt(ruleNoun.X(), ruleNoun.Y() - 1);
            if (ruleVerb != null && ruleVerb.isVerb)
            {
                Rule ruleAdjective = GridManager.instance.GetRuleAt(ruleNoun.X(), ruleNoun.Y() - 2);
                if (ruleAdjective != null && ruleAdjective.isAdjective)
                {
                    if (ruleVerb.name.Equals("IS"))
                    {
                        Apply(ruleAdjective.name, ruleNoun.name);
                    }
                }
            }
        }
    }

    private void ClearRules()
    {
        Noun[] nouns = GameObject.FindObjectsOfType<Noun>();

        foreach (Noun noun in nouns)
        {
            noun.isYou = false;
            noun.isWin = false;
            noun.isStop = false;
            noun.isPush = false;
            noun.isMove = false;
            noun.isLose = false;
            noun.isLock = false;
            noun.isKey = false;
            noun.isExplode = false;
        }
    }

    private void Apply(string adjectiveName, string nounName)
    {
        Noun[] nouns = GameObject.FindObjectsOfType<Noun>();

        foreach (Noun noun in nouns)
        {
            if (noun.name.Equals(nounName))
            {
                switch (adjectiveName)
                {
                    case "YOU":
                        noun.isYou = true;
                        break;
                    case "EXPLODE":
                        noun.isExplode = true;
                        break;
                    case "KEY":
                        noun.isKey = true;
                        break;
                    case "LOCK":
                        noun.isLock = true;
                        break;
                    case "DEFEAT":
                        noun.isLose = true;
                        break;
                    case "MOVE":
                        noun.isMove = true;
                        break;
                    case "PUSH":
                        noun.isPush = true;
                        break;
                    case "STOP":
                        noun.isStop = true;
                        break;
                    case "WIN":
                        noun.isWin = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

public enum GAME_STATE
{
    PLAYING,
    VICTORY
}