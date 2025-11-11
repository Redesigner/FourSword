using System;
using Game.Facts;
using UnityEngine;

namespace Props.Pickups
{
    public class SinglePickup : Pickup
    {
        [SerializeField]
        private FlagName itemFlagName;

        private void Awake()
        {
            if (!GameState.instance.factState.TryGetFlag(itemFlagName.name, out var flag))
            {
                return;
            }
            
            if (flag)
            {
                Destroy(gameObject);
            }
        }

        protected override void PlayerPickedUp(GameObject player)
        {
            GameState.instance.factState.WriteFlag(itemFlagName.name, true);
            Destroy(gameObject);
        }
    }
}