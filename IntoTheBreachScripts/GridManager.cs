using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public static Dictionary<DIRECTION, Vector2> DIRECTION_VECTORS = new Dictionary<DIRECTION, Vector2>() { { DIRECTION.DOWN, new Vector2(0,-1)},{ DIRECTION.LEFT, new Vector2(-1, 0) },{ DIRECTION.UP, new Vector2(0, 1) },{ DIRECTION.RIGHT, new Vector2(1, 0) } };
    public static Dictionary<DIRECTION, int> DIRECTION_ROTATIONS = new Dictionary<DIRECTION, int>() { {DIRECTION.DOWN, 0}, {DIRECTION.RIGHT, 90}, {DIRECTION.UP, 180}, {DIRECTION.LEFT, 270} };
    public static Dictionary<char, Tile> tilePrefabsByKeycode = new Dictionary<char, Tile>();

    [SerializeField] private float tileScale;
    [SerializeField] private TextMeshProUGUI tilePositionDisplay;
    [SerializeField] private TextMeshProUGUI occupantDisplay;
    [SerializeField] private TextMeshProUGUI tileDescriptionDisplay;
    [SerializeField] private int width, height;
    [SerializeField] private Tile[] tilePrefabs;
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Logger loggerPrefab;
    [SerializeField] private Gunner gunnerPrefab;
    [SerializeField] private Hero mechaPrefab, tankPrefab, artilleryPrefab;
    [SerializeField] private Color TraversableColor, OccupiableColor;
    [SerializeField] private LineRenderer playerMovementLineRenderer;
    [SerializeField] private Transform _camera;
    [SerializeField] private GameObject arrowPrefab;

    private Dictionary<Vector2, Tile> tiles;
    private List<Hero> heroes = new List<Hero>();
    private List<Enemy> enemies = new List<Enemy>();
    private List<GameObject> enemyTargetingIcons = new List<GameObject>();
    private Hero heroToSpawn;
    private int heroSpawnIndex = 0;
    private List<Tile> enemySpawnTiles = new List<Tile>();
    private List<TextAsset> mapFiles = new List<TextAsset>();

    public Tile selectedTile;
    public Character selectedCharacter;

    private void Awake()
    {
        Instance = this;

        heroes.Add(Instantiate(mechaPrefab, new Vector3(500, 500, 0), Quaternion.identity));
        heroes.Add(Instantiate(tankPrefab, new Vector3(500, 500, 0), Quaternion.identity));
        heroes.Add(Instantiate(artilleryPrefab, new Vector3(500, 500, 0), Quaternion.identity));

        foreach(Tile tile in tilePrefabs)
        {
            tilePrefabsByKeycode.Add(tile.keyCode, tile);
        }

        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private async void GameManager_OnGameStateChanged(GAME_STATE oldState, GAME_STATE newState)
    {
        switch (newState)
        {
            case GAME_STATE.PREPARING_MAP:
                if (mapFiles.Count == 0) RestockMapDeck();
                int index = Random.Range(0, mapFiles.Count);
                GenerateGrid(mapFiles[index]);
                mapFiles.RemoveAt(index);
                GameManager.Instance.UpdateTurnCount(5);
                break;
            case GAME_STATE.SPAWN_ENEMIES:
                SpawnEnemies(GameManager.Instance.initialEnemyCount);
                break;
            case GAME_STATE.SPAWN_HEROES:
                SelectHeroToSpawn();
                break;
            case GAME_STATE.PLAYER_TURN_UPKEEP:
                OnPlayerTurnUpkeep();
                break;
            case GAME_STATE.PLAYER_TURN:
                EndTileHighlightingForMovement();
                break;
            case GAME_STATE.PLAYER_TURN_HERO_SELECTED:
                if(((Hero)selectedCharacter).CanMove)
                {
                    //HighlightTiles(GetReachableTiles());
                }
                break;
            case GAME_STATE.PLAYER_TURN_ENDSTEP:
                StartCoroutine(OnPlayerTurnEndstep());
                break;
            case GAME_STATE.ENEMY_TURN:
                await EnemyTurn();
                break;
            case GAME_STATE.ENEMY_TURN_ENDSTEP:
                break;
            case GAME_STATE.GAME_END:
                OnGameEnd();
                break;
            case GAME_STATE.CLEANUP:
                OnCleanup();
                break;
        }
    }

    #region PREPARING_MAP

    private void GenerateGrid(TextAsset gridTextAsset)
    {
        string gridText = gridTextAsset.text;
        string[] lines = Regex.Split(gridText, "\r\n");
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < 8; x++)
        {
            string line = lines[x];
            for (int y = 0; y < line.Length; y++)
            {
                Tile tilePrefab = tilePrefabsByKeycode[lines[7 - y][x]];
                Tile tile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                bool canEnemySpawnHere = lines[16-y][x] == 'O';
                string tileName = "Tile (" + x + "," + y + ")";
                bool isOffset = (x + y) % 2 == 1;
                tile.Init(isOffset, canEnemySpawnHere, tileName, tilePositionDisplay, occupantDisplay, tileDescriptionDisplay);

                tiles.Add(new Vector2(x, y), tile);
            }
        }

        GameManager.Instance.UpdateGameState(GAME_STATE.SPAWN_ENEMIES);
    }

    /*private void GenerateGrid(TextAsset gridTextAsset)
    {
        string gridText = gridTextAsset.text;
        string[] lines = Regex.Split(gridText, "\r\n");
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < lines.Length; x++)
        {
            string line = lines[x];
            for (int y = 0; y < line.Length; y++)
            {
                Tile tilePrefab = tilePrefabsByKeycode[lines[7 - y][x]];
                Tile tile = Instantiate(tilePrefab, new Vector2((x-y) * tileScale / 2f, (x+y) * tileScale / 4f), Quaternion.identity);
                tile.name = "Tile (" + x + "," + y + ")";
                bool isOffset = (x + y) % 2 == 1;
                tile.Init(isOffset, tilePositionDisplay, occupantDisplay);

                tiles.Add(new Vector2((x - y) * tileScale / 2f, (x + y) * tileScale / 4f), tile);
            }
        }

        GameManager.Instance.UpdateGameState(GAME_STATE.SPAWN_ENEMIES);
    }*/

    #endregion PREPARING_MAP

    #region SPAWN_ENEMIES

    private void SpawnEnemies(int numberOfEnemiesToSpawn)
    {
        List<Tile> possibleSpawnTiles = new List<Tile>();
        foreach (Tile tile in tiles.Values)
        {
            if (tile.CanSpawnEnemyHere()) possibleSpawnTiles.Add(tile);
        }

        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            Tile spawnTile = possibleSpawnTiles[Random.Range(0, possibleSpawnTiles.Count)];
            possibleSpawnTiles.Remove(spawnTile);
            SpawnEnemy(spawnTile);
        }

        GameManager.Instance.UpdateGameState(GAME_STATE.SPAWN_HEROES);
    }

    private void SpawnEnemy(Tile spawnTile)
    {
        Enemy enemy = Instantiate(enemyPrefabs[Random.Range(0,2)], spawnTile.transform.position, Quaternion.identity);
        spawnTile.occupant = enemy;
        enemies.Add(enemy);
        spawnTile.occupant.occupiedTile = spawnTile;
    }

    #endregion SPAWN_ENEMIES

    #region SPAWN_HEROES

    private void SelectHeroToSpawn()
    {
        heroToSpawn = heroes[heroSpawnIndex++];
    }

    public Hero GetHeroToSpawn()
    {
        return heroToSpawn;
    }

    public void SpawnHero(Tile tile)
    {
        heroToSpawn.transform.position = tile.transform.position;
        tile.occupant = heroToSpawn;
        heroToSpawn.occupiedTile = tile;

        if (heroSpawnIndex >= 3)
            GameManager.Instance.UpdateGameState(GAME_STATE.ENEMY_TURN);
        else
            SelectHeroToSpawn();
    }

    #endregion SPAWN_HEROES

    #region PLAYER_TURN_UPKEEP

    private async void OnPlayerTurnUpkeep()
    {
        List<Task> upkeepTasks = new List<Task>();
        foreach (Hero hero in heroes)
        {
            upkeepTasks.Add(hero.Upkeep());
        }
        await Task.WhenAll(upkeepTasks);
        GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN);
    }

    #endregion PLAYER_TURN_UPKEEP

    #region PLAYER_TURN_ENDSTEP

    private IEnumerator OnPlayerTurnEndstep()
    {
        //Wait for falling enemies to die?
        while (!enemies.TrueForAll(e => e.IsReadyForTurnEnd()))
        {
            yield return null;
        }

        GameManager.Instance.UpdateGameState(GAME_STATE.ENEMY_TURN);
    }

    #endregion

    #region ENEMY_TURN

    private async Task EnemyTurn()
    {
        EnvironmentEffectsPreMove();
        await EnemyMoves();
        EnvironmentEffectsPostMove();
        GameManager.Instance.DecrementTurnCount();
    }

    private void EnvironmentEffectsPreMove()
    {
        foreach (Enemy enemy in enemies.ToList())
        {
            if (enemy.occupiedTile.IsBurning()) enemy.SetAflame();
            if (enemy.isBurning && !enemy.isFlameproof) enemy.TakeDamage(1, DAMAGE_TYPE.FIRE);
        }
    }

    private async Task EnemyMoves()
    {
        foreach (Enemy enemy in enemies.ToList())
        {
            if (enemy.attackPrepared)
            {
                SelectTile(enemy.occupiedTile);
                await Task.Delay(500);
                await enemy.Attack();
                DisplayAttackingEnemiesTelegraphy();
            }
        }

        
        foreach (Tile tile in enemySpawnTiles.ToList())
        {
            await Task.Delay(500);
            if (tile.occupant != null)
            {
                tile.occupant.TakeDamage(1, DAMAGE_TYPE.BURROW);
            }
            else
            {
                SpawnEnemy(tile);
                tile.DisableSpawncracks();
                enemySpawnTiles.Remove(tile);
            }
        }


        if (GameManager.Instance.GetTurnCount() > 1)
        {
            foreach (Enemy enemy in enemies)
            {
                SelectTile(enemy.occupiedTile);
                await Task.Delay(200);
                List<Tile> tilesInMovementRange = GetReachableTilesByPath(enemy).Keys.ToList().Where(t => t.IsOccupiable(enemy)).ToList(); ;
                List<Tile> tilesInTargetRange = new List<Tile>();
                Dictionary<Tile, List<Tile>> tilesByListsOfTilesThatCanHitThatTile = new Dictionary<Tile, List<Tile>>();

                foreach (Tile movementTile in tilesInMovementRange)
                {
                    List<Tile> targetableTiles = enemy.GetTilesTargetableFromGivenTile(movementTile);
                    foreach (Tile targetableTile in targetableTiles)
                    {
                        List<Tile> tilesThatCanHitThatTile;
                        if (tilesByListsOfTilesThatCanHitThatTile.ContainsKey(targetableTile))
                            tilesThatCanHitThatTile = tilesByListsOfTilesThatCanHitThatTile[targetableTile];
                        else
                            tilesThatCanHitThatTile = new List<Tile>();

                        tilesThatCanHitThatTile.Add(movementTile);
                        tilesByListsOfTilesThatCanHitThatTile[targetableTile] = tilesThatCanHitThatTile;
                        if (!tilesInTargetRange.Contains(targetableTile)) tilesInTargetRange.Add(targetableTile);
                    }
                }

                List<Tile> priorityTargetTiles = tilesInTargetRange.FindAll(t => t.keyCode == 'B' || t.keyCode == 'b' || t.occupant != null && t.occupant.faction == FACTION.PLAYER);

                if (priorityTargetTiles.Count == 0) priorityTargetTiles.Add(tilesInTargetRange[Random.Range(0, tilesInTargetRange.Count)]);
                Tile targetTile = priorityTargetTiles[Random.Range(0, priorityTargetTiles.Count)];
                enemy.targetTile = targetTile;

                Tile tileToMoveTo = tilesByListsOfTilesThatCanHitThatTile[targetTile][Random.Range(0, tilesByListsOfTilesThatCanHitThatTile[targetTile].Count)];

                if (tileToMoveTo != null) await enemy.Move(enemy.occupiedTile, tileToMoveTo);

                enemy.attackPrepared = true;
                DisplayAttackingEnemiesTelegraphy();
                enemy.LoseSelection();
            }
        }
    }

    private void EnvironmentEffectsPostMove()
    {
        if (GameManager.Instance.GetTurnCount() > 2)
        {
            List<Tile> possibleSpawnTiles = new List<Tile>();
            foreach (Tile tile in tiles.Values)
            {
                if (tile.CanSpawnEnemyHere()) possibleSpawnTiles.Add(tile);
            }

            while (enemies.Count + enemySpawnTiles.Count < GameManager.Instance.desiredEnemyCount)
            {
                Tile newSpawnTile = possibleSpawnTiles[Random.Range(0, possibleSpawnTiles.Count)];
                possibleSpawnTiles.Remove(newSpawnTile);
                enemySpawnTiles.Add(newSpawnTile);
                newSpawnTile.EnableSpawncracks();
            }
        }

        GameManager.Instance.UpdateGameState(GAME_STATE.PLAYER_TURN_UPKEEP);
    }

    #endregion

    #region GAME_END

    private void OnGameEnd()
    {
        foreach(Tile tile in tiles.Values)
        {
            tile.EndAllHighlights();
            tile.UnThreaten();
            tile.DisableSpawncracks();
        }

        foreach(Enemy enemy in enemies.ToList())
        {
            enemy.EndDisplayDetailedAttackTelegraphy();
            enemy.Die();
        }
    }

    #endregion

    #region CLEANUP

    private void OnCleanup()
    {
        foreach(Hero hero in heroes)
        {
            hero.Refresh();
            hero.transform.position = new Vector3(500, 500, 0);
        }
        heroSpawnIndex = 0;

        foreach(Tile tile in tiles.Values)
        {
            Destroy(tile.gameObject);
        }

        GameManager.Instance.UpdateGameState(GAME_STATE.PREPARING_MAP);
    }

    #endregion

    #region GET_TILES

    public List<Tile> GetReachableTiles(Character character)
    {
        return GetReachableTilesByPath(character).Keys.ToList<Tile>();
    }

    public Dictionary<Tile, List<Tile>> GetReachableTilesByPath(Character character)
    {
        Dictionary<int, List<Tile>> tilesReachableInXMoves = new Dictionary<int, List<Tile>>();
        Dictionary<Tile, List<Tile>> reachableTilesByPath = new Dictionary<Tile, List<Tile>>();
        Tile originTile = character.occupiedTile;

        List<Tile> reachableTiles = new List<Tile>();
        reachableTiles.Add(originTile);
        tilesReachableInXMoves.Add(0, reachableTiles);

        List<Tile> path = new List<Tile>();
        Tile potentialTile = originTile;
        path.Add(potentialTile);
        reachableTilesByPath.Add(originTile, path);

        for (int i = 1; i <= character.movement; i++)
        {
            reachableTiles = new List<Tile>();

            foreach (Tile tileInRange in tilesReachableInXMoves[i - 1])
            {
                foreach (Vector2 directionVector in DIRECTION_VECTORS.Values)
                {
                    path = reachableTilesByPath[tileInRange].ToList();
                    potentialTile = GetTileAtPos(new Vector2(tileInRange.transform.position.x + directionVector.x, tileInRange.transform.position.y + directionVector.y));
                    if (potentialTile != null && potentialTile.IsTraversable(character) && !reachableTilesByPath.ContainsKey(potentialTile) && potentialTile != originTile)
                    {
                        reachableTiles.Add(potentialTile);
                        path.Add(potentialTile);
                        reachableTilesByPath.Add(potentialTile, path);
                    }
                }
            }

            tilesReachableInXMoves.Add(i, reachableTiles);
        }

        return reachableTilesByPath;
    }

    public List<Tile> GetAdjacentTiles(Tile originTile)
    {
        List<Tile> tilesInRange = new List<Tile>();

        foreach (DIRECTION direction in DIRECTION_VECTORS.Keys.ToList())
        {
            Tile tile = GetTileInDirection(originTile, direction, 1);
            if (tile != null)
                tilesInRange.Add(tile);
        }

        return tilesInRange;
    }

    public List<Tile> GetTilesForAttackBasicMelee(Tile originTile)
    {
        return GetAdjacentTiles(originTile);
    }

    public List<Tile> GetTilesForAttackBasicProjectile(Tile originTile)
    {
        List<Tile> tilesInRange = new List<Tile>();

        foreach (DIRECTION direction in DIRECTION_VECTORS.Keys.ToList())
        {
            int magnitude = 1;
            Tile tile = null;
            do
            {
                tile = GetTileInDirection(originTile, direction, magnitude++);
                if (tile != null)
                    tilesInRange.Add(tile);
            } while (tile != null && !tile.IsBlocker());
        }

        return tilesInRange;
    }

    public List<Tile> GetTilesForAttackBasicArtillery(Tile originTile)
    {
        List<Tile> tilesInRange = new List<Tile>();

        foreach (DIRECTION direction in DIRECTION_VECTORS.Keys.ToList())
        {
            int magnitude = 1;
            Tile tile = null;
            do
            {
                tile = GetTileInDirection(originTile, direction, magnitude++);
                if (tile != null)
                    tilesInRange.Add(tile);
            } while (tile != null);
        }

        return tilesInRange;
    }

    public List<Tile> GetTilesForAttackNonAdjacentArtillery(Tile originTile)
    {
        List<Tile> tilesInRange = new List<Tile>();

        foreach (DIRECTION direction in DIRECTION_VECTORS.Keys.ToList())
        {
            int magnitude = 2;
            Tile tile = null;
            do
            {
                tile = GetTileInDirection(originTile, direction, magnitude++);
                if (tile != null)
                    tilesInRange.Add(tile);
            } while (tile != null);
        }

        return tilesInRange;
    }

    public List<Tile> GetTilesInDirection(Tile originTile, DIRECTION direction)
    {
        List<Tile> tiles = new List<Tile>();
        int magnitude = 1;
        Tile tile = null;
        do
        {
            tile = GetTileInDirection(originTile, direction, magnitude++);
            if (tile != null)
                tiles.Add(tile);
        } while (tile != null);

        return tiles;
    }

    public Tile GetTileInDirection(Tile originTile, DIRECTION direction, int magnitude)
    {
        Vector2 directionVector = DIRECTION_VECTORS[direction];

        float x = originTile.transform.position.x + directionVector.x * magnitude;
        float y = originTile.transform.position.y + directionVector.y * magnitude;

        Tile tile = GetTileAtPos(new Vector2(x, y));
        return tile;
    }

    #endregion

    #region HIGHLIGHTING

    public void HighlightPathToTile(Tile destinationTile, Dictionary<Tile, List<Tile>> reachableTilesByPath)
    {
        List<Tile> tilesInMovementRange = reachableTilesByPath.Keys.ToList();
        if (selectedCharacter.CanMove && tilesInMovementRange.Contains(destinationTile))
        {
            HighlightTilesForMovement(tilesInMovementRange);


            int pointsPerSegment = 100;
            int pathLength = reachableTilesByPath[destinationTile].Count;
            int positionCount = (pathLength - 1) * pointsPerSegment;
            Vector3[] positions = new Vector3[positionCount];

            playerMovementLineRenderer.enabled = true;
            playerMovementLineRenderer.positionCount = positionCount;

            for (int x = 1; x < pathLength; x++)
            {
                Vector3 segmentStart = reachableTilesByPath[destinationTile][x - 1].transform.position;
                Vector3 segmentEnd = reachableTilesByPath[destinationTile][x].transform.position;
                for (int y = 0; y < pointsPerSegment; y++)
                {
                    positions[(x - 1) * pointsPerSegment + y] = segmentStart + (segmentEnd - segmentStart) * (y / (pointsPerSegment - 1f));
                }
            }
            playerMovementLineRenderer.SetPositions(positions);
        }
    }

    public void DisplayAttackingEnemiesTelegraphy()
    {
        EndTileHighlightingForAttacks();
        enemyTargetingIcons.ForEach(x => Destroy(x));
        enemyTargetingIcons.Clear();

        foreach (Enemy enemy in enemies)
        {
            if (enemy.attackPrepared)
            {
                GameObject targetingIcon = enemy.Telegraph();
                enemyTargetingIcons.Add(targetingIcon);
            }
        }
    }

    public void HighlightTilesForMovement(List<Tile> tiles, Color color)
    {
        EndTileHighlightingForMovement();
        foreach (Tile tile in tiles)
        {
            tile.FullHighlight(color);
        }
    }

    public void HighlightTilesForMovement(List<Tile> tiles)
    {
        EndTileHighlightingForMovement();
        foreach (Tile tile in tiles)
        {
            tile.FullHighlight(TraversableColor);
        }
    }

    public void EndTileHighlightingForMovement(List<Tile> tiles)
    {
        playerMovementLineRenderer.enabled = false;
        foreach (Tile tile in tiles)
        {
            tile.EndFullHighlight();
        }
    }

    public void EndTileHighlightingForMovement()
    {
        EndTileHighlightingForMovement(tiles.Values.ToList());
    }

    public void EndTileHighlightingForAttacks()
    {
        foreach (Tile tile in tiles.Values.ToList())
        {
            tile.EndPartialHighlight();
        }
    }

    #endregion

    public void Shift(Character target, DIRECTION direction)
    {
        Tile newTile = GetTileInDirection(target.occupiedTile, direction, 1);

        if(newTile != null)
        {
            if (newTile.IsBlocker())
            {
                target.SetShiftPosition(newTile, true);
            }
            else
            {
                target.occupiedTile.occupant = null;
                target.occupiedTile = newTile;
                newTile.occupant = target;

                if(target.faction == FACTION.ENEMY)
                {
                    ((Enemy)target).targetTile = GetTileInDirection(((Enemy)target).targetTile, direction, 1);
                    DisplayAttackingEnemiesTelegraphy();
                }

                target.SetShiftPosition(newTile, false);
            }
        }
    }

    public void SelectTile(Tile tile)
    {
        if (selectedCharacter != null) selectedCharacter.LoseSelection();

        selectedTile = tile;
        selectedCharacter = tile.occupant;
        selectedCharacter.Select();
    }

    public void DeselectTile()
    {
        if (selectedCharacter != null) selectedCharacter.LoseSelection();
        selectedCharacter = null;
        selectedTile = null;
    }

    public bool AreAllHeroesOutOfActions()
    {
        return heroes.TrueForAll(h => h.hasMoved && h.hasActed || h.hitpoints == 0);
    }

    public Tile GetTileAtPos(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out Tile tile))
            return tile;

        return null;
    }

    public DIRECTION GetDirectionBetween(Tile originTile, Tile targetTile)
    {
        Vector2 directionVector = targetTile.transform.position - originTile.transform.position;
        directionVector.Normalize();

        foreach(KeyValuePair<DIRECTION,Vector2> pair in DIRECTION_VECTORS)
        {
            if (directionVector == pair.Value)
                return pair.Key;
        }
        throw new System.Exception("GetDirectionBetween only works for orthogonal cells");
    }

    public Tile GetTileInProjectilePath(Tile originTile, DIRECTION direction)
    {
        Tile targetedTile = originTile;
        Tile nextTile = GetTileInDirection(targetedTile, direction, 1);
        while (nextTile != null)
        {
            targetedTile = nextTile;
            nextTile = GetTileInDirection(targetedTile, direction, 1);
            if (targetedTile.IsBlocker()) break;
        }

        return targetedTile;
    }

    public void RemoveEnemyFromList(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    private void RestockMapDeck()
    {
        Object[] files = Resources.LoadAll("Mapfiles", typeof(TextAsset));
        foreach (Object file in files)
        {
            mapFiles.Add((TextAsset)file);
        }
    }
}

public enum DIRECTION
{
    DOWN,
    RIGHT,
    UP,
    LEFT
}