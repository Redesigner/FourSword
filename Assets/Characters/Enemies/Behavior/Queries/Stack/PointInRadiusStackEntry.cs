using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "QSE_PointInRadius", menuName = "PositionQuery/Stack/PointInRadius")]
    public class PointInRadiusStackEntry : PositionQueryStackEntry
    {
        [SerializeField] [Min(0.0f)] private float minRadius;
        [SerializeField] [Min(0.0f)] private float maxRadius;
        [SerializeField] [Min(1)] private int maxIterations = 10;
        
        public override void Evaluate(ref List<PositionResult> results, KinematicCharacterController self, KinematicCharacterController other)
        {
            results = results.Select(result => new PositionResult(RandomPoint(result.position), result.score)).ToList();
        }

        private Vector3 RandomPoint(Vector3 center)
        {
            for (var i = 0; i < maxIterations; ++i)
            {
                var result = NavigationHelpers.GetRandomPointInRadius(center, maxRadius, minRadius);
                if (NavigationHelpers.IsLocationInNavMesh(result))
                {
                    return result;
                }
            }

            return center;
        }
    }
}