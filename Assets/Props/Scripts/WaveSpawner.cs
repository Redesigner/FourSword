using System;
using UnityEditor;
using UnityEngine;

namespace Props.Scripts
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject spawnedObject;
        [SerializeField] private int maxSpawnedObjects;
        [SerializeField] [Min(0.0f)] private float waveRespawnTime = 1.0f;
        [SerializeField] [Min(0.0f)] private float interwaveRespawnTime = 0.5f;
        [SerializeField] [Min(0.0f)] private float respawnCircleRadius = 1.0f;
        
        private int _spawnedEntityCount = 0;
        private TimerHandle _interwaveRespawnTimer;

        private void Start()
        {
            SpawnObject();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void SpawnObject()
        {
            if (!spawnedObject)
            {
                return;
            }

            var newObject = Instantiate(spawnedObject, (Vector3)Shared.Math.RandomPointInRadius(respawnCircleRadius) + transform.position, Quaternion.identity);
            if (++_spawnedEntityCount < maxSpawnedObjects)
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
            Handles.Label(transform.position, 
                $"{(spawnedObject ? spawnedObject.name : "null")}\n" +
                $"{_spawnedEntityCount} / {maxSpawnedObjects}", style);
            
            DebugHelpers.Drawing.DrawCircle(transform.position, respawnCircleRadius, new Color(0.2f, 0.5f, 1.0f, 0.25f));
        }

        private void SpawnedObjectDestroyed()
        {
            --_spawnedEntityCount;
        }
    }
}