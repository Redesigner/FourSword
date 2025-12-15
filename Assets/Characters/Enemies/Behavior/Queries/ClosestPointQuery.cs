using System.Collections.Generic;
using System.Linq;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.Enemies.Behavior.Queries
{
    [CreateAssetMenu(fileName = "Q_ClosestPoint", menuName = "PositionQuery/ClosestPoint", order = 0)]
    public class ClosestPointQuery : PositionQuery
    {
        [SerializeField] [Min(0.0f)] private float radius;
        [SerializeField] [Min(1)] private int numPoints = 8;
        public override Vector3 RunQuery(GameObject self, KinematicCharacterController target)
        {
            return NavigationHelpers.GetClosestPointAroundRadius(self.transform.position, target.transform.position,
                radius, numPoints);
        }

        public override Vector3 RunQueryWithAllResults(GameObject self, KinematicCharacterController target, out List<PositionResult> results)
        {
            results = NavigationHelpers.GetLocationsInRadius(target.transform.position, radius, numPoints);
            results.RemoveAll(point => !NavigationHelpers.IsLocationInNavMesh(point.position));
            if (results.Count == 0)
            {
                return target.transform.position;
            }
            
            PositionResult.ScorePositionsByDistanceFromTarget(self.transform.position, results);
            results.Sort((a, b) => -a.score.CompareTo(b.score));

            return results.First().position;
        }
    }
}