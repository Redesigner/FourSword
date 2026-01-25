using System;
using Characters.Player.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            if (!other.transform.root.CompareTag("Player"))
            {
                return;
            }
            
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
            var doorColor = _active ? new Color(0.0f, 0.0f, 1.0f, 0.25f) : new Color(1.0f, 0.0f, 0.0f, 0.6f);
            
            if (doorTrigger)
            {
                DebugHelpers.Drawing.DrawBoxCollider2D(doorTrigger, doorColor);
            }

            if (doorDestination)
            {
                DebugHelpers.Drawing.DrawArrow(transform.position, doorDestination.transform.position, doorColor);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!roomToActivate)
            {
                return;
            }

            const float doorCircleRadius = 0.4f;
            const float roomCircleRadius = 1.0f;
            var directionToRoom = (Vector2)(roomToActivate.transform.position - transform.position).normalized;
            Handles.Label(transform.position, $"To {roomToActivate.name}");
            DebugHelpers.Drawing.DrawArrow(
                transform.position + (Vector3)(directionToRoom * doorCircleRadius),
                roomToActivate.transform.position - (Vector3)(directionToRoom * roomCircleRadius),
                Color.blue
            );
            DebugHelpers.Drawing.DrawCircle(transform.position, doorCircleRadius, new Color(0.0f, 0.0f, 1.0f, 0.2f));
            DebugHelpers.Drawing.DrawCircle(roomToActivate.transform.position, roomCircleRadius, new Color(0.0f, 0.0f, 1.0f, 0.2f));
            Handles.Label(roomToActivate.transform.position, $"Destination:\n{roomToActivate.name}");
        }
    }
}