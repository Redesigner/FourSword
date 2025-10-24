using System;

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
        Flag,
        Numeric
    }
    
    [Serializable]
    public struct Fact
    {
        private IVariantHolder _variant;
        public readonly FactType type;
        

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
    }
}