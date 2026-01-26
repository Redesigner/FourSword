using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Props.Scripts
{
    [Serializable]
    public class EnemySet
    {
        [SerializeField] public GameObject enemy;
        [SerializeField] [Min(0)] public int enemyCount;
        [SerializeField] public int enemiesSpawned;
    }
    
    [Serializable]
    public class WaveDefinition
    {
        [SerializeField] public List<EnemySet> enemyOptions;
        [SerializeField] public int enemiesThisWave;

        public int GetRandomObject()
        {
            var numObjects = enemyOptions.Count;

            if (numObjects <= 0)
            {
                return -1;
            }

            var unspawnedSets = GetUnspawnedSets();
            if (unspawnedSets.Count <= 0)
            {
                return -1;
            }
            
            return unspawnedSets[UnityEngine.Random.Range(0, unspawnedSets.Count)];
        }

        /// <summary>
        /// Gets a list of enemies that still have remaining units to spawn
        /// </summary>
        /// <returns></returns>
        private List<int> GetUnspawnedSets()
        {
            var result = new List<int>();
            for (var i = 0; i < enemyOptions.Count; ++i)
            {
                if (enemyOptions[i].enemiesSpawned < enemyOptions[i].enemyCount)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public int GetTotalEnemyCount()
        {
            return enemyOptions.Sum(enemyOption => enemyOption.enemyCount);
        }
    }
}