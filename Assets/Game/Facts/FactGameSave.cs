using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Facts
{
    [Serializable]
    public struct IntFact
    {
        public int value;
        public string name;
    }

    [Serializable]
    public struct BoolFact
    {
        public bool value;
        public string name;
    }
    
    [Serializable]
    public class FactGameSave
    {
        [SerializeField] public List<IntFact> numerics;
        [SerializeField] public List<BoolFact> flags;
    }
}