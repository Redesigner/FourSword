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

        public UnityEvent<StatusEffect> onStatusEffectApplied = new();
        public UnityEvent<StatusEffect> onStatusEffectRemoved = new();

        public void ApplyStatusEffectInstance(StatusEffectInstance instance)
        {
            if (_statusEffects.TryGetValue(instance.effect, out var effectList))
            {
                effectList.Add(instance);
                return;
            }
            
            _statusEffects.Add(instance.effect, new List<StatusEffectInstance>{instance});
            onStatusEffectApplied.Invoke(instance.effect);
        }

        public void RemoveStatusEffectInstance(StatusEffectInstance instance)
        {
            if (!_statusEffects.TryGetValue(instance.effect, out var effectList))
            {
                return;
            }

            if (!effectList.Remove(instance) || effectList.Count > 0)
            {
                return;
            }
            
            _statusEffects.Remove(instance.effect);
            onStatusEffectRemoved.Invoke(instance.effect);
        }

        public void Update(float deltaSeconds)
        {
            var effectsToRemove = new List<StatusEffect>();
            foreach (var stackList in _statusEffects)
            {
                stackList.Value.RemoveAll(effectInstance => effectInstance.UpdateCheckExpiration(deltaSeconds));
                if (stackList.Value.Count == 0)
                {
                    effectsToRemove.Add(stackList.Key);
                }
            }

            foreach (var effectToRemove in effectsToRemove)
            {
                onStatusEffectRemoved.Invoke(effectToRemove);
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

        public void WindowFunction(int windowId)
        {
            var rect = new Rect(0, 10, 100, 100);
            foreach (var statusEffect in _statusEffects)
            {
                GUI.Label(rect, $"{statusEffect.Key.effectName} : {statusEffect.Value.Count}");
            }
        }
    }
}