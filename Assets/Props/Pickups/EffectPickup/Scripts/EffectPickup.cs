using Game.StatusEffects;
using UnityEngine;

namespace Props.Pickups.EffectPickup.Scripts
{
    public class EffectPickup : Pickup
    {
        [SerializeField] private StatusEffect effect;
        [SerializeField] private float effectStrength;
        [SerializeField] [Min(0.0f)] private float effectDuration = 0.0f;

        protected override void PlayerPickedUp(GameObject player)
        {
            var healthComponent = player.GetComponent<HealthComponent>();
            if (!healthComponent)
            {
                return;
            }

            var effectInstance = new StatusEffectInstance(effect, this, effectDuration, effectStrength);
            healthComponent.statusEffects.ApplyStatusEffectInstance(effectInstance);
            
            Destroy(gameObject);
        }
    }
}