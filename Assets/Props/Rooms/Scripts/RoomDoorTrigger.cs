using System;
using Characters.Player.Scripts;
using UnityEngine;

namespace Props.Rooms.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomDoorTrigger : RoomObject
    {
        [SerializeField] private RoomDoorTrigger doorDestination;
        [SerializeField] private RoomArea roomToActivate;

        [SerializeField] private BoxCollider2D doorTrigger;
        private bool _active = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (roomToActivate && _active)
            {
                GameState.instance.SetActiveRoom(roomToActivate, doorDestination);
            }
        }
        

        public override void RoomEntered()
        {
            _active = true;

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
            
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite)
            {
                sprite.enabled = false;
            }

        }

        private void OnDrawGizmos()
        {
            var doorColor = _active ? new Color(0.0f, 0.0f, 1.0f, 0.25f) : new Color(1.0f, 0.0f, 0.0f, 0.25f);
            
            if (doorTrigger)
            {
                DebugHelpers.Drawing.DrawBoxCollider2D(doorTrigger, doorColor);
            }

            if (doorDestination)
            {
                DebugHelpers.Drawing.DrawArrow(transform.position, doorDestination.transform.position, doorColor);
            }
        }
    }
}