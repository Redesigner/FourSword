using System;
using System.Collections.Generic;
using Game.StatusEffects;
using UnityEngine;

namespace Props.Scripts
{
    public class EffectApplierTrigger : MonoBehaviour
    {

        [SerializeField] private StatusEffect effect;
        [SerializeField] private float effectStrength;

        private readonly Dictionary<HealthComponent, StatusEffectInstance> _appliedEffects = new();
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            var healthComponent = other.transform.root.GetComponent<HealthComponent>();
            if (!healthComponent)
            {
                return;
            }

            if (_appliedEffects.ContainsKey(healthComponent))
            {
                return;
            }

            var effectInstance = new StatusEffectInstance(effect, this, 0.0f, effectStrength);
            healthComponent.statusEffects.ApplyStatusEffectInstance(effectInstance);
            
            _appliedEffects.Add(healthComponent, effectInstance);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var healthComponent = other.transform.root.GetComponent<HealthComponent>();
            if (!healthComponent)
            {
                return;
            }

            if (!_appliedEffects.TryGetValue(healthComponent, out var appliedInstance))
            {
                return;
            }
            
            healthComponent.statusEffects.RemoveStatusEffectInstance(appliedInstance);
            _appliedEffects.Remove(healthComponent);
        }
    }
}