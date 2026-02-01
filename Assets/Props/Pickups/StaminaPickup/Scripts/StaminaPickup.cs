using Characters.Player.Scripts;
using UnityEngine;

namespace Props.Pickups.Scripts
{
    public class StaminaPickup : Pickup
    {
        [SerializeField] private float staminaRestoreAmount = 5.0f;

        protected override void PlayerPickedUp(GameObject player)
        {
            var swordController = player.GetComponentInChildren<SwordAttackController>();
            if (!swordController)
            {
                return;
            }
            
            swordController.AddStamina(staminaRestoreAmount);
            Destroy(gameObject);
        }
    }
}