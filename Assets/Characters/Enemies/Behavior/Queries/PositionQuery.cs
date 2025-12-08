using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    public abstract class PositionQuery : ScriptableObject
    {
        public abstract Vector3 RunQuery(GameObject self, KinematicCharacterController target);
    }
}