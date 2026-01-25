using System;
using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Props.Rooms.Scripts
{
    public class RoomSpawner : RoomObject
    {
        [SerializeField] private GameObject enemyType;
        [SerializeField] private uint numEnemies;
        [SerializeField] private Vector2 spawnAreaSize;
        [SerializeField] private UnityEvent allEnemiesDefeated;
        [SerializeField] private bool checkIfInNav = true;

        private List<WeakReference<GameObject>> _spawnedEnemies = new();
        
        private uint _numEnemiesDefeated;

        public override void RoomEntered()
        {
            SpawnEnemies();
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

        private void SpawnEnemies()
        {
            var spawnedObject = GetSpawnedObject();
            if (!spawnedObject)
            {
                return;
            }

            for (var i = _numEnemiesDefeated; i < numEnemies; ++i)
            {
                var newObject = Instantiate(spawnedObject, checkIfInNav ? GetSpawnLocationInNav(50) : GetSpawnLocation(), Quaternion.identity);
                var healthComponent = newObject.GetComponent<HealthComponent>();
                if (healthComponent)
                {
                    healthComponent.onDeath.AddListener(EnemyDefeated);
                }

                _spawnedEnemies.Add(new WeakReference<GameObject>(newObject));
            }
        }

        private Vector3 GetSpawnLocation()
        {
            return new Vector3(
                transform.position.x + spawnAreaSize.x * Random.value - spawnAreaSize.x * 0.5f,
                transform.position.y + spawnAreaSize.y * Random.value - spawnAreaSize.y * 0.5f,
                transform.position.z
            );
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

        private GameObject GetSpawnedObject()
        {
            return enemyType;
        }

        private void EnemyDefeated()
        {
            if (++_numEnemiesDefeated == numEnemies)
            {
                allEnemiesDefeated.Invoke();
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!enemyType)
            {
                return;
            }
            
            var style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = false;

            Handles.Label(transform.position, $"{enemyType.name}: {_numEnemiesDefeated}/{numEnemies}");
            DebugHelpers.Drawing.DrawBox(transform.position, spawnAreaSize, new Color(0.2f, 0.5f, 1.0f, 0.4f));
        }
    }
}