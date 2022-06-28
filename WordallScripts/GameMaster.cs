using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public TextMeshProUGUI gameEndTextComponent;
    public GameObject restartConfirmationModal;
    public GameObject optionsModal;

    public Bar[] bars;
    public TextMeshProUGUI totalWinsTextComponent;

    public TextAsset fiveLetterWordsTextFile;
    public TextAsset statsTextFile;
    public GameObject verticalLayoutGroup;
    public GameObject alphabetVerticalLayoutGroup;

    private string keyboardOrder = "QWERTYUIOPASDFGHJKLZXCVBNM";
    private System.Random rand = new System.Random();
    private string[] possibleWordles;

    private GAME_STATE state;
    private string actualWord;
    private int guessCount = 0;
    private int currentLetterIndex = 0;

    private int[] stats;

    private bool hardMode = true;
    private List<String> knownLetters = new List<String>();
    private List<Tuple<String, int>> knownLettersWithPositions = new List<Tuple<String, int>>();
    private string guessedWord = "";

    private void Awake()
    {
        ReadWordlesFromFile();
        PickWordle();
        LoadStats();
        UpdateGameState(GAME_STATE.PLAYING);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(state == GAME_STATE.PAUSE && Input.GetKeyDown(KeyCode.Escape))
        {
            restartConfirmationModal.GetComponent<Panel>().SetYPos(-1000f);
            UpdateGameState(GAME_STATE.PLAYING);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
            MakeGuess();
        else if (Input.GetKeyDown(KeyCode.Backspace))
            DeleteLetter();
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (state == GAME_STATE.PLAYING)
            {
                restartConfirmationModal.GetComponent<Panel>().SetYPos(0f);
                UpdateGameState(GAME_STATE.PAUSE);
            }
            else if (state == GAME_STATE.PAUSE || state == GAME_STATE.VICTORY || state == GAME_STATE.LOSS)
            {
                UpdateGameState(GAME_STATE.RESTART);
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
            EnterLetter("A");
        else if (Input.GetKeyDown(KeyCode.B))
            EnterLetter("B");
        else if (Input.GetKeyDown(KeyCode.C))
            EnterLetter("C");
        else if (Input.GetKeyDown(KeyCode.D))
            EnterLetter("D");
        else if (Input.GetKeyDown(KeyCode.E))
            EnterLetter("E");
        else if (Input.GetKeyDown(KeyCode.F))
            EnterLetter("F");
        else if (Input.GetKeyDown(KeyCode.G))
            EnterLetter("G");
        else if (Input.GetKeyDown(KeyCode.H))
            EnterLetter("H");
        else if (Input.GetKeyDown(KeyCode.I))
            EnterLetter("I");
        else if (Input.GetKeyDown(KeyCode.J))
            EnterLetter("J");
        else if (Input.GetKeyDown(KeyCode.K))
            EnterLetter("K");
        else if (Input.GetKeyDown(KeyCode.L))
            EnterLetter("L");
        else if (Input.GetKeyDown(KeyCode.M))
            EnterLetter("M");
        else if (Input.GetKeyDown(KeyCode.N))
            EnterLetter("N");
        else if (Input.GetKeyDown(KeyCode.O))
            EnterLetter("O");
        else if (Input.GetKeyDown(KeyCode.P))
            EnterLetter("P");
        else if (Input.GetKeyDown(KeyCode.Q))
            EnterLetter("Q");
        else if (Input.GetKeyDown(KeyCode.R))
            EnterLetter("R");
        else if (Input.GetKeyDown(KeyCode.S))
            EnterLetter("S");
        else if (Input.GetKeyDown(KeyCode.T))
            EnterLetter("T");
        else if (Input.GetKeyDown(KeyCode.U))
            EnterLetter("U");
        else if (Input.GetKeyDown(KeyCode.V))
            EnterLetter("V");
        else if (Input.GetKeyDown(KeyCode.W))
            EnterLetter("W");
        else if (Input.GetKeyDown(KeyCode.X))
            EnterLetter("X");
        else if (Input.GetKeyDown(KeyCode.Y))
            EnterLetter("Y");
        else if (Input.GetKeyDown(KeyCode.Z))
            EnterLetter("Z");
    }

    private void ResetWordle()
    {
        for(guessCount = 0; guessCount < 6; guessCount++)
        {
            for(currentLetterIndex = 0; currentLetterIndex < 5; currentLetterIndex++)
            {
                TextMeshProUGUI cell = GetCurrentCell();
                GetImageFromCell(cell).SetColor(Color.white);
                cell.text = "";
            }
        }

        CleanAlphabet();
        PickWordle();
        guessCount = 0;
        currentLetterIndex = 0;
        knownLetters.Clear();
        knownLettersWithPositions.Clear();

        gameEndTextComponent.text = "";
        restartConfirmationModal.GetComponent<Panel>().SetYPos(-1000f);

        UpdateGameState(GAME_STATE.PLAYING);
    }

    private void PickWordle()
    {
        //actualWord = possibleWordles[rand.Next(0, possibleWordles.Length-1)].ToUpper();
        actualWord = "BEING";
    }

    private void ReadWordlesFromFile()
    {
        string text = fiveLetterWordsTextFile.text;
        string[] lines = Regex.Split(text, "\r\n");

        possibleWordles = new string[lines.Length];

        int i = 0;
        foreach (string line in lines)
        {
            possibleWordles[i] = line;
            i++;
        }
    }

    private void EnterLetter(string letter)
    {
        if(state == GAME_STATE.PLAYING && currentLetterIndex < 5)
        {
            GetCurrentCell().text = letter;
            GetImageFromCell(GetCurrentCell()).SetScale(new Vector3(1.1f, 1.1f));
            //GetImageFromCell(GetCurrentCell()).SetBackgroundScale(new Vector3(.95f, .9f));
            currentLetterIndex++;
            guessedWord += letter;
        }
    }

    private void DeleteLetter()
    {
        if (state == GAME_STATE.PLAYING && currentLetterIndex > 0)
        {
            currentLetterIndex--;
            GetCurrentCell().text = "";
            //GetImageFromCell(GetCurrentCell()).SetBackgroundScale(new Vector3(1f, 1f));
            guessedWord = guessedWord.Remove(currentLetterIndex);
        }
    }

    private TextMeshProUGUI GetCurrentCell()
    {
        return verticalLayoutGroup.transform.GetChild(guessCount).transform.GetChild(currentLetterIndex).GetComponentInChildren<TextMeshProUGUI>();
    }

    private Letter GetImageFromCell(TextMeshProUGUI cell)
    {
        return cell.transform.parent.GetComponent<Letter>();
    }

    private void MakeGuess()
    {
        if (state != GAME_STATE.PLAYING || currentLetterIndex != 5)
            return;

        gameEndTextComponent.text = "";
        if (hardMode)
        {
            foreach(string letter in knownLetters)
            {
                if (!guessedWord.Contains(letter))
                {
                    gameEndTextComponent.text = "Must include " + letter;
                    return;
                }
            }
            foreach(Tuple<String, int> tuple in knownLettersWithPositions)
            {
                string enteredLetter = guessedWord[tuple.Item2].ToString();
                if (!enteredLetter.Equals(tuple.Item1))
                {
                    gameEndTextComponent.text = "Must correctly position " + tuple.Item1;
                    return;
                }
            }
        }

        //string guessedWord = "";
        for (currentLetterIndex = 0; currentLetterIndex < 5; currentLetterIndex++)
        {
            TextMeshProUGUI cell = GetCurrentCell();
            Letter image = GetImageFromCell(cell);
            //guessedWord += cell.text;

            int indexOf = actualWord.IndexOf(cell.text);
            if (indexOf == -1)
            {
                image.SetColor(Color.gray);
                GetAlphabetImage(cell.text).color = Color.gray;
            }
            else if(indexOf == currentLetterIndex || actualWord[currentLetterIndex].ToString() == cell.text)
            {
                image.SetColor(Color.green);
                GetAlphabetImage(cell.text).color = Color.green;
                knownLetters.Add(cell.text);
                knownLettersWithPositions.Add(new Tuple<string, int>(cell.text, currentLetterIndex));
            }
            else
            {
                image.SetColor(Color.yellow);
                GetAlphabetImage(cell.text).color = Color.yellow;
                knownLetters.Add(cell.text);
            }
        }

        guessCount++;
        currentLetterIndex = 0;

        if (actualWord == guessedWord)
            UpdateGameState(GAME_STATE.VICTORY);
        else if(guessCount == 6)
            UpdateGameState(GAME_STATE.LOSS);

        guessedWord = "";
    }

    private Image GetAlphabetImage(string letter)
    {
        for(int i = 0; i < alphabetVerticalLayoutGroup.transform.childCount; i++)
        {
            Transform alphabetRow = alphabetVerticalLayoutGroup.transform.GetChild(i);
            for(int j = 0; j < alphabetRow.childCount; j++)
            {
                if(alphabetRow.GetChild(j).GetComponentInChildren<TextMeshProUGUI>().text == letter)
                    return alphabetRow.GetChild(j).GetComponent<Image>();
            }
        }

        return null;
    }

    private void CleanAlphabet()
    {
        int index = 0;

        for (int i = 0; i < alphabetVerticalLayoutGroup.transform.childCount; i++)
        {
            Transform alphabetRow = alphabetVerticalLayoutGroup.transform.GetChild(i);
            for (int j = 0; j < alphabetRow.childCount; j++)
            {
                alphabetRow.GetChild(j).GetComponentInChildren<TextMeshProUGUI>().text = keyboardOrder.ToCharArray()[index].ToString();
                alphabetRow.GetChild(j).GetComponent<Image>().color = Color.white;
                index++;
            }
        }
    }

    private void LoadStats()
    {
        string path = Application.persistentDataPath + "Stats.txt";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        StreamReader reader = new StreamReader(stream);
        if(reader.Peek() == -1) 
            stats = new int[7];
        else
        {
            reader.ReadLine();
            stats = Array.ConvertAll(Regex.Split(reader.ReadLine(), "\t+"), s => int.Parse(s));
        }

        UpdateStatDisplay();
    }

    private void UpdateStats(bool victory)
    {
        stats[0] += victory ? 1 : 0;
        if(victory) stats[guessCount]++;

        string path = Application.persistentDataPath + "Stats.txt";
        FileStream stream = new FileStream(path, FileMode.Create);

        string defaultLine = "total win1	win2	win3	win4	win5	win6";
        string statLine = stats[0] + "\t\t" + stats[1] + "\t\t" + stats[2] + "\t\t" + stats[3] + "\t\t" + stats[4] + "\t\t" + stats[5] + "\t\t" + stats[6];

        StreamWriter writer = new StreamWriter(stream);
        writer.WriteLine(defaultLine);
        writer.Write(statLine);
        writer.Flush();
        writer.Close();

        UpdateStatDisplay();
    }

    private void UpdateStatDisplay()
    {
        totalWinsTextComponent.text = "Total Wins: " + stats[0];

        float min = .1f;
        float mostWins = 1f;
        for (int i = 1; i < stats.Length; i++)
        {
            mostWins = Math.Max(mostWins, stats[i]);
        }

        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].SetBarFillAmount(min + ((1 - min) * stats[i+1] / mostWins));
            bars[i].SetBarText(stats[i+1].ToString());
        }
    }

    private void UpdateGameState(GAME_STATE newState)
    {
        state = newState;
        switch (state)
        {
            case GAME_STATE.PLAYING:
                break;
            case GAME_STATE.VICTORY:
                UpdateStats(true);
                gameEndTextComponent.text = "Congratulations!" + "\r\nPress Spacebar to Restart";
                break;
            case GAME_STATE.LOSS:
                UpdateStats(false);
                gameEndTextComponent.text = "The word was: " + actualWord.ToUpper() + "\r\nPress Spacebar to Restart";
                break;
            case GAME_STATE.PAUSE:
                break;
            case GAME_STATE.RESTART:
                ResetWordle();
                break;
        }
    }

    public void SummonOptionsMenu()
    {
        optionsModal.GetComponent<Panel>().SetYPos(0);
    }

    public void CloseOptionsMenu()
    {
        optionsModal.GetComponent<Panel>().SetYPos(-1000);
    }

    public void SetHardMode(bool on)
    {
        hardMode = on;
    }
}

public enum GAME_STATE
{
    PLAYING = 1,
    VICTORY = 2,
    LOSS = 3,
    PAUSE = 4,
    RESTART = 5
}