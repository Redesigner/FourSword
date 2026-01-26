using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Props.Scripts
{
    public class WaveSpawner : MonoBehaviour
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

        private void Start()
        {
            SpawnObject();
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
            
            var spawnedObject = GetCurrentWave().enemyOptions[spawnedIndex].enemy;
            if (!spawnedObject)
            {
                return;
            }

            var newObject = Instantiate(spawnedObject, GetSpawnLocation(), Quaternion.identity);
            if (++_spawnedEntityCount < waves[_waveIndex].GetTotalEnemyCount())
            {
                _interwaveRespawnTimer = TimerManager.instance.CreateTimer(this, interwaveRespawnTime, SpawnObject);
            }

            var healthComponent = newObject.GetComponent<HealthComponent>();
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

            var waveLabel = GetCurrentWave().enemyOptions.Aggregate("", (current, enemySet) => current + $"{DebugHelpers.Names.GetNameSafe(enemySet.enemy)} : {enemySet.enemiesSpawned} / {enemySet.enemyCount}\n");

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