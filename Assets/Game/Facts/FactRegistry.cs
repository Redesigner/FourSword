using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Facts
{
    [CreateAssetMenu(fileName = "FactRegistry", menuName = "FactRegistry", order = 0)]
    public class FactRegistry : ScriptableObject
    {
        public Action onItemsChanged;
        
        [SerializedDictionary("Name", "Fact")]
        public SerializedDictionary<string, Fact> facts = new();

        public void CreateFact(string factName, Fact fact)
        {
            facts.Add(factName, fact);
            onItemsChanged.Invoke();
        }

        public void Reset()
        {
            facts.Clear();
            onItemsChanged.Invoke();
        }
    }
}