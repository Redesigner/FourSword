using System;
using UnityEngine;

namespace Game.Facts
{
    interface IVariantHolder
    {
        public bool IsType<T>();

        object Get();

        void Set(object value);
    }

    class VariantHolder<T> : IVariantHolder
    {
        private T _item;
        public bool IsType<TU>() => typeof(TU) == typeof(T);

        public object Get() => _item;

        public void Set(object newValue)
        {
            _item = (T)newValue;
        }
        
        public VariantHolder(T item) => this._item = item;
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

        public void Set<T>(T newValue)
        {
            _variant.Set(newValue);
        }
    }
}