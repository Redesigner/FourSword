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

        private void Awake()
        {
            doorTrigger.isTrigger = false;
        }

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
            doorTrigger.isTrigger = true;
        }

        public override void RoomExited()
        {
            _active = false;
            doorTrigger.isTrigger = false;
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
            const float doorCircleRadius = 0.4f;
            const float roomCircleRadius = 1.0f;
            
            if (roomToActivate)
            {
                var directionToRoom = (Vector2)(roomToActivate.transform.position - transform.position).normalized;
                DebugHelpers.Drawing.DrawCircle(transform.position, doorCircleRadius, new Color(0.0f, 0.0f, 1.0f, 0.2f));
                DebugHelpers.Drawing.DrawArrow(
                    transform.position + (Vector3)(directionToRoom * doorCircleRadius),
                    roomToActivate.transform.position - (Vector3)(directionToRoom * roomCircleRadius),
                    Color.blue
                );
                DebugHelpers.Drawing.DrawCircle(roomToActivate.transform.position, roomCircleRadius,
                    new Color(0.0f, 0.0f, 1.0f, 0.2f));

                Handles.Label(transform.position, $"To {roomToActivate.name}");
                Handles.Label(roomToActivate.transform.position, $"Destination:\n{roomToActivate.name}");
            }
            else
            {
                DebugHelpers.Drawing.DrawCircle(transform.position, doorCircleRadius, new Color(1.0f, 0.0f, 0.0f, 0.2f));
                var tempGUIStyle = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        textColor = Color.red
                    }
                };
                Handles.Label(transform.position, "No room destination set!", tempGUIStyle);
            }
        }
    }
}