using System;

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
        public Fact comparison;
    }
}