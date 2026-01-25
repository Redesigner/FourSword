using UnityEngine;

namespace Props.Rooms.Scripts
{
    public abstract class RoomObject: MonoBehaviour
    {
        public virtual void RoomTransitionStarted() {}

        public virtual void RoomEntered() {}

        public virtual void RoomExited() {}
    }
}