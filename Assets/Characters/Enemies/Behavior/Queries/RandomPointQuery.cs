using System.Collections.Generic;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "Q_RandomPoint", menuName = "PositionQuery/RandomPoint")]
    public class RandomPointQuery : PositionQuery
    {
        [SerializeField] [Min(0.0f)] private float minRadius;
        [SerializeField] [Min(0.0f)] private float maxRadius;
        [SerializeField] [Min(1)] private int maxIterations = 10;
        public override Vector3 RunQuery(GameObject self, KinematicCharacterController target)
        {
            for (var i = 0; i < maxIterations; ++i)
            {
                var result = NavigationHelpers.GetRandomPointInRadius(self.transform.position, maxRadius, minRadius);
                if (NavigationHelpers.IsLocationInNavMesh(result))
                {
                    return result;
                }
            }

            return self.transform.position;
        }

        public override Vector3 RunQueryWithAllResults(GameObject self, KinematicCharacterController target, out List<PositionResult> results)
        {
            var result = RunQuery(self, target);
            results = new List<PositionResult> { new(result, 1.0f) };
            return result;
        }
    }
}