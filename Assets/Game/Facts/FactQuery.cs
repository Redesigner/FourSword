using System;
using UnityEngine;

namespace Game.Facts
{
    public enum FactQueryType
    {
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
    }
    
    [Serializable]
    public struct FactQuery
    {
        public string factName;
        public FactQueryType type;
        
        [SerializeReference]
        public FactVariant comparison;
    }
}