using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShop : MonoBehaviour
{
    private WaveSpawner waveSpawner;
    private Prefabs enemyPrefabs;

    private void Start()
    {
        waveSpawner = WaveSpawner.instance;
        enemyPrefabs = Prefabs.instance;
    }

    public void SpawnBadguy()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.enemy);
    }

    public void SpawnSmolBadguy()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.smolEnemy);
    }

    public void SpawnOriginiumSlug()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.originiumSlug);
    }

    public void SpawnSoldier()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.soldier);
    }

    public void SpawnCaster()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.caster);
    }

    public void SpawnHeavyDefender()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.heavyDefender);
    }

    public void SpawnAvenger()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.avenger);
    }

    public void SpawnCorruptedKnight()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.corruptedKnight);
    }

    public void SpawnCocktailThrower()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.cocktailThrower);
    }

    public void SpawnLogger()
    {
        waveSpawner.SpawnEnemy(enemyPrefabs.logger);
    }
}
