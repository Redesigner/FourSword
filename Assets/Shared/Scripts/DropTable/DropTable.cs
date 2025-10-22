using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Shared.DropTable
{
    [Serializable]
    public struct DropTableEntry
    {
        [SerializeField] public GameObject drop;
        [SerializeField] [Min(0.0f)] public float dropWeight;

        public DropTableEntry(GameObject drop, float dropWeight)
        {
            this.drop = drop;
            this.dropWeight = dropWeight;
        }
    }
    
    [CreateAssetMenu(fileName = "Dt_DropTable", menuName = "Drop Table", order = 0)]
    public class DropTable : ScriptableObject
    {
        [SerializeField] private List<DropTableEntry> drops;
        private float _totalWeight;

        public GameObject GetDrop()
        {
            var randomWeight = _totalWeight * Random.value;
            var currentWeight = randomWeight;
            foreach (var entry in drops)
            {
                if (entry.dropWeight > currentWeight)
                {
                    return entry.drop;
                }

                currentWeight -= entry.dropWeight;
            }

            return null;
        }
        
        private void OnValidate()
        {
            _totalWeight = drops.Sum(entry => entry.dropWeight);
        }

        private void OnEnable()
        {
            _totalWeight = drops.Sum(entry => entry.dropWeight);
        }
    }
}