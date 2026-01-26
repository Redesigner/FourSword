using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Props.Rooms.Scripts
{
    public class RoomArea : MonoBehaviour
    {
        [SerializeField] private List<RoomObject> roomObjects;
        [SerializeField] private CinemachineCamera roomCamera;

        [SerializeField] private bool isActive;
        [SerializeField] private bool lockOnEnter = false;

        public void Start()
        {
            LoadRoomObjects();
            
            if (isActive)
            {
                GameState.instance.SetActiveRoom(this);
            }
        }

        public void StartTransition()
        {
            roomCamera.enabled = true;
            
            foreach (var roomObject in roomObjects)
            {
                roomObject.RoomTransitionStarted();
            }
        }
        
        public void EnterRoom()
        {
            Debug.LogWarningFormat("Room '{0}' entered.", name);
            foreach (var roomObject in roomObjects)
            {
                roomObject.RoomEntered();
            }

            if (lockOnEnter)
            {
                LockRoom();
            }
        }

        public void DeactivateRoom()
        {
            foreach (var roomObject in roomObjects)
            {
                roomObject.RoomExited();
            }

            roomCamera.enabled = false;
        }

        public void LockRoom()
        {
            lockOnEnter = false;
            foreach (var roomObject in roomObjects)
            {
                roomObject.RoomLocked();
            }
        }

        public void UnlockRoom()
        {
            foreach (var roomObject in roomObjects)
            {
                roomObject.RoomUnlocked();
            }
        }

        private void LoadRoomObjects()
        {
            foreach (var roomObject in GetComponentsInChildren<RoomObject>())
            {
                if (roomObjects.Contains(roomObject))
                {
                    continue;
                }
                
                Debug.LogFormat("Room '{0}' had child '{1}', but it was not in the list of room triggered objects. Automatically registering...", name, roomObject.name);
                roomObjects.Add(roomObject);
            }
        }

        private void OnDrawGizmos()
        {
            if (!isActive)
            {
                return;
            }
            
            DebugHelpers.Drawing.DrawBox(transform.position + new Vector3(0.0f, 1.25f, 0.0f), new Vector2(2.0f, 0.5f), new Color(0.0f, 0.0f, 1.0f, 0.2f));
            Handles.Label(transform.position + new Vector3(0.0f, 1.25f, 0.0f), "Active room");
        }

        private void OnDrawGizmosSelected()
        {
            
            DebugHelpers.Drawing.DrawCircle(transform.position, 1.0f, new Color(0.0f, 0.0f, 1.0f, 0.2f));
            Handles.Label(transform.position, $"Selected:\n{name}");
        }
    }
}