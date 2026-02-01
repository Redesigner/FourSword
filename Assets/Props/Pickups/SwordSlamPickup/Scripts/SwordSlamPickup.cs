using Characters.Player.Scripts;
using Props.Pickups;
using UnityEngine;

public class SwordSlamPickup : Pickup
{
    protected override void PlayerPickedUp(GameObject player)
    {
        var swordController = player.GetComponentInChildren<SwordAttackController>();
        if (!swordController)
        {
            return;
        }

        swordController.slamLaunchesProjectile = true;
        Destroy(gameObject);
    }
}
