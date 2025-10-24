using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Facts
{
    [CreateAssetMenu(fileName = "FactRegistry", menuName = "FactRegistry", order = 0)]
    public class FactRegistry : ScriptableObject
    {
        [SerializedDictionary("Item Type", "Weight")]
        public SerializedDictionary<string, Fact> facts = new();
    }
}