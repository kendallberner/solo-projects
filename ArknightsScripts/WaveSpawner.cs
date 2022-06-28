using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class WaveSpawner : MonoBehaviour
{
    [HideInInspector] public static WaveSpawner instance;

    public GameObject pathLineLand;
    public TextAsset waveInformationFile;
    public Spawn[] waveInformation;
    public Transform[] spawnPoints;
    public Text waveProgressText;

    private readonly System.Random rand = new System.Random();
    private float countdown = 2f;
    private int spawnIndex = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        String text = waveInformationFile.text;
        String[] lines = Regex.Split(text, "\r\n");
        waveInformation = new Spawn[lines.Length];
        int i = 0;
        foreach (String line in lines)
        {
            String[] components = Regex.Split(line, ", ");
            Spawn spawn = new Spawn();
            spawn.enemyPrefab = Prefabs.GetPrefabByName(components[0]);
            spawn.spawnPoint = GameObject.Find(components[1]).transform;
            spawn.path = Waypoints.GetPath(components[2]);
            spawn.spawnDelay = float.Parse(components[3]);
            spawn.pathLine = components[4] == "PathLineLand" ? pathLineLand : null;

            waveInformation[i++] = spawn;
        }

        countdown = waveInformation[spawnIndex].spawnDelay;

        waveProgressText.text = "0/" + lines.Length;
    }

    private void Update()
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0 && spawnIndex < waveInformation.Length)
        {
            Spawn spawn = waveInformation[spawnIndex++];
            if(spawnIndex < waveInformation.Length)
                countdown = waveInformation[spawnIndex].spawnDelay;
            if(spawn.pathLine != null)
            {
                GameObject pathLine = Instantiate(spawn.pathLine, spawn.spawnPoint.position, Quaternion.identity);
                pathLine.GetComponent<PathLine>().SetPath(spawn.path);
            }
            StartCoroutine(SpawnEnemy(spawn.enemyPrefab, spawn.spawnPoint, spawn.path));
        }
    }

    public IEnumerator SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint, Waypoint[] path)
    {
        yield return new WaitForSeconds(2f);
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position + GetDisplacement(), spawnPoint.rotation);
        enemy.GetComponent<EnemyMovement>().SetPath(path);
        enemy.GetComponent<Enemy>().waveProgressText = waveProgressText;
        enemy.GetComponent<Enemy>().OnSpawn();
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoints[0].position, spawnPoints[0].rotation);
        enemy.GetComponent<EnemyMovement>().SetPath(Waypoints.GetPath(0));
        enemy.GetComponent<Enemy>().OnSpawn();
    }

    private Vector3 GetDisplacement()
    {
        return new Vector3((float)rand.NextDouble() * .4f * Constants.NODE_WIDTH - .2f * Constants.NODE_WIDTH, 0f, (float)rand.NextDouble() * .4f * Constants.NODE_WIDTH - .2f * Constants.NODE_WIDTH);
    }
}
