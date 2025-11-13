using System;
using UnityEngine;

namespace Game.Facts
{
    [Serializable]
    public struct Fact : IEquatable<Fact>
    {
        [SerializeReference] public FactVariant data;
        [SerializeField] public string name;

        public Fact(string name, bool value)
        {
            data = new FactVariant(value);
            this.name = name;
        }

        public Fact(string name, int value)
        {
            data = new FactVariant(value);
            this.name = name;
        }

        public bool Equals(Fact other)
        {
            return Equals(data, other.data) && name == other.name;
        }

        public override bool Equals(object obj)
        {
            return obj is Fact other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(data, name);
        }

        public void Rename(string newName)
        {
            name = newName;
        }
    }
}