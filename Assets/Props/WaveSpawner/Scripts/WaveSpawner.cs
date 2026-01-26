using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Props.Scripts
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private List<WaveDefinition> waves;
        [SerializeField] [Min(0.0f)] private float waveRespawnTime = 1.0f;
        [SerializeField] [Min(0.0f)] private float interwaveRespawnTime = 0.5f;
        [SerializeField] [Min(0.0f)] private float respawnCircleRadius = 1.0f;

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

            var newObject = Instantiate(spawnedObject, (Vector3)Shared.Math.RandomPointInRadius(respawnCircleRadius) + transform.position, Quaternion.identity);
            if (++_spawnedEntityCount < waves[_waveIndex].enemiesThisWave)
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

            var waveRespawnTime = _waveTimer.GetRemainingTime();
            
            Handles.Label(transform.position,
                $"Wave: {_waveIndex + 1} / {waves.Count} {(waveRespawnTime < 0.0f ? "Active" : $"({waveRespawnTime:0.0} s)")}\n" +
                $"{waveLabel}\n", style);
            
            DebugHelpers.Drawing.DrawCircle(transform.position, respawnCircleRadius, new Color(0.2f, 0.5f, 1.0f, 0.1f));
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
    }
}