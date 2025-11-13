using System;
using Game.Facts;
using UnityEngine;

namespace Shared.Facts
{
    public class FactQuerySpawner : MonoBehaviour
    {
        [SerializeField] private FactQuery query;
        [SerializeField] private GameObject spawnedPrefab;


        private void Awake()
        {
            if (!GameState.instance.factState.RunQuery(query))
            {
                return;
            }
            
            if (spawnedPrefab)
            {
                Instantiate(spawnedPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}