using System;
using UnityEngine;

namespace Game.Facts
{
    interface IVariantHolder
    {
        public bool IsType<T>();

        object Get();
    }

    class VariantHolder<T> : IVariantHolder
    {
        public T item { get; }
        public bool IsType<TU>() => typeof(TU) == typeof(T);

        public object Get() => item;
        
        public VariantHolder(T item) => this.item = item;
    }
    
    public enum FactType
    {
        Flag = 0,
        Numeric = 1
    }
    
    [Serializable]
    public class Fact
    {
        private IVariantHolder _variant;
        
        [field: SerializeField]
        public FactType type;

        public Fact(bool value)
        {
            _variant = new VariantHolder<bool>(value);
            type = FactType.Flag;
        }

        public Fact(int value)
        {
            _variant = new VariantHolder<int>(value);
            type = FactType.Numeric;
        }

        public Fact()
        {
            _variant = new VariantHolder<bool>(true);
            type = FactType.Flag;
        }

        public T Get<T>()
        {
            return (T)_variant.Get();
        }
    }
}