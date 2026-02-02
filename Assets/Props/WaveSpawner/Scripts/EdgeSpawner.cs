using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using Characters.Player.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class EdgeSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 spawnAreaSizeInner; 
    [SerializeField] private Vector2 spawnAreaSizeOuter; 
    
    [SerializeField] [Range(1, 50)]
    public int waveDifficulty = 1;
    
    [SerializeField] [Range(1, 50)]
    public int waveDuration;

    private PlayerController _playerCenter;
    
    private int _waveValue;
    
    private float _waveTimer;
    private float _spawnInterval;
    private float _spawnTimer;
 
    public List<Enemy> enemies = new();
    public List<GameObject> enemiesToSpawn = new();
    
    // Start is called before the first frame update
    private void Start()
    {
        _playerCenter = GameState.instance.activePlayer;
        GenerateNewWave();
    }
 
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (_spawnTimer <= 0)
        {
            //spawn an enemy
            if(enemiesToSpawn.Count > 0)
            {
                var position = GetSpawnLocationInNav();
                var enemy = Instantiate(enemiesToSpawn[0], position,Quaternion.identity); // spawn first enemy in our list
                
                enemiesToSpawn.RemoveAt(0); // and remove it
                _spawnTimer = _spawnInterval;
            }
            else
            {
                _waveTimer = 0; // if no enemies remain, end wave
            }
        }
        else
        {
            _spawnTimer -= Time.fixedDeltaTime;
            _waveTimer -= Time.fixedDeltaTime;
        }

        if (!(_waveTimer <= 0))
        {
            return;
        }
        waveDifficulty++;
        GenerateNewWave();
    }

    private void GenerateNewWave()
    {
        _waveValue = waveDifficulty * 10;
        GenerateEnemies();
 
        _spawnInterval = waveDuration / (float)enemiesToSpawn.Count; // gives a fixed time between each enemy
        _waveTimer = waveDuration;
    }

    private void GenerateEnemies()
    {
        // Create a temporary list of enemies to generate
        // 
        // in a loop grab a random enemy 
        // see if we can afford it
        // if we can, add it to our list, and deduct the cost.
 
        // repeat... 
 
        //  -> if we have no points left, leave the loop
        
        
        var generatedEnemies = new List<GameObject>();
        while(_waveValue > 0 || generatedEnemies.Count < 50)
        {
            var randEnemyId = GetRandomAffordableEnemy();
            if (randEnemyId == -1)
            {
                break;
            }
            var randEnemyCost = enemies[randEnemyId].cost;
            generatedEnemies.Add(enemies[randEnemyId].enemyPrefab);
            _waveValue -= randEnemyCost;
                
            if(_waveValue <= 0)
            {
                break;
            }
        }
        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
        var waveString = enemies.Aggregate("", (current, enemy) => current + $"{generatedEnemies.Count(prefab => prefab == enemy.enemyPrefab)} {DebugHelpers.Names.GetNameSafe(enemy.enemyPrefab)}s, ");

        Debug.LogFormat("Generated wave with {0}", waveString);
    }

    private Vector3 GetSpawnLocation()
    {
        var regionSize = spawnAreaSizeOuter - spawnAreaSizeInner;
        var areaTopBottom = spawnAreaSizeOuter.x * regionSize.y / 2.0f;
        var areaLeftRight = regionSize.x / 2.0f * spawnAreaSizeInner.y;

        var totalArea = areaTopBottom + areaLeftRight;
        var chooseRegion = Random.value * totalArea;

        var randomOffset = new Vector2();
        
        if (chooseRegion <= areaTopBottom) // use the top or bottom region
        {
            var sign = Random.Range(0, 2) * 2 - 1;
            randomOffset.y = Random.Range(spawnAreaSizeInner.y, spawnAreaSizeOuter.y) / 2.0f * sign;
            randomOffset.x = Random.Range(0.0f, spawnAreaSizeOuter.x) - spawnAreaSizeOuter.x / 2.0f;
        }
        else // use left or right
        {
            var sign = Random.Range(0, 2) * 2 - 1;
            randomOffset.y = Random.Range(0.0f, spawnAreaSizeInner.y) - spawnAreaSizeInner.y / 2.0f;
            randomOffset.x = Random.Range(spawnAreaSizeInner.x, spawnAreaSizeInner.x) / 2.0f * sign;
        }
        
        return GetPlayerCenter() + (Vector3)randomOffset;
    }

    private Vector3 GetPlayerCenter()
    {
        return _playerCenter ? _playerCenter.transform.position : transform.position;
    }
    
    private Vector3 GetSpawnLocationInNav(int maxIterations = 20)
    {
        for (var i = 0; i < maxIterations; ++i)
        {
            var spawnLocation = GetSpawnLocation();
            if (NavigationHelpers.IsLocationInNavMesh(spawnLocation))
            {
                return spawnLocation;
            }
        }

        return transform.position;
    }

    private int GetRandomEnemyIndexWeighted()
    {
        var enemyTotalCost = enemies.Aggregate(0.0f, (current, enemy) => current + enemy.GetRandomChance());
        var randomValue = Random.value * enemyTotalCost;

        for (var i = 0; i < enemies.Count; ++i)
        {
            var randomChance = enemies[i].GetRandomChance();
            if (randomValue < randomChance)
            {
                return i;
            }

            randomValue -= randomChance;
        }

        return 0;
    }

    private int GetRandomAffordableEnemy()
    {
        var affordableEnemies = new List<int>();

        for(var i = 0; i < enemies.Count; ++i)
        {
            if (enemies[i].cost < _waveValue)
            {
                affordableEnemies.Add(i);
            }
        }

        if (affordableEnemies.Count == 0)
        {
            return -1;
        }

        return affordableEnemies[Random.Range(0, affordableEnemies.Count)];
    }

    private void OnDrawGizmos()
    {
        var regionSize = spawnAreaSizeOuter - spawnAreaSizeInner;
        var origin = _playerCenter ? _playerCenter.transform.position : transform.position;
        var color = new Color(0.2f, 0.5f, 1.0f, 0.4f);
        
        // Draw top rect
        DebugHelpers.Drawing.DrawBox(
            new Vector3(origin.x, origin.y + (spawnAreaSizeOuter.y / 2.0f) - (regionSize.y / 4.0f), transform.position.z),
            new Vector2(spawnAreaSizeOuter.x, regionSize.y / 2.0f), color);
        // Bottom
        DebugHelpers.Drawing.DrawBox(
            new Vector3(origin.x, origin.y - (spawnAreaSizeOuter.y / 2.0f) + (regionSize.y / 4.0f), transform.position.z), 
            new Vector2(spawnAreaSizeOuter.x, regionSize.y / 2.0f), color);
        // Left
        DebugHelpers.Drawing.DrawBox(
            new Vector3(origin.x + (spawnAreaSizeOuter.x / 2.0f) - (regionSize.x / 4.0f), origin.y, transform.position.z),
            new Vector2(regionSize.x / 2.0f, spawnAreaSizeInner.y), color);
        // Right
        DebugHelpers.Drawing.DrawBox(
            new Vector3(origin.x - (spawnAreaSizeOuter.x / 2.0f) + (regionSize.x / 4.0f), origin.y, transform.position.z), 
            new Vector2(regionSize.x / 2.0f, spawnAreaSizeInner.y), color);
    }
}
 
[System.Serializable]
public class Enemy
{
    public GameObject enemyPrefab;
    public int cost;

    public float GetRandomChance()
    {
        var result = cost > 0.0f ? 1.0f / cost : 0.0f;
        return result;
    }
}
