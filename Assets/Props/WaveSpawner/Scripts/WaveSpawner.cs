using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using Props.Rooms.Scripts;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Props.Scripts
{
    public class WaveSpawner : RoomObject
    {
        [SerializeField] private List<WaveDefinition> waves;
        [SerializeField] [Min(0.0f)] private float waveRespawnTime = 1.0f;
        [SerializeField] [Min(0.0f)] private float interwaveRespawnTime = 0.5f;
        [SerializeField] private Vector2 spawnAreaSize;
        [SerializeField] private bool checkIfInNav = true;

        private int _waveIndex = 0;
        private int _spawnedEntityCount = 0;
        private int _defeatedEntityCount = 0;
        private TimerHandle _interwaveRespawnTimer;
        private TimerHandle _waveTimer;

        private readonly List<WeakReference<GameObject>> _spawnedEnemies = new();

        public override void RoomEntered()
        {
            SpawnObject();
        }
        
        public override void RoomExited()
        {
            foreach (var enemyRef in _spawnedEnemies)
            {
                if (!enemyRef.TryGetTarget(out var enemy))
                {
                    continue;
                }
                
                Destroy(enemy);
            }
            _spawnedEnemies.Clear();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void SpawnObject()
        {
            var spawnedIndex = GetCurrentWave().GetRandomObject();
            if (spawnedIndex < 0)
            {
                return;
            }
            
            ++GetCurrentWave().enemyOptions[spawnedIndex].enemiesSpawned;
            
            var enemyToSpawn = GetCurrentWave().enemyOptions[spawnedIndex].enemy;
            if (!enemyToSpawn)
            {
                return;
            }

            var newEnemy = Instantiate(enemyToSpawn, GetSpawnLocation(), Quaternion.identity);
            _spawnedEnemies.Add(new WeakReference<GameObject>(newEnemy));
            if (++_spawnedEntityCount < waves[_waveIndex].GetTotalEnemyCount())
            {
                _interwaveRespawnTimer = TimerManager.instance.CreateTimer(this, interwaveRespawnTime, SpawnObject);
            }

            var healthComponent = newEnemy.GetComponent<HealthComponent>();
            if (healthComponent)
            {
                healthComponent.onDeath.AddListener(SpawnedObjectDestroyed);
            }

        }

        private void OnDrawGizmos()
        {
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = false;

            var currentWave = GetCurrentWave();
            if (currentWave == null)
            {
                Handles.Label(transform.position, "No current wave set.");
                return;
            }

            var waveLabel = currentWave.enemyOptions.Aggregate("", (current, enemySet) => current + $"{DebugHelpers.Names.GetNameSafe(enemySet.enemy)} : {enemySet.enemiesSpawned} / {enemySet.enemyCount}\n");

            var remainingTime = _interwaveRespawnTimer.GetRemainingTime();
            if (remainingTime < 0.0f)
            {
                remainingTime = 0.0f;
            }
            var remainingWaveTime = _waveTimer.GetRemainingTime();
            
            Handles.Label(transform.position,
                $"Wave: {_waveIndex + 1} / {waves.Count} {(remainingWaveTime < 0.0f ? "Active" : $"({remainingWaveTime:0.0} s)")}\n" +
                $"{waveLabel}\n" +
                $"Defeated: {_defeatedEntityCount} / {GetMaxThisWave()}", style);
            
            DebugHelpers.Drawing.DrawBox(transform.position, spawnAreaSize, new Color(0.2f, 0.5f, 1.0f, 0.4f));
        }

        private void SpawnedObjectDestroyed()
        {
            if (++_defeatedEntityCount < GetMaxThisWave())
            {
                return;
            }
            
            if (_waveIndex < waves.Count - 1)
            {
                _waveTimer = TimerManager.instance.CreateTimer(this, waveRespawnTime, () =>
                {
                    ++_waveIndex;
                    _defeatedEntityCount = 0;
                    _spawnedEntityCount = 0;
                    SpawnObject();
                });
            }
        }

        private WaveDefinition GetCurrentWave()
        {
            if (_waveIndex < 0 || _waveIndex >= waves.Count)
            {
                return null;
            }
            
            return waves[_waveIndex];
        }

        private int GetMaxThisWave()
        {
            return GetCurrentWave().GetTotalEnemyCount();
        }

        private Vector3 GetSpawnLocation()
        {
            return checkIfInNav ? GetSpawnLocationInNav(50) : GetRandomSpawnLocation();
        }

        private Vector3 GetSpawnLocationInNav(int maxIterations)
        {
            for (var i = 0; i < maxIterations; ++i)
            {
                var spawnLocation = GetRandomSpawnLocation();
                if (NavigationHelpers.IsLocationInNavMesh(spawnLocation))
                {
                    return spawnLocation;
                }
            }

            return transform.position;
        }

        private Vector3 GetRandomSpawnLocation()
        {
            return new Vector3(
                transform.position.x + spawnAreaSize.x * Random.value - spawnAreaSize.x * 0.5f,
                transform.position.y + spawnAreaSize.y * Random.value - spawnAreaSize.y * 0.5f,
                transform.position.z
            );
        }
    }
}