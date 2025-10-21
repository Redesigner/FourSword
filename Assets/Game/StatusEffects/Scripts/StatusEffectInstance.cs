using System;
using UnityEngine;

namespace Game.StatusEffects
{
    public class StatusEffectInstance : IEquatable<StatusEffectInstance>
    {
        public StatusEffectInstance(StatusEffect effect, MonoBehaviour applier, float duration = 0.0f, float strength = 0.0f)
        {
            this.effect = effect;
            this.applier = applier;
            this.duration = duration;
            this.strength = strength;
            currentTime = 0.0f;
        }
        
        public readonly StatusEffect effect;
        public readonly MonoBehaviour applier;
        public readonly float duration;
        public readonly float strength;
        public float currentTime { private set; get; }

        public bool Equals(StatusEffectInstance other)
        {
            return other != null && Equals(effect, other.effect) && Equals(applier, other.applier);
        }

        public override bool Equals(object obj)
        {
            return obj is StatusEffectInstance other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(effect, applier);
        }

        /// <summary>
        /// Update this instance, like updating a timer
        /// This should only be called from inside the container
        /// </summary>
        /// <param name="deltaSeconds"></param>
        /// <returns>true if this instance has expired, false if it is still valid</returns>
        public bool UpdateCheckExpiration(float deltaSeconds)
        {
            if (duration == 0.0f)
            {
                return false;
            }

            currentTime += deltaSeconds;
            return currentTime > duration;
        }
    }
}