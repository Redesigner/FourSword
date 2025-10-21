using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Game.StatusEffects
{
    [System.Serializable]
    public class StatusEffectContainer
    {
        private Dictionary<StatusEffect, List<StatusEffectInstance>> _statusEffects = new ();

        private Dictionary<StatusEffect, UnityEvent> _onStatusEffectAppliedMap = new();
        private Dictionary<StatusEffect, UnityEvent> _onStatusEffectRemovedMap = new();
        private Dictionary<StatusEffect, UnityEvent<int, float>> _onStatusEffectStacksChangedMap = new();

        public void ApplyStatusEffectInstance(StatusEffectInstance instance)
        {
            if (_statusEffects.TryGetValue(instance.effect, out var effectList))
            {
                effectList.Add(instance);
            }
            else
            {
                // Create the stack in our dictionary, and invoke the event for a new stack here
                // if the event exists. Assume no one has subscribed if the event does not exist
                effectList = new List<StatusEffectInstance> { instance };
                _statusEffects.Add(instance.effect, effectList);
                if (_onStatusEffectAppliedMap.TryGetValue(instance.effect, out var effectEvent))
                {
                    effectEvent.Invoke();
                }
            }

            if (_onStatusEffectStacksChangedMap.TryGetValue(instance.effect, out var changedEvent))
            {
                changedEvent.Invoke(effectList.Count, Accumulate(instance.effect, effectList));
            }
        }

        public void RemoveStatusEffectInstance(StatusEffectInstance instance)
        {
            if (!_statusEffects.TryGetValue(instance.effect, out var effectList))
            {
                return;
            }

            if (!effectList.Remove(instance))
            {
                return;
            }
            
            if (_onStatusEffectStacksChangedMap.TryGetValue(instance.effect, out var changedEvent))
            {
                changedEvent.Invoke(effectList.Count, Accumulate(instance.effect, effectList));
            }

            if (effectList.Count != 0) return;
            {
                _statusEffects.Remove(instance.effect);
                if (_onStatusEffectRemovedMap.TryGetValue(instance.effect, out var effectEvent))
                {
                    effectEvent.Invoke();
                }
            }
        }

        public void Update(float deltaSeconds)
        {
            var effectsToRemove = new List<StatusEffect>();
            foreach (var stackList in _statusEffects)
            {
                var numRemoved = stackList.Value.RemoveAll(effectInstance => effectInstance.UpdateCheckExpiration(deltaSeconds));
                if (numRemoved > 0 && _onStatusEffectStacksChangedMap.TryGetValue(stackList.Key, out var changedEvent))
                {
                    changedEvent.Invoke(stackList.Value.Count, Accumulate(stackList.Key, stackList.Value));
                }
                
                if (stackList.Value.Count == 0)
                {
                    effectsToRemove.Add(stackList.Key);
                }
            }

            foreach (var effectToRemove in effectsToRemove)
            {
                if (_onStatusEffectRemovedMap.TryGetValue(effectToRemove, out var effectEvent))
                {
                    effectEvent.Invoke();
                }
                _statusEffects.Remove(effectToRemove);
            }
        }

        public override string ToString()
        {
            var result = new string("");

            return _statusEffects.Aggregate(result, (current, statusEffect)
                => current + $"{statusEffect.Key.name}: '{statusEffect.Value.Count}'");
        }

        public Dictionary<StatusEffect, List<StatusEffectInstance>>.Enumerator GetEnumerator()
        {
            return _statusEffects.GetEnumerator();
        }

        public UnityEvent GetEffectAppliedEvent(StatusEffect effect)
        {
            if (_onStatusEffectAppliedMap.TryGetValue(effect, out var effectEvent))
            {
                return effectEvent;
            }

            var newEvent = new UnityEvent();
            _onStatusEffectAppliedMap.Add(effect, newEvent);
            return newEvent;
        }
        
        public UnityEvent GetEffectRemovedEvent(StatusEffect effect)
        {
            if (_onStatusEffectRemovedMap.TryGetValue(effect, out var effectEvent))
            {
                return effectEvent;
            }

            var newEvent = new UnityEvent();
            _onStatusEffectRemovedMap.Add(effect, newEvent);
            return newEvent;
        }
        
        public UnityEvent<int, float> GetEffectStacksChangedEvent(StatusEffect effect)
        {
            if (_onStatusEffectStacksChangedMap.TryGetValue(effect, out var effectEvent))
            {
                return effectEvent;
            }

            var newEvent = new UnityEvent<int, float>();
            _onStatusEffectStacksChangedMap.Add(effect, newEvent);
            return newEvent;
        }
        

        public void WindowFunction(int windowId)
        {
            var rect = new Rect(0, 10, 100, 100);
            foreach (var statusEffect in _statusEffects)
            {
                GUI.Label(rect, $"{statusEffect.Key.effectName} : {statusEffect.Value.Count}");
            }
        }

        public float Accumulate(StatusEffect statusEffect, List<StatusEffectInstance> instances)
        {
            if (instances.Count == 0)
            {
                return 0.0f;
            }
            
            switch (statusEffect.accumulator)
            {
                default:
                case EffectAccumulator.None:
                    return 0.0f;
                
                case EffectAccumulator.Additive:
                    return instances.Sum(instance => instance.strength);
                
                case EffectAccumulator.Maximum:
                    return instances.Max(instance => instance.strength);
                
                case EffectAccumulator.Minimum:
                    return instances.Min(instance => instance.strength);
                
                case EffectAccumulator.Multiplicative:
                    var total = instances.Aggregate(1.0f, (current, instance) => current * instance.strength);
                    return total;
            }
        }

        public bool HasEffect(StatusEffect effect)
        {
            return _statusEffects.ContainsKey(effect);
        }
    }
}