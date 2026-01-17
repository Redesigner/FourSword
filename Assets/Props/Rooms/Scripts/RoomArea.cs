using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Props.Rooms.Scripts
{
    public class RoomArea : MonoBehaviour
    {
        [SerializeField] private List<RoomObject> roomObjects;

        public void ActivateRoom()
        {
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
        }
}
}