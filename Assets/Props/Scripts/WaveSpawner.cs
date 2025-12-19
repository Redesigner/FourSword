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

        private void SpawnObject()
        {
            
        }

        private void OnDrawGizmos()
        {
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = false;
            Handles.Label(transform.position, $"{_spawnedEntityCount} / {maxSpawnedObjects}", style);
            
            DebugHelpers.Drawing.DrawCircle(transform.position, respawnCircleRadius, new Color(0.2f, 0.5f, 1.0f, 0.25f));
        }
    }
}