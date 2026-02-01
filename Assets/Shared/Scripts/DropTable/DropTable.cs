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
        [SerializeField] [Range(0.0f, 1.0f)] public float dropWeight;

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

        public GameObject GetDrop()
        {
            var randomWeight = Random.value;
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
        
        private void OnEnable()
        {
            if (drops.Sum(entry => entry.dropWeight) > 1.0f)
            {
                Debug.LogWarningFormat("[DropTable] The total drop rate in '{0}' is more than 1! Some items might not be dropped.", name);
            }
        }
    }
}