using System;
using UnityEngine;

namespace Props.Rooms.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomTrigger : RoomObject
    {
        [SerializeField] private RoomArea roomToActivate;
        
        private Collider2D _collider;
        private bool _active = false;
        
        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (roomToActivate)
            {
                GameState.instance.SetActiveRoom(roomToActivate);
            }
        }

        public override void RoomEntered()
        {
            _active = true;
            _collider.enabled = true;

            // Just for display purposes for now
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite)
            {
                sprite.enabled = true;
            }
        }

        public override void RoomExited()
        {
            _active = false;
            _collider.enabled = false;
            
            
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite)
            {
                sprite.enabled = false;
            }

        }
    }
}