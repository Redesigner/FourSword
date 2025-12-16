using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "QSE_NavMesh", menuName = "PositionQuery/Stack/NavMesh")]
    public class NavMeshStackEntry : PositionQueryStackEntry
    {
        public override void Evaluate(ref List<PositionResult> results, KinematicCharacterController self, KinematicCharacterController other)
        {
            results.RemoveAll((result) => !NavigationHelpers.IsLocationInNavMesh(result.position));
        }
    }
}