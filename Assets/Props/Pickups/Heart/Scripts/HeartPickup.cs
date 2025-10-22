using UnityEngine;

namespace Props.Pickups.Heart
{
    public class HeartPickup : Pickup
    {
        [SerializeField] [Min(0.0f)] private float healing = 5.0f;
        protected override void PlayerPickedUp(GameObject player)
        {
            var healthComponent = player.GetComponent<HealthComponent>();
            if (!healthComponent)
            {
                return;
            }
            
            healthComponent.Heal(healing);
            Destroy(gameObject);
        }
    }
}