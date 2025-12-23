using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "QSE_ClosestToSelf", menuName = "PositionQuery/Stack/ClosestToSelf")]
    public class ClosestToSelfStackEntry : PositionQueryStackEntry
    {
        public override void Evaluate(ref List<PositionResult> results, KinematicCharacterController self, KinematicCharacterController other)
        {
            PositionResult.ScorePositionsByDistanceFromTarget(self.transform.position, results);
        }
    }
}