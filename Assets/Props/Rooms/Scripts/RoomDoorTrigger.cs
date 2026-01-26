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
        [SerializeField] private bool lockable = false;

        [SerializeField] private Animator animator;
        
        private bool _active = false;
        private bool _locked = false;
        
        private static readonly int LockedAnimationBlend = Animator.StringToHash("Locked");

        private void Awake()
        {
            // This makes the door solid!
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

        public override void RoomLocked()
        {
            if (!lockable)
            {
                return;
            }
            
            _locked = true;
            doorTrigger.isTrigger = false;

            if (animator)
            {
                animator.SetBool(LockedAnimationBlend, true);
            }
        }

        public override void RoomUnlocked()
        {
            if (!lockable)
            {
                return;
            }
            
            _locked = false;
            
            if (_active)
            {
                doorTrigger.isTrigger = true;
            }
            
            if (animator)
            {
                animator.SetBool(LockedAnimationBlend, false);
            }
        }

        private void OnDrawGizmos()
        {
            Color doorColor;
            if (_locked || (lockable && !_active))
            {
                doorColor = new Color(0.7f, 0.0f, 0.7f, 0.6f);
            }
            else
            {
                doorColor = _active ? new Color(0.0f, 0.0f, 1.0f, 0.25f) : new Color(1.0f, 0.0f, 0.0f, 0.6f);
            }


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