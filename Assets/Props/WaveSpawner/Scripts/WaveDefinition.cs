using System;
using System.Collections.Generic;
using UnityEngine;

namespace Props.Scripts
{
    [Serializable]
    public struct WaveDefinition
    {
        [SerializeField] public List<GameObject> spawnedObjectOptions;
        [SerializeField] public int enemiesThisWave;

        public GameObject GetRandomObject()
        {
            var numObjects = spawnedObjectOptions.Count;

            if (numObjects <= 0)
            {
                return null;
            }
            
            return numObjects == 1 ? spawnedObjectOptions[0] : spawnedObjectOptions[UnityEngine.Random.Range(0, numObjects)];
        }
    }
}