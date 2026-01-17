using UnityEngine;

namespace Props.Rooms.Scripts
{
    public abstract class RoomObject: MonoBehaviour
    {
        public abstract void RoomEntered();

        public abstract void RoomExited();
    }
}