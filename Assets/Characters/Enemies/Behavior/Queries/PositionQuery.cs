using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    public abstract class PositionQuery : ScriptableObject
    {
        public abstract Vector3 RunQuery(GameObject self, KinematicCharacterController target);

        public abstract Vector3 RunQueryWithAllResults(GameObject self, KinematicCharacterController target,
            out List<PositionResult> results);
    }
}