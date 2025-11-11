using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Facts
{
    public enum FactChangeType
    {
        Assignment,
        Increment,
        Decrement
    }
    
    [Serializable]
    public class FactChange
    {
        public string factName;
        public FactChangeType type;
        
        [FormerlySerializedAs("comparison")] [SerializeReference]
        public FactVariant assignment;
    }
}