using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Props.Rooms.Scripts
{
    public class RoomArea : MonoBehaviour
    {
        [SerializeField] private List<RoomObject> roomObjects;
        [SerializeField] private CinemachineCamera roomCamera;

        [SerializeField] private bool isActive;

        public void Start()
        {
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
            Debug.LogFormat("Room '{0}' entered.", name);
            foreach (var roomObject in roomObjects)
            {
                roomObject.RoomEntered();
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
}
}