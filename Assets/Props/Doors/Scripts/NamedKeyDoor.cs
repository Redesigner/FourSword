using System;
using Game.Facts;
using UnityEngine;

namespace Props.Doors.Scripts
{
    public class NamedKeyDoor : MonoBehaviour
    {
        [SerializeField] private FlagName keyFlagName;
        [SerializeField] private FlagName doorFlagName;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var root = other.transform.root.gameObject;
            if (!root.CompareTag("Player"))
            {
                return;
            }

            if (!GameState.instance.factState.TryGetFlag(keyFlagName.name, out var keyFlag))
            {
                return;
            }

            if (keyFlag)
            {
                OpenDoor();
            }
        }

        private void OpenDoor()
        {
            GameState.instance.factState.WriteFlag(doorFlagName.name, true);
            Destroy(gameObject);
        }

        private void Awake()
        {
            if (!GameState.instance.factState.TryGetFlag(doorFlagName.name, out var doorFlag))
            {
                return;
            }

            if (!doorFlag)
            {
                return;
            }
            
            Destroy(gameObject);
        }
    }
}