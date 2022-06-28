using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager instance;

    public TextMeshProUGUI scoreboard;
    public TextMeshProUGUI gameoverScreen;
    public GameObject spawnpointLeft, spawnpointRight, spawnpointTop, spawnpointBottom;
    public GameObject[] arrowPrefabs;
    public Dictionary<string, GameObject> arrowPrefabsByName = new Dictionary<string, GameObject>();

    public MODE mode = MODE.RANDOM;
    public int level = 4;
    public float delayBetweenArrows = 1f;

    public static event Action<GAME_STATE> OnGameStateChanged;

    private float timeToShoot;
    private string[] lines;
    private int arrowNumber;
    private float startTime;
    private readonly int DIRECTION = 0;
    private readonly int SPAWNTIME = 1;
    private readonly int VELOCITY = 2;
    private readonly int PREFAB = 3;


    private List<Arrow> arrows = new List<Arrow>();
    private GAME_STATE gameState;

    private void Awake()
    {
        instance = this;

        foreach(GameObject arrowPrefab in arrowPrefabs)
        {
            arrowPrefabsByName.Add(arrowPrefab.name, arrowPrefab);
        }
    }

    private void Start()
    {
        startTime = Time.time;
        arrowNumber = 1;

        if (mode.Equals(MODE.FILE))
        {
            TextAsset file = Resources.Load<TextAsset>("Level" + level);
            lines = Regex.Split(file.text, "\r\n");
        }
        else if (mode.Equals(MODE.RANDOM))
        {
            lines = GenerateRandomLines(500);
        }

        NockArrow();
    }

    private void Update()
    {
        if(gameState.Equals(GAME_STATE.PLAYING) && timeToShoot <= Time.time)
        {
            arrows[arrows.Count - 1].Shoot();

            if (arrowNumber < lines.Length)
                NockArrow();
            else
            {
                level++;
                TextAsset file = Resources.Load<TextAsset>("Level" + level);
                lines = Regex.Split(file.text, "\r\n");
                startTime = Time.time + 2;
                arrowNumber = 1;
                NockArrow();
            }
        }

        if (gameState.Equals(GAME_STATE.GAME_END) && Input.GetKeyDown(KeyCode.Space))
        {
            UpdateGameState(GAME_STATE.PLAYING);
        }
    }

    private void NockArrow()
    {
        GameObject spawnpoint;
        Vector2 direction;

        string[] arrowInfo = Regex.Split(lines[arrowNumber++], "\t");
        string directionString = arrowInfo[DIRECTION];
        float velocity = float.Parse(arrowInfo[VELOCITY]);
        timeToShoot = startTime + float.Parse(arrowInfo[SPAWNTIME]);

        if (directionString.Equals("LEFT"))
        {
            spawnpoint = spawnpointLeft;
            direction = new Vector2(1, 0);
        }
        else if (directionString.Equals("RIGHT"))
        {
            spawnpoint = spawnpointRight;
            direction = new Vector2(-1, 0);
        }
        else if (directionString.Equals("TOP"))
        {
            spawnpoint = spawnpointTop;
            direction = new Vector2(0, -1);
        }
        else
        {
            spawnpoint = spawnpointBottom;
            direction = new Vector2(0, 1);
        }

        Arrow arrow = Instantiate(arrowPrefabsByName[arrowInfo[PREFAB]], spawnpoint.transform.position, Quaternion.identity).GetComponent<Arrow>();
        arrow.Setup(direction, velocity);

        arrows.Add(arrow);
        arrows[0].Highlight();
    }

    public void RemoveArrow(Arrow arrow)
    {
        arrows.Remove(arrow);
        if (arrows.Count > 0) arrows[0].Highlight();
    }

    private string[] GenerateRandomLines(int count)
    {
        string[] directions = new string[4] { "TOP","LEFT","BOTTOM","RIGHT" };
        float spawntime = 0;
        int velocity = 8;
        string arrowType = "Arrow";

        string[] randomLines = new string[count];
        for (int i = 0; i < count; i++)
        {
            spawntime += Mathf.Max(delayBetweenArrows * Random.Range(.5f, Mathf.Max(1f, 1.5f - i/100f)), arrowType.Equals("Reverse Arrow") ? .7f : 0f);
            arrowType = Random.Range(0f, 1f) > .9f ? "Reverse Arrow" : "Arrow";
            randomLines[i] = directions[Random.Range(0,4)] + "\t" + spawntime + "\t" + velocity + "\t" + arrowType;
        }

        return randomLines;
    }

    public void UpdateGameState(GAME_STATE newState)
    {
        gameState = newState;
        switch (gameState)
        {
            case GAME_STATE.PLAYING:
                //gameoverScreen.enabled = false;
                //Start();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case GAME_STATE.GAME_END:
                if(int.Parse(scoreboard.text) > PlayerPrefs.GetInt("highscore", 0))
                    PlayerPrefs.SetInt("highscore", int.Parse(scoreboard.text));
                arrows.ForEach(a => Destroy(a.gameObject));
                arrows = new List<Arrow>();
                gameoverScreen.enabled = true;
                break;
        }
        OnGameStateChanged(gameState);
    }
}

public enum MODE
{
    RANDOM,
    FILE
}

public enum GAME_STATE
{
    PLAYING,
    GAME_END
}