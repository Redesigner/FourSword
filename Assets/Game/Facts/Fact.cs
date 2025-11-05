using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeReference]
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
        [SerializeReference]
        private IVariantHolder variant;
        
        [SerializeField]
        public FactType type;

        public Fact()
        {
            variant = new VariantHolder<bool>(true);
            type = FactType.Flag;
        }

        public Fact(bool value)
        {
            variant = new VariantHolder<bool>(value);
            type = FactType.Flag;
        }

        public Fact(int value)
        {
            variant = new VariantHolder<int>(value);
            type = FactType.Numeric;
        }

        public T Get<T>()
        {
            return (T)variant.Get();
        }

        public void Set<T>(T newValue)
        {
            variant.Set(newValue);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", type);
            switch (type)
            {
                case FactType.Flag:
                    info.AddValue("value", Get<bool>());
                    break;
                case FactType.Numeric:
                    info.AddValue("value", Get<int>());
                    break;
                default:
                    break;
            }
        }

        public Fact(SerializationInfo info, StreamingContext context)
        {
            type = (FactType)info.GetValue("type", typeof(FactType));
            variant = type switch
            {
                FactType.Flag => new VariantHolder<bool>(info.GetBoolean("value")),
                FactType.Numeric => new VariantHolder<int>(info.GetInt32("value")),
                _ => new VariantHolder<bool>(false)
            };
        }
    }
}