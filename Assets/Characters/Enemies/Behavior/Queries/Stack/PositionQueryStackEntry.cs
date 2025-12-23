using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    public abstract class PositionQueryStackEntry : ScriptableObject
    {
        public abstract void Evaluate(ref List<PositionResult> results, KinematicCharacterController self,
            KinematicCharacterController other);
    }
}