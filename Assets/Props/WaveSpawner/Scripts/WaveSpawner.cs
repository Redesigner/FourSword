using System;
using System.Collections.Generic;
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
            var spawnedObject = GetSpawnedObject();
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

            var objectsThisWaveLabel = "";
            foreach (var spawnedObject in GetCurrentWave().spawnedObjectOptions)
            {
                objectsThisWaveLabel += spawnedObject.name;
                objectsThisWaveLabel += ", ";
            }

            var remainingTime = _interwaveRespawnTimer.GetRemainingTime();
            if (remainingTime < 0.0f)
            {
                remainingTime = 0.0f;
            }

            var waveRespawnTime = _waveTimer.GetRemainingTime();
            
            Handles.Label(transform.position,
                $"Wave: {_waveIndex + 1} / {waves.Count} {(waveRespawnTime < 0.0f ? "Active" : $"({waveRespawnTime:0.0} s)")}\n" +
                $"Spawned: {_spawnedEntityCount} / {GetMaxThisWave()} ({remainingTime:0.0} s)\n" +
                $"Defeated {_defeatedEntityCount}\n" +
                $"{objectsThisWaveLabel}\n", style);
            
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

        private GameObject GetSpawnedObject()
        {
            return GetCurrentWave().GetRandomObject();
        }

        private int GetMaxThisWave()
        {
            return GetCurrentWave().enemiesThisWave;
        }
    }
}